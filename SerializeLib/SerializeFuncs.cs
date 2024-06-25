namespace SerializeLib;

public static partial class Serializer
{
    public static byte[] Serialize(object o)
    {
        using var stream = new MemoryStream();
        Serialize(o, stream);
        return stream.ToArray();
    }

    public static T? Deserialize<T>(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Deserialize<T>(stream);
    }

    public static void SerializeToFile<T>(T o, string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        Serialize(o, stream);
    }

    public static T? DeserializeFromFile<T>(string path)
    {
        using var s = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Deserialize<T>(s);
    }
}