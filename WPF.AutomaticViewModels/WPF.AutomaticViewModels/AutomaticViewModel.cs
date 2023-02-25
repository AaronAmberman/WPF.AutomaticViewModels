using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// dynamic object. These properties will have built in change notification that push the 
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
        //private Dictionary<string, ObservableCollection<object>> remappedCollectionProperties;
        //private Dictionary<string, ObservableCollection<PropertyChangeNotifier>> remappedCollectionPropertyNotifiers;
        private Dictionary<string, AutomaticViewModel> remappedProperties;
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

            //remappedCollectionProperties = new Dictionary<string, ObservableCollection<object>>();
            //remappedCollectionPropertyNotifiers = new Dictionary<string, ObservableCollection<PropertyChangeNotifier>>();
            remappedProperties = new Dictionary<string, AutomaticViewModel>();
        }

        #endregion

        #region Methods

        /// <summary>Maps a property to a PropertyChangeNotifier so its properties can broadcast changes.</summary>
        /// <param name="propertyName">The name of the property to wrap.</param>
        /// <exception cref="ArgumentNullException">propertyName is null, empty or consist of white-space characters only.</exception>
        /// <exception cref="KeyNotFoundException">The property name is not found on the object.</exception>
        public void MapPropertyToNotifier(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            PropertyInfo match = properties.FirstOrDefault(p => p.Name == propertyName);

            if (match == null) throw new KeyNotFoundException();

            if (match.PropertyType.IsPrimitive)
                throw new InvalidOperationException($"propertyName {propertyName} cannot be a primitive type. Must be a complex type with set-able properties.");

            AutomaticViewModel propertyNotifier = new AutomaticViewModel(match.GetValue(wrapped));

            remappedProperties.Add(propertyName, propertyNotifier);
        }

        //public void MapCollectionPropertyToObservableCollection(string propertyName, bool mapToNotifier)
        //{
        //    if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

        //    PropertyInfo match = properties.FirstOrDefault(p => p.Name == propertyName);

        //    if (match == null) throw new KeyNotFoundException();

        //    if (!match.PropertyType.IsSubclassOf(typeof(IEnumerable)) || match.PropertyType != typeof(IEnumerable))
        //        throw new InvalidOperationException($"propertyName {propertyName} is not a collection type.");

        //    IEnumerable values = match.GetValue(wrapped) as IEnumerable;

        //    if (values == null)
        //        throw new InvalidOperationException($"propertyName {propertyName} could not be cast as an IEnumerable.");

        //    if (mapToNotifier)
        //    {
        //        List<PropertyChangeNotifier> remappedValues = new List<PropertyChangeNotifier>();

        //        foreach (object value in values)
        //        {
        //            if (value.GetType().IsPrimitive)
        //                throw new InvalidOperationException($"propertyName {propertyName} cannot be a collection primitive types. Must be a complex type with set-able properties contained in the collection.");

        //            remappedValues.Add(new PropertyChangeNotifier(value));
        //        }

        //        remappedCollectionPropertyNotifiers.Add(propertyName, new ObservableCollection<PropertyChangeNotifier>(remappedValues));
        //    }
        //    else
        //    {
        //        List<object> newValues = new List<object>();

        //        foreach (object value in values)
        //        {
        //            newValues.Add(value);
        //        }

        //        remappedCollectionProperties.Add(propertyName, new ObservableCollection<object>(newValues));
        //    }
        //}

        /// <summary>Attempts to get a property.</summary>
        /// <param name="binder">The binder with the info for property retrieval.</param>
        /// <param name="result">The result of the retrieval.</param>
        /// <returns>True if successfully retrieved otherwise false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // if we have a remapped property then we will get that
            if (remappedProperties.ContainsKey(binder.Name))
            {
                return remappedProperties[binder.Name].TryGetMember(binder, out result);
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
                return remappedProperties[binder.Name].TrySetMember(binder, value);
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
