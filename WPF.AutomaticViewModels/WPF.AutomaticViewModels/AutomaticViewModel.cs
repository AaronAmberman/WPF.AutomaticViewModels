using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace WPF.AutomaticViewModels
{
    /// <summary>
    /// An object that can add property change notifications to any object it wraps.
    /// <para>
    /// How it works...
    /// </para>
    /// <para>
    /// This type reads the public properties of the wrapped object that have setters (need 
    /// to be able to set the value). Which will then be available as properties on this 
    /// dynamic object. These properties will have built in change notifications that push the 
    /// value back to the wrapped object.
    /// </para>
    /// <para>
    /// That being said, kind of important that the developer know the properties they want 
    /// to access because there is no automatic completion for dynamic objects.
    /// </para>
    /// <para>
    /// New properties, methods or events cannot be added to this type. The collection of 
    /// properties is enforced and this object is immutable. The values are mutable, they need 
    /// to be in order to automatically update the UI.
    /// </para>
    /// </summary>
    public class AutomaticViewModel : DynamicObject, INotifyPropertyChanged
    {
        #region Fields

        private List<PropertyInfo> properties;
        private Dictionary<string, object> remappedProperties;
        private object wrapped;

        #endregion

        #region Events

        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="AutomaticViewModel"/> class.</summary>
        /// <param name="objectToWrap">The object to wrap with property changes.</param>
        /// <exception cref="ArgumentNullException">objectToWrap is null.</exception>
        public AutomaticViewModel(object objectToWrap)
            : this(objectToWrap, BindingFlags.Public | BindingFlags.Instance)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AutomaticViewModel"/> class.</summary>
        /// <param name="objectToWrap">The object to wrap with property changes.</param>
        /// <exception cref="ArgumentNullException">objectToWrap is null.</exception>
        public AutomaticViewModel(object objectToWrap, BindingFlags propertyFlags)
        {
            if (objectToWrap == null) throw new ArgumentNullException(nameof(objectToWrap));

            wrapped = objectToWrap;

            // get properties, remove properties that have no setter
            properties = objectToWrap.GetType().GetProperties(propertyFlags).Where(p => p.CanWrite).ToList();

            remappedProperties = new Dictionary<string, object>();

            RemapComplexTypesAndCollections();
        }

        #endregion

        #region Methods

        private void RemapComplexTypesAndCollections()
        {
            try
            {
                foreach (PropertyInfo propertyInfo in properties)
                {
                    // can't remap primitives, only wrap them for notification, which has already been setup in the constructor
                    if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string) || propertyInfo.PropertyType == typeof(DateTime))
                        continue;

                    // remap collections (all collection types derive from IEnumerable)
                    //bool isIEnumerable = propertyInfo.PropertyType.IsSubclassOf(typeof(IEnumerable)); // this fails
                    IEnumerable values = propertyInfo.GetValue(wrapped) as IEnumerable; // but this works...why?

                    if (values == null)
                    {
                        // if not primitive or not a collection then just wrap in a AutomaticViewModel
                        AutomaticViewModel automaticViewModel = new AutomaticViewModel(propertyInfo.GetValue(wrapped));

                        remappedProperties.Add(propertyInfo.Name, automaticViewModel);
                    }
                    else
                    {
                        Type observableCollectionType = typeof(ObservableCollection<>);

                        Type argumentType = null;
                        Type observableType = null;
                        object collection = null;

                        foreach (object value in values)
                        {
                            if (argumentType == null)
                            {
                                argumentType = value.GetType();

                                if (argumentType.IsPrimitive || argumentType == typeof(string) || argumentType == typeof(DateTime))
                                {
                                    // make observable collection type a non-AutomaticViewModel type as it is a non-complex type
                                    observableType = observableCollectionType.MakeGenericType(argumentType);
                                }
                                //else if (propertyInfo.PropertyType.IsSubclassOf(typeof(IEnumerable)) || propertyInfo.PropertyType == typeof(IEnumerable)) // this fails
                                else if (value is IEnumerable enumerable) // but this works...why?
                                {
                                    // a collection of collections
                                    throw new ArgumentException($"Property {propertyInfo.Name} on type {wrapped.GetType()} is too complex for auto mapping. A collection of collections is not supported. Suggestion is to build your own view model.");

                                    /*
                                     * The reason a collection of collections isn't supported is because where does it stop? We can't map all that depth at the top
                                     * level with one collection of remapped properties. Generic collections mean the type could have an X number of nested sub types
                                     * underneath that are also collections.
                                     */
                                }
                                else
                                {
                                    argumentType = typeof(AutomaticViewModel);

                                    // make observable collection an AutomaticViewModel type as it is a complex type
                                    observableType = observableCollectionType.MakeGenericType(argumentType);
                                }

                                collection = Activator.CreateInstance(observableType);
                            }

                            IList temp = collection as IList;

                            if (temp != null)
                            {
                                if (argumentType == typeof(AutomaticViewModel))
                                {
                                    temp.Add(new AutomaticViewModel(value));
                                }
                                else
                                {
                                    temp.Add(value);
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Could not add the value to the collection.");

                                throw new InvalidOperationException($"Cannot auto map property {propertyInfo.Name}. Adding items to auto mapped ObservableCollection failed.");
                            }
                        }

                        remappedProperties.Add(propertyInfo.Name, collection);

                        // processed collection property, move onto next property
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred attempting to process object {wrapped}", ex);
            }
        }

        /// <summary>Attempts to get a property.</summary>
        /// <param name="binder">The binder with the info for property retrieval.</param>
        /// <param name="result">The result of the retrieval.</param>
        /// <returns>True if successfully retrieved otherwise false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // if we have a remapped property then we will get that
            if (remappedProperties.ContainsKey(binder.Name))
            {
                result = remappedProperties[binder.Name];

                return true;
            }

            PropertyInfo property = properties.FirstOrDefault(p => p.Name == binder.Name);

            if (property != null)
            {
                result = property.GetValue(wrapped);

                return true;
            }
            else
            {
                result = null;

                return false;
            }
        }

        /// <summary>Attempts to set the value of a property.</summary>
        /// <param name="binder">The binder with the info for property retrieval.</param>
        /// <param name="value">The value to set for the property.</param>
        /// <returns>True if the value was set otherwise false.</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // we are not adding properties this way, our properties will be generated from the constructor

            // if we have a remapped property then we set that
            if (remappedProperties.ContainsKey(binder.Name))
            {
                remappedProperties[binder.Name] = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(binder.Name));

                return true;
            }

            PropertyInfo property = properties.FirstOrDefault(p => p.Name == binder.Name);

            if (property == null)
            {
                return false;
            }

            // we will not put nulls in the dictionary
            property.SetValue(wrapped, value);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(binder.Name));

            return true;
        }

        #endregion
    }
}
