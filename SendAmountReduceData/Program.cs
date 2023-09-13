using System;

namespace SendAmountReduceData
{
    internal class Program
    {
        public static void Main(string[] _)
        {
            PlayerPosition pp = new PlayerPosition(15.055f, 17.453f, -56.7342f);

            byte[] array = Serializer.SerializeToBytes(pp);
            Console.WriteLine($"Position sent: ({pp.X},{pp.Y},{pp.Z}) | Will be sent: {array.Length * 8} bits");

            PlayerPosition ppDeserialized = Serializer.DeserializeFromBytes(array);
            Console.WriteLine($"Received position: ({ppDeserialized.X},{ppDeserialized.Y},{ppDeserialized.Z})");

            Console.ReadKey();
        }
    }
}