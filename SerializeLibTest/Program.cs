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
}

public static class Tests
{
    public static void Main()
    {
        var memoryStream = new MemoryStream();
        Serializer.Serialize(new TestClass()
        {
            TestBool = true,
            TestInt = 123,
            TestString = "I am a cool string! Yay!",
            TestFloat = 3.14f,
            TestList = new List<int>() { 1, 2, 3 },
            TestSubClass = new TestSubClass()
            {
                TestBool = true
            },
            TestSubClassList = new ()
            {
                new TestSubClass()
                {
                    TestBool = true
                },
                new TestSubClass()
                {
                    TestBool = false
                }
            }
        }, memoryStream);
        // Console.WriteLine(memoryStream.Length);
        memoryStream.Seek(0, SeekOrigin.Begin); // Reset
        var testInst = Serializer.Deserialize<TestClass>(memoryStream);
        Console.WriteLine(String.Join(", ", testInst.TestList));
        Console.WriteLine(testInst.TestBool);
        Console.WriteLine(String.Join(", ", testInst.TestSubClassList.Select(@class => @class.TestBool)));
    }
}