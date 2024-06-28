namespace SerializeLib.Interfaces;

// Usage:
// public class SerializableExample : ISerializableClass<SerializableExample>
public interface ISerializableClass<out T> where T : ISerializableClass<T>
{
    public void Serialize(Stream s);
    public T Deserialize(Stream s); // A new instance is created when running this method.
}