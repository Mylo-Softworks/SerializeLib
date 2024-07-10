namespace SerializeLib;

public static partial class Serializer
{
    /// <summary>
    /// Serialize to a byte[] (type specified as generic).
    /// </summary>
    /// <param name="o">The object to serialize.</param>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <returns>A byte[] containing the serialized bytes from object o.</returns>
    public static byte[] Serialize<T>(T o)
    {
        using var stream = new MemoryStream();
        SerializeValue(o, stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Serialize to a byte[] (type specified as argument).
    /// </summary>
    /// <param name="o">The object to serialize.</param>
    /// <param name="t">The type to serialize.</param>
    /// <returns>A byte[] containing the serialized bytes from object o.</returns>
    public static byte[] Serialize(object? o, Type t)
    {
        using var stream = new MemoryStream();
        SerializeValue(o, stream, t);
        return stream.ToArray();
    }

    /// <summary>
    /// Deserialize from a byte[] (type specified as generic).
    /// </summary>
    /// <param name="bytes">The bytes to read from.</param>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <returns>A deserialized instance of T from bytes.</returns>
    public static T? Deserialize<T>(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return DeserializeValue<T>(stream);
    }

    /// <summary>
    /// Deserialize from a byte[] (type specified as argument).
    /// </summary>
    /// <param name="bytes">The bytes to read from.</param>
    /// <param name="t">The type to deserialize into.</param>
    /// <returns>A deserialized instance of T from bytes.</returns>
    public static object? Deserialize(byte[] bytes, Type t)
    {
        using var stream = new MemoryStream(bytes);
        return DeserializeValue(stream, t);
    }

    /// <summary>
    /// Serialize to a file (type specified as generic).
    /// </summary>
    /// <param name="o">The object to serialize.</param>
    /// <param name="path">The file path to write to.</param>
    /// <typeparam name="T">The type to serialize.</typeparam>
    public static void SerializeToFile<T>(T o, string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        SerializeValue(o, stream);
    }

    /// <summary>
    /// Serialize to a file (type specified as argument).
    /// </summary>
    /// <param name="o">The object to serialize.</param>
    /// <param name="t">The type to serialize.</param>
    /// <param name="path">The file path to write to.</param>
    public static void SerializeToFile(object o, Type t, string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        SerializeValue(o, stream, t);
    }
    
    /// <summary>
    /// Deserialize from a file (type specified as generic).
    /// </summary>
    /// <param name="path">The file path to read from.</param>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <returns>A deserialized instance of T from the file at path.</returns>
    public static T? DeserializeFromFile<T>(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return DeserializeValue<T>(stream);
    }

    /// <summary>
    /// Deserialize from a file (type specified as argument).
    /// </summary>
    /// <param name="path">The file path to read from.</param>
    /// <param name="t">The type to deserialize into.</param>
    /// <returns>A deserialized instance of T from the file at path.</returns>
    public static object? DeserializeFromFile(string path, Type t)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return DeserializeValue(stream, t);
    }
}