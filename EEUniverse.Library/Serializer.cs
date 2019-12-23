using System;
using System.Collections.Generic;
using System.IO;

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
        /// Convert a message into a stream of bytes.<br />Use with caution.
        /// </summary>
        public static byte[] Serialize(Message message)
        {
            var memStream = new MemoryStream();
            using var writer = new BitEncodedStreamWriter(memStream);
            writer.Write7BitEncodedInt((byte)message.Scope);
            writer.Write7BitEncodedInt((int)message.Type);

            foreach (var data in message) {
                switch (data) {
                    case bool oBool: writer.Write(oBool ? _patternBooleanTrue : _patternBooleanFalse); break;

                    case byte oByte:
                    case sbyte oSByte:
                    case short oShort:
                    case int oInt: {
                            var value = Convert.ToInt32(data);
                            writer.Write(value < 0 ? _patternIntNeg : _patternIntPos);
                            writer.Write7BitEncodedInt(value < 0 ? -(value + 1) : value);
                        }
                        break;

                    case double oDouble: {
                            writer.Write(_patternDouble);
                            writer.Write(oDouble);
                        }
                        break;

                    case string oString: {
                            writer.Write(_patternString);
                            writer.Write(oString);
                        }
                        break;

                    case byte[] oBytes: {
                            writer.Write(_patternBytes);
                            writer.Write7BitEncodedInt(oBytes.Length);
                            writer.Write(oBytes);
                        }
                        break;

                    case IDictionary<string, object> oDict: {
                            writer.Write(_patternObject);
                            foreach (var kvp in oDict) {
                                switch (kvp.Value) {
                                    case bool oBool: writer.Write(oBool ? _patternBooleanTrue : _patternBooleanFalse); break;

                                    case byte oByte:
                                    case sbyte oSByte:
                                    case short oShort:
                                    case int oInt: {
                                            var value = Convert.ToInt32(kvp.Value);
                                            writer.Write(value < 0 ? _patternIntNeg : _patternIntPos);
                                            writer.Write7BitEncodedInt(value < 0 ? -(value + 1) : value);
                                        }
                                        break;

                                    case double oDouble: {
                                            writer.Write(_patternDouble);
                                            writer.Write(oDouble);
                                        }
                                        break;

                                    case string oString: {
                                            writer.Write(_patternString);
                                            writer.Write(oString);
                                        }
                                        break;

                                    case byte[] oBytes: {
                                            writer.Write(_patternBytes);
                                            writer.Write7BitEncodedInt(oBytes.Length);
                                            writer.Write(oBytes);
                                        }
                                        break;

                                    default: throw new NotSupportedException($"Data type {kvp.Value.GetType().Name} in MessageObject not supported.");
                                }

                                writer.Write(kvp.Key);
                            }

                            writer.Write(_patternObjectEnd);
                        }
                        break;

                    default: throw new NotSupportedException($"Data type {data.GetType().Name} is not supported.");
                }
            }

            return memStream.ToArray();
        }

        /// <summary>
        /// Deserialize a stream of bytes into a message.<br />Use with caution.
        /// </summary>
        [Obsolete("Please  use '" + nameof(Deserialize) + "(" + nameof(MemoryStream) + " memoryStream)' to deserialize messages.")]
        public static Message Deserialize(byte[] data) => Deserialize(new MemoryStream(data));

        /// <summary>
        /// Deserialize a stream of bytes into a message.<br />Use with caution.
        /// </summary>
        public static Message Deserialize(MemoryStream memoryStream)
        {
            // if `leaveOpen` is left false, this will dispose the memory stream once the BitEncodedStreamReader is disposed.
            // we don't want to dispose the memory stream because we continually reuse it for reading
            using var stream = new BitEncodedStreamReader(memoryStream, true);
            var scope = (ConnectionScope)stream.ReadByte();
            var type = (MessageType)stream.Read7BitEncodedInt();

            var argData = new List<object>();
            while (stream.BaseStream.Position < memoryStream.Length) {
                var patternType = stream.ReadByte();
                switch (patternType) {
                    case _patternString: argData.Add(stream.ReadString()); break;
                    case _patternIntPos: argData.Add(stream.Read7BitEncodedInt()); break;
                    case _patternIntNeg: argData.Add(-stream.Read7BitEncodedInt() - 1); break;
                    case _patternDouble: argData.Add(BitConverter.ToDouble(stream.ReadBytes(8), 0)); break;
                    case _patternBooleanFalse: argData.Add(false); break;
                    case _patternBooleanTrue: argData.Add(true); break;
                    case _patternBytes: {
                            var length = stream.Read7BitEncodedInt();
                            argData.Add(stream.ReadBytes(length));
                        }
                        break;

                    case _patternObject: {
                            var objectArgs = new MessageObject();
                            while (stream.ReadByte() != _patternObjectEnd) {
                                stream.BaseStream.Position--;

                                object value;
                                switch (stream.ReadByte()) {
                                    case _patternString: value = stream.ReadString(); break;
                                    case _patternIntPos: value = stream.Read7BitEncodedInt(); break;
                                    case _patternIntNeg: value = -stream.Read7BitEncodedInt() - 1; break;
                                    case _patternDouble: value = BitConverter.ToDouble(stream.ReadBytes(8), 0); break;
                                    case _patternBooleanFalse: value = false; break;
                                    case _patternBooleanTrue: value = true; break;
                                    case _patternBytes: {
                                            var length = stream.Read7BitEncodedInt();
                                            value = stream.ReadBytes(length);
                                        }
                                        break;

                                    default: throw new InvalidDataException($"Invalid pattern type {patternType} in MessageObject.");
                                }

                                objectArgs.Add(stream.ReadString(), value);
                            }

                            argData.Add(objectArgs);
                        }
                        break;

                    default: throw new InvalidDataException($"Invalid pattern type {patternType}.");
                }
            }

            return new Message(scope, type, argData.ToArray());
        }
    }
}
