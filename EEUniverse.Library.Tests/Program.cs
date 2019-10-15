// Ideally these would be automated tests with a mocked server.
// Potential libraries to use:
//  - Specflow (https://github.com/techtalk/SpecFlow); To write human-readable acceptance tests.
//  - Moq (https://github.com/moq/moq4); To easily mock functions. (mainly to fake the client part, because connecting to the real Everybody Edits Universe™ servers is a bad idea...)

// For now it's being manually tested...

using EEUniverse.Library;

namespace EEUniverseLibrary.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
			var msg = new Message
			(
				ConnectionScope.Lobby,
				MessageType.ChatOld,
				"str",
				12345,
				-12345,
				123456.0d,
				true,
				false,
				false,
				new byte[] { 1, 2, 3 },
				new MessageObject()
					.Add("a", "str")
					.Add("b", 12345)
					.Add("c", -12345)
					.Add("d", 123456.0d)
					.Add("e", true)
					.Add("f", false)
					.Add("g", new byte[] { 1, 2, 3 })
			);

			var bytes = Serializer.Serialize(msg);

			var d1 = Serializer.Deserialize(bytes);
			var d2 = Serializer.Deserialize(new System.ReadOnlySpan<byte>(bytes));

			System.Console.WriteLine("done");
        }
    }
}
