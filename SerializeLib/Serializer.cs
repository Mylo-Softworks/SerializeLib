using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using SerializeLib.Attributes;
using SerializeLib.Interfaces;

namespace SerializeLib;

public static partial class Serializer
{
    // First byte of object is 0 when null, 1 when not null
    
    /// <summary>
    /// Serialize an object to a stream (type specified as generic).
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="s">The stream to write to.</param>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    public static void Serialize<T>(T obj, Stream s)
    {
        var t = typeof(T);
        Serialize(obj, t, s);
    }

    private static bool IsManualSerialized(Type t) =>
        t.GetInterfaces().Any(x =>
            x.IsGenericType &&
            x.GetGenericTypeDefinition() == typeof(ISerializableClass<>));

    private static Dictionary<Type, object> _overrides = new ();

    /// <summary>
    /// Register a new override to be used.
    /// </summary>
    /// <param name="o">The override to use.</param>
    /// <typeparam name="T">The type to override for.</typeparam>
    public static void RegisterOverride<T>(ISerializableOverride<T>? o)
    {
        if (o == null)
        {
            _overrides.Remove(typeof(T));
            return;
        }
        _overrides[typeof(T)] = o;
    }

    /// <summary>
    /// Register a new override to be used.
    /// </summary>
    /// <param name="o"></param>
    public static void RegisterOverride<T, TS>() where T : ISerializableOverride<TS>
    {
        var o = Activator.CreateInstance(typeof(T))!;
        _overrides[typeof(TS)] = o;
    }

    private static bool IsRegisteredOverride(Type t) =>
        _overrides.ContainsKey(t);

    private static bool NullIndicatorNeeded(Type t) =>
        !t.IsValueType;
    
    
    /// <summary>
    /// Serialize an object to a stream (type specified as argument).
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="t">The type of the object to serialize.</param>
    /// <param name="s">The stream to write to.</param>
    /// <param name="noError">Return when no attribute or interface is found instead of giving an error. You probably don't want to change this.</param>
    /// <exception cref="ArgumentException">If the type isn't ISerializableClass and doesn't have SerializeClassAttribute.</exception>
    public static void Serialize(object? obj, Type t, Stream s, bool noError = false)
    {
        if (IsManualSerialized(t))
        {
            t.GetMethod("Serialize")!.Invoke(obj, new object[] { s });
            return;
        }

        if (IsRegisteredOverride(t))
        {
            var over = _overrides[t];
            over.GetType().GetMethod("Serialize")!.Invoke(over, new object[] { obj, s });
            return;
        }
        
        if (t.GetCustomAttributes(typeof(SerializeClassAttribute), false).Length == 0)
            if (noError) return;
            else throw new ArgumentException("This type does not have a SerializeClassAttribute");

        // Null indicator
        if (NullIndicatorNeeded(t))
        {
            if (obj == null)
            {
                s.WriteByte(0);
                return;
            }
            s.WriteByte(1);
        }

        // Fields
        var fields = new List<(int, FieldInfo)>();
        foreach (var fieldInfo in t.GetFields())
        {
            var attribute = fieldInfo.GetCustomAttribute(typeof(SerializeFieldAttribute), false);
            if (attribute == null) continue;
            fields.Add(((attribute as SerializeFieldAttribute).Order, fieldInfo));
        }
        
        fields.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        foreach (var fieldInfo in fields.Select(tuple => tuple.Item2))
        {
            SerializeField(fieldInfo, obj, s);
        }

        // Properties
        var properties = new List<(int, PropertyInfo)>();
        foreach (var propertyInfo in t.GetProperties())
        {
            var attribute = propertyInfo.GetCustomAttribute(typeof(SerializeFieldAttribute), false);
            if (attribute == null) continue;
            
            properties.Add(((attribute as SerializeFieldAttribute).Order, propertyInfo));
        }
        
        properties.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        foreach (var propertyInfo in properties.Select(tuple => tuple.Item2))
        {
            SerializeProperty(propertyInfo, obj, s);
        }
        
        s.Flush();
    }

    private static void SerializeField(FieldInfo field, object obj, Stream s)
    {
        var val = field.GetValue(obj);
        SerializeValue(val, s, field.FieldType);
    }

    private static void SerializeProperty(PropertyInfo property, object obj, Stream s)
    {
        if (property.SetMethod == null || property.GetMethod == null)
            return;
        
        var val = property.GetValue(obj, null);
        SerializeValue(val, s, property.PropertyType);
    }

    /// <summary>
    /// Serialize a single value (type specified as generic).
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="s">The stream to write to.</param>
    /// <typeparam name="T">The type of the value to be serialized.</typeparam>
    public static void SerializeValue<T>(T value, Stream s)
    {
        SerializeValue(value, s, typeof(T));
    }

    /// <summary>
    /// Serialize a signel value (type specified as argument).
    /// </summary>
    /// <param name="v">The value to serialize.</param>
    /// <param name="s">The stream to write to.</param>
    /// <param name="t">The type of the value to be serialized.</param>
    public static void SerializeValue(object? v, Stream s, Type t)
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

        if (IsGenericList(t) || IsGenericArray(t))
        {
            if (v == null)
            {
                var zeroLen = new byte[] { 0, 0, 0, 0 };
                s.Write(zeroLen);
                return;
            }
            var outList = new List<object>();
            if (v is IEnumerable enumerable)
            {
                foreach (var variable in enumerable)
                {
                    outList.Add(variable);
                }
            }
            SerializeList(outList, s);
            return;
        }

        if (v == null)
        {
            s.WriteByte(0);
            return;
        }
        
        // Object
        Serialize(v, v.GetType(), s, true);
    }

    /// <summary>
    /// Deserialize an object (generic).
    /// </summary>
    /// <param name="s">The stream to read from.</param>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <returns>The deserialized object.</returns>
    public static T? Deserialize<T>(Stream s)
    {
        var t = typeof(T);
        return (T?)Deserialize(s, t);
    }

    /// <summary>
    /// Deserialize an object (not generic)
    /// </summary>
    /// <param name="s">The stream to read from.</param>
    /// <param name="t">The type to deserialize into.</param>
    /// <param name="noError">Return when no attribute or interface is found instead of giving an error. You probably don't want to change this.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="ArgumentException">If the type isn't ISerializableClass and doesn't have SerializeClassAttribute.</exception>
    /// <exception cref="NullReferenceException">If the type cannot be instantiated without arguments. (No parameterless constructor available.)</exception>
    public static object? Deserialize(Stream s, Type t, bool noError = false)
    {
        if (IsManualSerialized(t))
        {
            return t.GetMethod("Deserialize")!.Invoke(Activator.CreateInstance(t), new object[] { s }); // Creates a new instance, values can be assigned. Returning this as the value is not a requirement.
        }
        
        if (IsRegisteredOverride(t))
        {
            var over = _overrides[t];
            return over.GetType().GetMethod("Deserialize")!.Invoke(over, new object[] { s });
        }
        
        if (t.GetCustomAttributes(typeof(SerializeClassAttribute), false).Length == 0)
            if (noError) return null;
            else throw new ArgumentException("This type does not have a SerializeClassAttribute");

        // Null check
        if (NullIndicatorNeeded(t))
        {
            // Read first byte
            if (s.ReadByte() == 0) return null;
        }
        
        var objInst = Activator.CreateInstance(t);
        if (objInst == null) throw new NullReferenceException("Could not create instance of serializable object");
        
        // Fields
        var fields = new List<(int, FieldInfo)>();
        foreach (var fieldInfo in t.GetFields())
        {
            var attribute = fieldInfo.GetCustomAttribute(typeof(SerializeFieldAttribute), false);
            if (attribute == null) continue;
            fields.Add(((attribute as SerializeFieldAttribute).Order, fieldInfo));
        }
        
        fields.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        foreach (var fieldInfo in fields.Select(tuple => tuple.Item2))
        {
            DeserializeField(fieldInfo, s, objInst);
        }

        // Properties
        var properties = new List<(int, PropertyInfo)>();
        foreach (var propertyInfo in t.GetProperties())
        {
            var attribute = propertyInfo.GetCustomAttribute(typeof(SerializeFieldAttribute), false);
            if (attribute == null) continue;
            
            properties.Add(((attribute as SerializeFieldAttribute).Order, propertyInfo));
        }
        
        properties.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        foreach (var propertyInfo in properties.Select(tuple => tuple.Item2))
        {
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

    /// <summary>
    /// Deserialize a single value (Type specified as generic).
    /// </summary>
    /// <param name="s">The stream to read from.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized value.</returns>
    public static T? DeserializeValue<T>(Stream s)
    {
        return (T?)DeserializeValue(s, typeof(T));
    }

    /// <summary>
    /// Deserialize a single value (Type specified as argument).
    /// </summary>
    /// <param name="s">The stream to read from.</param>
    /// <param name="t">The type to deserialize to.</param>
    /// <returns>The deserialized value.</returns>
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
            var size = sizeof(short);
            var buffer = new byte[size];
            s.Read(buffer, 0, size);
            return BitConverter.ToInt16(buffer, 0);
        }
        if (t == typeof(ushort))
        {
            var size = sizeof(ushort);
            var buffer = new byte[size];
            s.Read(buffer, 0, size);
            return BitConverter.ToUInt16(buffer, 0);
        }
        
        // 4 byte types
        // Fixed
        if (t == typeof(int))
        {
            var size = sizeof(int);
            var buffer = new byte[size];
            s.Read(buffer, 0, size);
            return BitConverter.ToInt32(buffer, 0);
        }
        if (t == typeof(uint))
        {
            var size = sizeof(uint);
            var buffer = new byte[size];
            s.Read(buffer, 0, size);
            return BitConverter.ToUInt32(buffer, 0);
        }
        // Floating
        if (t == typeof(float))
        {
            var size = sizeof(float);
            var buffer = new byte[size];
            s.Read(buffer, 0, size);
            return BitConverter.ToSingle(buffer, 0);
        }
        
        // 8 byte types
        // Fixed
        if (t == typeof(long))
        {
            var size = sizeof(long);
            var buffer = new byte[size];
            s.Read(buffer, 0, size);
            return BitConverter.ToInt64(buffer, 0);
        }
        if (t == typeof(ulong))
        {
            var size = sizeof(ulong);
            var buffer = new byte[size];
            s.Read(buffer, 0, size);
            return BitConverter.ToUInt64(buffer, 0);
        }
        // Floating
        if (t == typeof(double))
        {
            var size = sizeof(double);
            var buffer = new byte[size];
            s.Read(buffer, 0, size);
            return BitConverter.ToDouble(buffer, 0);
        }
        if (t == typeof(decimal))
        {
            var size = sizeof(double); // As double because BitConverter doesn't support decimal
            var buffer = new byte[size];
            s.Read(buffer, 0, size);
            return (decimal)BitConverter.ToDouble(buffer, 0);
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
            var function1 = typeof(Enumerable).GetMethod("OfType")!.MakeGenericMethod(listType);
            var function2 = typeof(Enumerable).GetMethod("ToList")!.MakeGenericMethod(listType);
            
            var result1 = function1.Invoke(null, new []{ list });
            var result2 = function2.Invoke(null, new []{ result1 });
            return result2;
        }

        if (IsGenericArray(t))
        {
            var elementType = t.GetElementType();
            var list = DeserializeList(s, elementType);
            
            var function1 = typeof(Enumerable).GetMethod("OfType")!.MakeGenericMethod(elementType);
            var function2 = typeof(Enumerable).GetMethod("ToArray")!.MakeGenericMethod(elementType);
            
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

    private static bool IsGenericArray(Type t)
    {
        return t.IsArray;
    }
}