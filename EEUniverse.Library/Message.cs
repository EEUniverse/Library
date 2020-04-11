using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EEUniverse.Library
{
    /// <summary>
    /// Represents an Everybody Edits Universe™ message.
    /// </summary>
    public class Message : IEnumerable<object>
    {
        /// <summary>
        /// Represents the scope this message was received in.
        /// </summary>
        public ConnectionScope Scope { get; }

        /// <summary>
        /// Represents the type of the message.
        /// </summary>
        public MessageType Type { get; }

        /// <summary>
        /// Gets or sets an object at the given index.
        /// </summary>
        /// <param name="index">The index to set the object at.</param>
        public object this[int index] { get => Data[index]; set => Set(index, value); }

        /// <summary>
        /// Gets the total amount of elements in the message.
        /// </summary>
        public int Count => Data.Count;

        /// <summary>
        /// Collection of objects in the message.
        /// </summary>
        public List<object> Data;

        /// <summary>
        /// Initializes a new message.
        /// </summary>
        /// <param name="scope">The scope of the message.</param>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">The data of the message.</param>
        public Message(ConnectionScope scope, MessageType type, params object[] data)
        {
            Scope = scope;
            Type = type;

            EnsureValidMessageTypes(data);
            Data = new List<object>(data);
        }

        /// <summary>
        /// Initializes a new message.
        /// </summary>
        /// <param name="scope">The scope of the message.</param>
        /// <param name="type">The type of the message.</param>
        /// <param name="data">The data of the message.</param>
        public Message(ConnectionScope scope, MessageType type, List<object> data)
        {
            Scope = scope;
            Type = type;

            EnsureValidMessageTypes(data);
            Data = data;
        }

        /// <summary>
        /// Returns an IEnumerator for the data.
        /// </summary>
        public IEnumerator GetEnumerator() => Data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<object> IEnumerable<object>.GetEnumerator() => ((IEnumerable<object>)Data).GetEnumerator();

        /// <summary>
        /// Sets data at the given index to the given object.
        /// </summary>
        /// <param name="index">The index where the data should be written to.</param>
        /// <param name="value">The value to write.</param>
        public void Set(int index, object value)
        {
            EnsureValidMessageType(value);
            Data[index] = value;
        }

        /// <summary>
        /// Adds one or more objects to the existing message data.
        /// </summary>
        /// <param name="value">The object to add.</param>
        public void Add(params object[] values)
        {
            EnsureValidMessageTypes(values);
            Data.AddRange(values);
        }

        /// <summary>
        /// Gets an object at a given index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public object Get(int index) => Data[index];

        /// <summary>
        /// Gets an object at a given index.
        /// </summary>
        /// <typeparam name="T">The object will be converted to this type.</typeparam>
        /// <param name="index">The index to grab the object from.</param>
        public T Get<T>(int index)
        {
            try {
                if (Data is T value)
                    return value;

                return (T)Convert.ChangeType(Data[index], typeof(T));
            }
            catch (InvalidCastException) { throw new InvalidCastException($"The value at index {index} could not be converted from type '{Data[index].GetType().Name}' to type '{typeof(T).Name}'."); }
        }

        /// <summary>
        /// Gets a MessageObject at the specified index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public MessageObject GetObject(int index) => Get<MessageObject>(index);

        /// <summary>.
        /// Gets a string at the specified index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public string GetString(int index) => Get<string>(index);

        /// <summary>
        /// Gets a double at the specified index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public double GetDouble(int index) => Get<double>(index);

        /// <summary>
        /// Gets a byte[] at the specified index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public byte[] GetBytes(int index) => Get<byte[]>(index);

        /// <summary>
        /// Gets a bool at the specified index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public bool GetBool(int index) => Get<bool>(index);

        /// <summary>
        /// Gets an int at the specified index.
        /// </summary>
        /// <param name="index">The index to grab the object from.</param>
        public int GetInt(int index) => Get<int>(index);

        /// <summary>
        /// Returns a new Message with the sliced data.
        /// </summary>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns></returns>
        public Message Slice(int index, int count) => new Message(Scope, Type, Data.GetRange(index, count));

        /// <summary>
        /// Returns a human-readable string of the message.
        /// </summary>
        /// <returns>A human-readable string of the message.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Scope = {Scope}, Id = {Type.ToString(Scope)}, {Count} entr{(Count == 1 ? "y" : "ies")}, {Serializer.Serialize(this).Length} bytes");

            for (int i = 0; i < Data.Count; i++) {
                if (Data[i] is MessageObject mo) {
                    sb.AppendLine($"  [{i}] = MessageObject,");
                    foreach (var kvp in mo)
                        sb.AppendLine($"    [{kvp.Key}] = {DataObjectToString(kvp.Value)} ({kvp.Value.GetType().Name})");

                    continue;
                }

                sb.AppendLine($"  [{i}] = {DataObjectToString(Data[i])} ({Data[i].GetType().Name})");
            }

            return sb.ToString();

            static string DataObjectToString(object value)
            {
                if (value is byte[] bytes) {
                    const int amountOfBytesToShow = 4;

                    return $"[{string.Join(", ", bytes[..Math.Min(amountOfBytesToShow, bytes.Length)])}" +
                        $"{(bytes.Length > amountOfBytesToShow ? $", ...{bytes.Length - amountOfBytesToShow} more" : "")}]";
                }

                return value.ToString();
            }
        }

        [Conditional("DEBUG")]
        private static void EnsureValidMessageTypes(IEnumerable<object> data, bool allowDictionary = true)
        {
            foreach (var entry in data)
            {
                EnsureValidMessageType(entry, allowDictionary);
            }
        }

        [Conditional("DEBUG")]
        private static void EnsureValidMessageType(object entry, bool allowDictionary = true)
        {
            var isSerializeable = entry is bool
                || entry is byte
                || entry is sbyte
                || entry is short
                || entry is int
                || entry is double
                || entry is string
                || entry is byte[]
                || entry is ReadOnlyMemory<byte>
                || (allowDictionary ? entry is IDictionary<string, object> : false);

            Debug.Assert(isSerializeable, $"Data entry is not serializeable (type: {entry.GetType()}) (value: {entry.ToString()})");

            if (entry is IDictionary<string, object> dictionary)
            {
                EnsureValidMessageTypes(dictionary.Values, false);
            }
        }
    }
}
