using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EEUniverse.Library
{
    /// <summary>
    /// The serializer class used to Serialize outgoing messages and Deserialize incoming messages.
    /// </summary>
    public static class Serializer
    {
        private const byte _patternString = 0;
        private const byte _patternIntPos = 1;
        private const byte _patternIntNeg = 2;
        private const byte _patternDouble = 3;
        private const byte _patternBooleanFalse = 4;
        private const byte _patternBooleanTrue = 5;
        private const byte _patternBytes = 6;
        private const byte _patternObject = 7;
        private const byte _patternObjectEnd = 8;

        /// <summary>
		/// Calculates the exact size of a message, in bytes.
		/// </summary>
        public static int EstimateSize(Message message)
        {
			// currently, a connectionscope & message type can't be above 1 byte when 7 bit encoded.
			// this optimization is safe, currently.
			int size = 2

            // we add the message count because every item in the message requires a byte for the pattern type
            // that way we don't have to do size++ in the loop
                + message.Count;

            for (var i = 0; i < message.Count; i++)
            {
				var data = message[i];

				switch (data)
				{
					case int val: size += GetVarIntSize(val < 0 ? -val : val); break;
					case string val: size += GetVarIntSize(val.Length) + Encoding.UTF8.GetByteCount(val); break;
					case byte[] val: size += GetVarIntSize(val.Length) + val.Length; break;

					case double _: size += sizeof(double); break;

					case bool _: break;

					case IDictionary<string, object> dict:
                    {
						// there's a pattern byte for each value
						size += dict.Count

                        // there's a byte to end the pattern
                            + 1;

                        foreach(var kvp in dict)
                        {
							// write string
							size += GetVarIntSize(kvp.Key.Length) + Encoding.UTF8.GetByteCount(kvp.Key);

                            // TODO: i literally just copied and pasted this
                            // this is literally awful D:

                            switch(kvp.Value)
							{
								case int val: size += GetVarIntSize(val < 0 ? -val : val); break;
								case string val: size += GetVarIntSize(val.Length) + Encoding.UTF8.GetByteCount(val); break;
								case byte[] val: size += GetVarIntSize(val.Length) + val.Length; break;

								case double _: size += sizeof(double); break;

								case bool _: break;
							}
						}
					}
					break;
				}
			}

			return size;
		}

        private static int GetVarIntSize(int value)
	    {
			// nearly copied from https://source.dot.net/#System.Private.CoreLib/shared/System/IO/BinaryWriter.cs,456

			// Write out an int 7 bits at a time.  The high bit of the byte,
			// when on, tells reader to continue reading more bytes.
			uint v = (uint)value;   // support negative numbers
			int size = 1;
			while (v >= 0x80)
			{
				size++;
				v >>= 7;
			}

			return size;
		}

		/// <summary>
		/// Convert a message into a stream of bytes.<br />Use with extreme caution.
		/// </summary>
		public static Memory<byte> Serialize(Message message)
        {
			var target = new Memory<byte>(new byte[EstimateSize(message)]);
			var writer = new MessageWriter(target.Span);

			writer.Write(message.Scope);
			writer.Write(message.Type);

            for (var i = 0; i < message.Count; i++)
            {
				var data = message[i];

				switch (data)
				{
					case bool @bool: writer.Write(@bool ? _patternBooleanTrue : _patternBooleanFalse); break;

					case int @int:
					{
						writer.Write(@int < 0 ? _patternIntNeg : _patternIntPos);
						writer.Write(@int < 0 ? -(@int + 1) : @int);
					}
					break;

					case double @double:
					{
						writer.Write(_patternDouble);
						writer.Write(@double);
					}
					break;

					case string @string:
					{
						writer.Write(_patternString);
						writer.Write(@string);
					}
					break;

					case byte[] bytes:
					{
						writer.Write(_patternBytes);
						writer.Write(bytes);
					}
					break;

					case IDictionary<string, object> dict:
					{
						writer.Write(_patternObject);

                        foreach(var (key, value) in dict)
						{
							if (key.Length == _patternObjectEnd) // Until they fix, which will be a breaking change...
							{
								throw new InvalidDataException("The specified key in MessageObject is invalid; it must be lower or greater than 8 characters in length.");
							}

							writer.Write(key);
                            
                            // this blatant copying and pasting actually hurts my soul
                            switch(value)
							{
								case bool @bool: writer.Write(@bool ? _patternBooleanTrue : _patternBooleanFalse); break;

								case int @int:
								{
									writer.Write(@int < 0 ? _patternIntNeg : _patternIntPos);
									writer.Write(@int < 0 ? -(@int + 1) : @int);
								}
								break;

								case double @double:
								{
									writer.Write(_patternDouble);
									writer.Write(@double);
								}
								break;

								case string @string:
								{
									writer.Write(_patternString);
									writer.Write(@string);
								}
								break;

								case byte[] bytes:
								{
									writer.Write(_patternBytes);
									writer.Write(bytes);
								}
								break;
							}
						}

						writer.Write(_patternObjectEnd);
					}
					break;
				}
			}

			return target;
		}

		/// <summary>
		/// Deserialize a stream of bytes into a message.<br />Use with extreme caution.
		/// </summary>
		public static Message Deserialize(ReadOnlySpan<byte> data)
        {
			var reader = new MessageReader(data);

			var scope = reader.ReadConnectionScope();
			var type = reader.ReadMessageType();

			var argData = new List<object>();

			while (reader.IsDataLeft())
			{
				var patternType = reader.ReadByte();

				object obj;

				switch (patternType)
				{
					case _patternString: obj = reader.ReadString(); break;
					case _patternIntPos: obj = reader.ReadInt(); break;
					case _patternIntNeg: obj = -reader.ReadInt(); break;
					case _patternDouble: obj = reader.ReadDouble(); break;
					case _patternBooleanTrue: obj = true; break;
					case _patternBooleanFalse: obj = false; break;
					case _patternBytes: obj = reader.ReadBytes().ToArray(); break;
					case _patternObject:
                    {
						var messageObject = new MessageObject();

                        while (reader.ReadByte() != _patternObjectEnd)
                        {
							reader.BackUp();

							var key = reader.ReadString();
							object value;

							switch (reader.ReadByte())
                            {
								case _patternString: value = reader.ReadString(); break;
								case _patternIntPos: value = reader.ReadInt(); break;
								case _patternIntNeg: value = -reader.ReadInt(); break;
								case _patternDouble: value = reader.ReadDouble(); break;
								case _patternBooleanTrue: value = true; break;
								case _patternBooleanFalse: value = false; break;
								case _patternBytes: value = reader.ReadBytes().ToArray(); break;

								default: throw new InvalidDataException($"Invalid pattern type {patternType} in MessageObject.");
							}

							messageObject.Add(key, value);
						}

						obj = messageObject;
					}
					break;

					default: throw new InvalidDataException($"Invalid pattern type {patternType}.");
				}

				argData.Add(obj);
			}

			return new Message(scope, type, argData.ToArray());
		}
    }
}
