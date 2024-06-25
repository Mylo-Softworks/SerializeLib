# SerializeLib
A library for serializing and deserializing in .net

## Supported types
* Primitives
  * bool, string, byte, short, int, long, float, double, decimal (and unsigned variants).
* Lists
  * Lists can contain any supported types.
  * **Note**: Lists with value of `null` will be deserialized as an empty list.
* Objects
  * Any object which has the \[SerializeClass] attribute can be serialized as a field as well.
  * Objects with value of `null` will be deserialized as `null` as expected.

## Usage
### Defining a serializable object
```csharp
using SerializeLib.Attributes;

[SerializeClass]
public class SerializationExample {
    [SerializeField] public bool ExampleBool;
}
```

### Serializing the object
```csharp
using SerializeLib;

// Create an object to serialize
var exampleObject = new SerializationExample {
    ExampleBool = true
}

// Serialize to a stream
var stream = new MemoryStream();
Serializer.Serialize(exampleObject, stream);

// Serialize to a byte[]
var bytes = Serializer.Serialize(exampleObject);

// Serialize and write to file
Serialize.SerializeToFile(exampleObject, "filename.bin")
```

### Deserializing the object
```csharp
using SerializeLib;

// Deserialize from a stream
var stream = new MemoryStream(); // In practice, this should be a stream with the serialized bytes
var exampleObject = Serializer.Deserialize<SerializationExample>(stream);

// Deserialize from a byte[]
var bytes = new byte[0]; // In practice, this should be a byte array with the serialized bytes
var exampleObject = Serializer.Deserialize<SerializationExample>();

// Deserialize from a file
var exampleObject = Serializer.DeserializeFromFile<SerializationExample>("filename.bin");
```