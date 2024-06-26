﻿using System.Collections;
using System.Text;

namespace SerializeLib;

public static partial class Serializer
{
    // Serialized as [item count][item size][item content][item size][item content] etc
    private static void SerializeList<T>(List<T> list, Stream s)
    {
        var countSerialized = BitConverter.GetBytes(list.Count);
        s.Write(countSerialized); // Item count
        
        foreach (var item in list)
        {
            SerializeValue(item, s, typeof(T));
        }
    }

    private static List<object> DeserializeList(Stream s, Type t)
    {
        var countBytes = new byte[4];
        s.Read(countBytes, 0, countBytes.Length); // Item count
        var count = BitConverter.ToInt32(countBytes, 0);
        var list = new List<object>();

        for (int i = 0; i < count; i++)
        {
            list.Add(DeserializeValue(s, t));
        }
        
        return list;
    }
}