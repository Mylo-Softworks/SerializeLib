using System.Collections;
using System.Reflection;
using System.Text;
using SerializeLib.Attributes;

namespace SerializeLib;

public static partial class Serializer
{
    // First byte of object is 0 when null, 1 when not null
    public static void Serialize<T>(T obj, Stream s)
    {
        var t = typeof(T);
        Serialize(obj, t, s);
    }
    
    public static void Serialize(object? obj, Type t, Stream s, bool noError = false)
    {
        if (t.GetCustomAttributes(typeof(SerializeClassAttribute), false).Length == 0)
            if (noError) return;
            else throw new ArgumentException("This type does not have a SerializeClassAttribute");
        
        if (obj == null)
        {
            s.WriteByte(0);
            return;
        }
        s.WriteByte(1);

        foreach (var fieldInfo in t.GetFields())
        {
            if (fieldInfo.GetCustomAttributes(typeof(SerializeFieldAttribute), false).Length == 0) continue;
            SerializeField(fieldInfo, obj, s);
        }

        foreach (var propertyInfo in t.GetProperties())
        {
            if (propertyInfo.GetCustomAttributes(typeof(SerializeFieldAttribute), false).Length == 0) continue;
            SerializeProperty(propertyInfo, obj, s);
        }
        
        s.Flush();
    }

    private static void SerializeField(FieldInfo field, object obj, Stream s)
    {
        var val = field.GetValue(obj);
        SerializeValue(val, s);
    }

    private static void SerializeProperty(PropertyInfo property, object obj, Stream s)
    {
        if (property.SetMethod == null || property.GetMethod == null)
            return;
        
        var val = property.GetValue(obj, null);
        SerializeValue(val, s);
    }

    public static void SerializeValue(object? v, Stream s)
    {
        // Static types
        switch (v)
        {
            case bool vBool:
                s.Write(BitConverter.GetBytes(vBool));
                return;
            case byte vByte:
                s.WriteByte(vByte);
                return;
            case short vShort:
                s.Write(BitConverter.GetBytes(vShort));
                return;
            case ushort vUShort:
                s.Write(BitConverter.GetBytes(vUShort));
                return;
            case int vInt:
                s.Write(BitConverter.GetBytes(vInt));
                return;
            case uint vUInt:
                s.Write(BitConverter.GetBytes(vUInt));
                return;
            case long vLong:
                s.Write(BitConverter.GetBytes(vLong));
                return;
            case ulong vULong:
                s.Write(BitConverter.GetBytes(vULong));
                return;
            case float vFloat:
                s.Write(BitConverter.GetBytes(vFloat));
                return;
            case double vDouble:
                s.Write(BitConverter.GetBytes(vDouble));
                return;
            case decimal vDecimal:
                s.Write(BitConverter.GetBytes((double)vDecimal));
                return;
        }
        
        // Dynamic types
        switch (v)
        {
            case string vString:
                SerializeString(vString, s);
                return;
        }

        if (v != null && IsGenericList(v.GetType()))
        {
            var outList = new List<object>();
            if (v is IEnumerable enumerable)
            {
                foreach (var variable in enumerable)
                {
                    outList.Add(variable);
                }
            }
            SerializeList(outList, s);
        }
        
        // Object
        Serialize(v, v.GetType(), s, true);
    }

    public static T? Deserialize<T>(Stream s)
    {
        var t = typeof(T);
        return (T?)Deserialize(s, t);
    }

    public static object? Deserialize(Stream s, Type t, bool noError = false)
    {
        // Read first byte
        if (s.ReadByte() == 0) return null;
        
        if (t.GetCustomAttributes(typeof(SerializeClassAttribute), false).Length == 0)
            if (noError) return null;
            else throw new ArgumentException("This type does not have a SerializeClassAttribute");
        
        var objInst = Activator.CreateInstance(t);
        if (objInst == null) throw new NullReferenceException("Could not create instance of serializable object");
        
        foreach (var fieldInfo in t.GetFields())
        {
            if (fieldInfo.GetCustomAttributes(typeof(SerializeFieldAttribute), false).Length == 0) continue;
            DeserializeField(fieldInfo, s, objInst);
        }

        foreach (var propertyInfo in t.GetProperties())
        {
            if (propertyInfo.GetCustomAttributes(typeof(SerializeFieldAttribute), false).Length == 0) continue;
            DeserializeProperty(propertyInfo, s, objInst);
        }

        return objInst;
    }

    private static void DeserializeField(FieldInfo field, Stream s, object objInst)
    {
        field.SetValue(objInst, DeserializeValue(s, field.FieldType));
    }

    private static void DeserializeProperty(PropertyInfo property, Stream s, object objInst)
    {
        if (property.SetMethod == null || property.GetMethod == null)
            return;
        
        property.SetValue(objInst, DeserializeValue(s, property.PropertyType));
    }

    public static object? DeserializeValue(Stream s, Type t)
    {
        // Single byte types
        if (t == typeof(bool))
        {
            return s.ReadByte() == 1;
        }
        if (t == typeof(byte))
        {
            return (byte)s.ReadByte();
        }

        // 2 byte types
        if (t == typeof(short))
        {
            var buffer = new byte[2];
            s.Read(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }
        if (t == typeof(ushort))
        {
            var buffer = new byte[2];
            s.Read(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer, 0);
        }
        
        // 4 byte types
        // Fixed
        if (t == typeof(int))
        {
            var buffer = new byte[4];
            s.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }
        if (t == typeof(uint))
        {
            var buffer = new byte[4];
            s.Read(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }
        // Floating
        if (t == typeof(float))
        {
            var buffer = new byte[4];
            s.Read(buffer, 0, 4);
            return BitConverter.ToSingle(buffer, 0);
        }
        
        // 8 byte types
        // Fixed
        if (t == typeof(long))
        {
            var buffer = new byte[8];
            s.Read(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }
        if (t == typeof(ulong))
        {
            var buffer = new byte[8];
            s.Read(buffer, 0, 8);
            return BitConverter.ToUInt64(buffer, 0);
        }
        // Floating
        if (t == typeof(double))
        {
            var buffer = new byte[8];
            s.Read(buffer, 0, 8);
            return BitConverter.ToDouble(buffer, 0);
        }
        if (t == typeof(decimal))
        {
            var buffer = new byte[8];
            s.Read(buffer, 0, 8);
            return (decimal)BitConverter.ToInt64(buffer, 0);
        }
        
        // Dynamic size types
        if (t == typeof(string))
        {
            return DeserializeString(s);
        }

        if (IsGenericList(t))
        {
            var list = DeserializeList(s, t.GetGenericArguments()[0]);

            var listType = t.GetGenericArguments()[0];

            // return list.OfType<?>().ToList();
            // Reflection
            var function1 = typeof(Enumerable).GetMethod("OfType").MakeGenericMethod(listType);
            var function2 = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(listType);
            
            var result1 = function1.Invoke(null, new []{ list });
            var result2 = function2.Invoke(null, new []{ result1 });
            return result2;
        }

        return Deserialize(s, t, true);
    }
    
    private static bool IsGenericList(Type t)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);
    }
}