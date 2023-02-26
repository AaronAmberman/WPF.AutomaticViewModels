# WPF.AutomaticViewModels
An API that provides functionality to automatically add property change notifications to objects by providing special object wrappers.

## What is an AutomaticViewModel?
It is exactly what you think it is by name. A wrapper object that can wrap most any object and add property change notifications to it. Property change notifications originate from the AutomatciViewModel and changes are pushed back to the wrapped object so no additional code needs to be written to keep the view model and the model in sync. 

## How Does it Work?
For starters the AutomaticViewModel is a *dynamic* object so that it can be interpreted at runtime. We are not going to go over how *dynamic* objects work in .NET so if you don't know then please google it and read up on it. Should only take a few minutes. 

### Public Property Mapping
The AutomaticViewModel has 2 constructors, `AutomaticViewModel(object)` and `AutomaticViewModel(object, BindingFlags)`. Both only grab properties of the specified type. The `AutomaticViewModel(object)` constructor will grab all public instance properties. The `AutomaticViewModel(object, BindingFlags)` constructor will grab whatever the specified binding flags tell it to grab. Property names are an exact match.

### No Expansion
These *dynamic* objects ***do not*** allow for additional properties, methods or events to be added to it, they are immutable. The properties themselves are mutable, they need to be in order to automatically update the UI.

## Auto Remapping
The AutomaticViewModel will gladly wrap any primitive type (Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single), String or DateTime without the need for any remapping to occur. They will still have property change notifications. Auto remappings occur during construction of the object.

### Complex Property Types
If the AutomaticViewModel encounters a complex type (does not fit into the above category) it will generate an AutomaticViewModel wrapper for that property so that its properties can respond to change notifications. 

### Collection Property Types
If the AutomaticViewModel encounters a collection of any kind type it will generate an ObservableCollection of the same type (or object in the case of non-generic collections such as IList). 

If a collection property contains a complex type then that complex type is wrapped in an AutomatciViewModel. If the passed in object has a property of type `List<User>` then on the AutomaticViewModel it will be a `ObservableCollection<AutomaticViewModel>`. This way the objects in the collection can respond to property changes and broadcast notifications. 

### Collections of Collections
This is where the AutomaticViewModel needs to draw the line. As we enter the realm of generics we have to limit how deep we dig. This greatly increases the complexity of property/type management. What if a Collection has a collection of tuples that had a collection in it? I mean shame on your for writing such a complex property type but complex types are common with generics. So if you have a lot of various generics or collections of collections in your data models than consider making your own view models. 

## Limitations
As aforementioned, collections of collections. The API will throw an error at runtime if the provided object has a collection of collections on it. As WPF MVVM developers we've been writing view models for a long time so we are used to it.

## What the API Is and What It Is Not
### What It Is Not
It is not meant to replace the need for a complex view models written by hand. In situations where you have the need for a complex view model (such as one with additional validation rules or something) then this simple API is not for you.

### What It Is
It is a simple type auto mapper that adds property change notifications with the ability to remap complex and collection type properties. To be used in situations where the UI workflow is simple or where the data management workflow is simple. 

Writing lots of small view models to handle changes to things while ensuring the UI stays updated is very tedious. This can help in those situations. 

# Example
Please check out the demo application for a simple demostration.
