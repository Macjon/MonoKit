// --------------------------------------------------------------------------------------------------------------------
// <copyright file=".cs" company="sgmunn">
//   (c) sgmunn 2012  
//
//   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//   documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
//   the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
//   to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
//   The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
//   the Software.
// 
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//   THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//   CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
//   IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MonoKit.DataBinding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides extension methods for handling AttachedProperties
    /// </summary>
    public static class AttachedPropertyExtensions
    {
        // todo: perform background check for stale values
  
        /// <summary>
        /// Value data store
        /// </summary>
        private static Dictionary<object, List<KeyValueWeakReference>> ValueDictionary = new Dictionary<object, List<KeyValueWeakReference>>();
        
        /// <summary>
        /// Sets the value of an AttachedProperty for an object 
        /// </summary>
        /// <param name='instance'>
        /// The instance for which the value is being set
        /// </param>
        /// <param name='property'>
        /// The AttachedProperty that defines the value being set
        /// </param>
        /// <param name='value'>
        /// The value of the property
        /// </param>
        public static void SetValue(this object instance, AttachedProperty property, object value)
        {
            var current = GetCurrent(instance, property, true);
            
            var oldValue = current.Value;
            current.Value = value;  
            
            if (oldValue != value && property.Metadata.ChangeCallback != null)
            {
                var args = new AttachedPropertyChangedEventArgs(property, value, oldValue);
                
                property.Metadata.ChangeCallback(instance, args);
            }
        }
        
        /// <summary>
        /// Gets the value of an AttachedProperty for an object 
        /// </summary>
        /// <param name='instance'>
        /// The instance for which the value is being retrieved
        /// </param>
        /// <param name='property'>
        /// The AttachedProperty that defines the value being retrieved
        /// </param>
        /// <returns>Returns an object, the default value or null</returns>
        public static object GetValue(this object instance, AttachedProperty property)
        {
            var current = GetCurrent(instance, property, false);
            if (current != null)
            {
                return current.Value;
            }
            
            return property.Metadata.DefaultValue;
        }
        
        /// <summary>
        /// Gets the current value of the property for the object
        /// </summary>
        /// <returns>
        /// Returns an object or null
        /// </returns>
        /// <param name='instance'>
        /// The object to look up in the values dictionary
        /// </param>
        /// <param name='property'>
        /// The property describing the value to look up.
        /// </param>
        /// <param name='addIfMissing'>Adds a reference if not found</param>
        private static KeyValueWeakReference GetCurrent(object instance, AttachedProperty property, bool addIfMissing)
        {
            KeyValueWeakReference result = null;
            
            if (ValueDictionary.ContainsKey(instance))
            {
                var values = ValueDictionary[instance];
                result = values.FirstOrDefault(x => x.Target == instance && x.Key == property.PropertyKey);
                
                if (result == null && addIfMissing)
                {
                    result = new KeyValueWeakReference(instance, property.PropertyKey);
                    values.Add(result);
                }
                
                return result;
            }
            
            if (addIfMissing)
            {
                result = new KeyValueWeakReference(instance, property.PropertyKey);
                ValueDictionary.Add(instance, new List<KeyValueWeakReference>() { result });
            }
            
            return result;
        }
    }
}

