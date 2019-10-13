using System;
using System.Collections.Generic;

namespace EEUniverse.Library
{
    /// <summary>
    /// Represents a collection of string keys and object values.
    /// </summary>
    public class MessageObject : Dictionary<string, object>
    {
        /// <summary>
        /// Adds an object to the collection.
        /// </summary>
        /// <param name="key">The name of the key.</param>
        /// <param name="value">The value of the object.</param>
        public new MessageObject Add(string key, object value)
        {
            base.Add(key, value);
            return this;
        }

        /// <summary>
        /// Gets an object at a given index.
        /// </summary>
        /// <typeparam name="T">The return type.<br />The object will also be converted to this type.</typeparam>
        /// <param name="index">The index to grab the object from.</param>
        public T Get<T>(string index)
        {
            try {
                if (this[index] is T value)
                    return value;

                return (T)Convert.ChangeType(this[index], typeof(T));
            }
            catch (InvalidCastException) { throw new InvalidCastException($"The value at index '{index}' could not be converted from type '{this[index].GetType().Name}' to type '{typeof(T).Name}'."); }
        }

        /// <summary>
        /// Gets a string at a given index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public string GetString(string index) => Get<string>(index);

        /// <summary>
        /// Gets a double at a given index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public double GetDouble(string index) => Get<double>(index);

        /// <summary>
        /// Gets a byte[] at a given index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public byte[] GetBytes(string index) => Get<byte[]>(index);

        /// <summary>
        /// Gets a bool at a given index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public bool GetBool(string index) => Get<bool>(index);

        /// <summary>
        /// Gets an int at a given index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public int GetInt(string index) => Get<int>(index);
    }
}
