namespace SerializeLib.Interfaces;

/// <summary>
/// An interface for manual serialization.
/// </summary>
/// <example>
/// class SerializableExample : ISerializableClass&lt;SerializableExample>
/// </example>
/// <typeparam name="T">The same class, see example.</typeparam>
public interface ISerializableClass<out T> where T : ISerializableClass<T>
{
    /// <summary>
    /// Function used to serialize this object.
    /// This function must write the exact same number of bytes as the Deserialize() function reads.
    /// </summary>
    /// <param name="s">The stream to write the data to.</param>
    public void Serialize(Stream s);
    
    /// <summary>
    /// Function used to deserialize an object of this type from a stream.  
    /// This function must read the exact same number of bytes as the Serialize() function writes.
    /// </summary>
    /// <param name="s">The stream to read the data from.</param>
    /// <returns>this, or another object of type T.</returns>
    public T Deserialize(Stream s); // A new instance is created when running this method.
}