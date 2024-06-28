using System.Text;

namespace SerializeLib;

public static partial class Serializer
{
    private static void SerializeString(string v, Stream s)
    {
        var bytes = Encoding.UTF8.GetBytes(v);
        var size = bytes.Length;
        var writeSize = BitConverter.GetBytes(size);
        s.Write(writeSize, 0, writeSize.Length); // First write the size
        s.Write(bytes, 0, bytes.Length); // Then write the content
    }

    private static string DeserializeString(Stream s)
    {
        var lengthBytes = new byte[4];
        s.Read(lengthBytes, 0, lengthBytes.Length);
        var size = BitConverter.ToInt32(lengthBytes);
        var bytes = new byte[size];
        s.Read(bytes, 0, bytes.Length);
        return Encoding.UTF8.GetString(bytes);
    }
}