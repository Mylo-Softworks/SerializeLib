using System.Text;
using SerializeLib;
using SerializeLib.Attributes;
using SerializeLib.Interfaces;

namespace SerializeLibTest;

[SerializeClass]
internal class TestClass
{
    [SerializeField] public bool TestBool;
    [SerializeField] public int TestInt;
    [SerializeField] public string TestString;
    [SerializeField] public float TestFloat;
    [SerializeField] public List<int> TestList;
    [SerializeField] public TestSubClass TestSubClass;
    [SerializeField] public List<TestSubClass> TestSubClassList;
    [SerializeField] public int[] TestArray;
    [SerializeField] public Guid TestGuid;
}

[SerializeClass]
internal class TestSubClass
{
    [SerializeField] public bool TestBool;
    [SerializeField] public string TestString;
}

internal class ManualSerializeClass : ISerializableClass<ManualSerializeClass>
{
    public int Number = 0;
    public void Serialize(Stream s)
    {
        Serializer.SerializeValue(Number, s); // Generic, so type is auto-detected here
    }

    public ManualSerializeClass Deserialize(Stream s)
    {
        Number = Serializer.DeserializeValue<int>(s); // Generic, type is specified here
        
        return this;
    }
}

public class GuidSerializeOverride : ISerializableOverride<Guid>
{
    private int size = 16;
    
    public void Serialize(Guid target, Stream s)
    {
        s.Write(target.ToByteArray());
    }

    public Guid Deserialize(Stream s)
    {
        var buffer = new byte[size];
        s.Read(buffer, 0, buffer.Length);
        return new Guid(buffer);
    }
}

public static class Tests
{
    public static void Main()
    {
        // Register override
        Serializer.RegisterOverride<GuidSerializeOverride, Guid>();
        // Serializer.RegisterOverride(new GuidSerializeOverride());
        
        var obj = new TestClass()
        {
            TestBool = true,
            TestInt = 123,
            TestString = "I am a cool string! Yay!",
            TestFloat = 3.14f,
            TestList = null,
            TestSubClass = new TestSubClass()
            {
                TestBool = true,
                TestString = "Another cool string!"
            },
            TestSubClassList = new()
            {
                new TestSubClass()
                {
                    TestBool = true,
                    TestString = "String 3!"
                },
                new TestSubClass()
                {
                    TestBool = false,
                    TestString = "String 4!"
                }
            },
            TestArray = new []
            {
                0, 1, 2
            },
            TestGuid = Guid.NewGuid()
        };
        
        var obj2 = new ManualSerializeClass()
        {
            Number = 123
        };
        
        var stream = new MemoryStream();
        Serializer.Serialize(obj, stream);
        stream.Seek(0, SeekOrigin.Begin);
        
        var testInst = Serializer.Deserialize<TestClass>(stream)!;
        
        // Serializer.SerializeToFile(obj, "TestClass.bin");
        //
        // var testInst = Serializer.DeserializeFromFile<TestClass>("TestClass.bin");
        
        // Console.WriteLine(String.Join(" ", stream.ToArray().Select(b => b + " ")));
        
        Console.WriteLine(obj.TestGuid);
        Console.WriteLine(testInst.TestGuid);
        
        // Console.WriteLine(String.Join(", ", testInst.TestList));
        // Console.WriteLine(testInst.TestBool);
        // Console.WriteLine(String.Join(", ", testInst.TestSubClassList.Select(@class => @class.TestString)));
    }
}