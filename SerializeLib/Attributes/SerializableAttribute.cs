namespace SerializeLib.Attributes;

/// <summary>
/// An attribute which specifies that the targeted class is serializable using SerializeLib.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false)]
public class SerializeClassAttribute : Attribute
{
    public SerializeClassAttribute() {}
}

/// <summary>
/// An attribute which specifies that the targeted field or property is serializable using SerializeLib.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public class SerializeFieldAttribute : Attribute
{
    public SerializeFieldAttribute() {}
}