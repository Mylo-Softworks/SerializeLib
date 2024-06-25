﻿using System.Collections;
using System.Text;

namespace SerializeLib;

public static partial class Serializer
{
    // Serialized as [item count][item size][item content][item size][item content] etc
    private static void SerializeList<T>(List<T> list, Stream s)
    {
        Console.WriteLine("SerializeList");
        var countSerialized = BitConverter.GetBytes(list.Count);
        s.Write(countSerialized); // Item count
        
        foreach (var item in list)
        {
            var contentStream = new MemoryStream();
            SerializeValue(item, contentStream);
            
            // s.Write(BitConverter.GetBytes(contentStream.Length)); // Item size
            
            contentStream.Seek(0, SeekOrigin.Begin);
            contentStream.CopyTo(s); // Item content
        }
    }

    private static List<object> DeserializeList(Stream s, Type t)
    {
        Console.WriteLine("DeserializeList");
        var countBytes = new byte[4];
        s.Read(countBytes, 0, countBytes.Length); // Item count
        var count = BitConverter.ToInt32(countBytes, 0);
        var list = new List<object>();

        for (int i = 0; i < count; i++)
        {
            // var sizeBytes = new byte[4];
            // s.Read(sizeBytes, 0, sizeBytes.Length); // Item size
            // var size = BitConverter.ToInt32(sizeBytes, 0);
            
            // Console.WriteLine(size);

            list.Add(DeserializeValue(s, t));
        }
        
        return list;
    }
}