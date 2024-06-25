using System.Text;
using SerializeLib;
using SerializeLib.Attributes;

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
}

[SerializeClass]
internal class TestSubClass
{
    [SerializeField] public bool TestBool;
    [SerializeField] public string TestString;
}

public static class Tests
{
    public static void Main()
    {
        var obj = new TestClass()
        {
            TestBool = true,
            TestInt = 123,
            TestString = "I am a cool string! Yay!",
            TestFloat = 3.14f,
            TestList = new List<int>() { 1, 2, 3 },
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
            }
        };
        // Console.WriteLine(memoryStream.Length);
        
        // memoryStream.Seek(0, SeekOrigin.Begin); // Reset
        // var testInst = Serializer.Deserialize<TestClass>(memoryStream);
        
        Serializer.SerializeToFile(obj, "TestClass.bin");
        
        var testInst = Serializer.DeserializeFromFile<TestClass>("TestClass.bin");
        
        Console.WriteLine(String.Join(", ", testInst.TestList));
        Console.WriteLine(testInst.TestBool);
        Console.WriteLine(String.Join(", ", testInst.TestSubClassList.Select(@class => @class.TestString)));
    }
}