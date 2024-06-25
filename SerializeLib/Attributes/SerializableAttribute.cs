namespace SerializeLib.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false)]
public class SerializeClassAttribute : Attribute
{
    public SerializeClassAttribute() {}
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public class SerializeFieldAttribute : Attribute
{
    public SerializeFieldAttribute() {}
}