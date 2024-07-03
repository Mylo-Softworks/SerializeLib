namespace SerializeLib.Attributes;

/// <summary>
/// An attribute which specifies that the targeted class is serializable using SerializeLib.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false)]
public class SerializeClassAttribute : Attribute
{
    
}

/// <summary>
/// An attribute which specifies that the targeted field or property is serializable using SerializeLib.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public class SerializeFieldAttribute : Attribute
{
    /// <summary>
    /// Indicate an order for this attribute
    /// </summary>
    public int Order;

    public SerializeFieldAttribute(int order)
    {
        Order = order;
    }
}