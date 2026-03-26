using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Represents information about a declared local variable
/// </summary>
public class LocalVariable
{
    internal LocalBuilder Builder { get; }
    public string Name { get; }
    public Type LocalType => Builder.LocalType;

    internal LocalVariable(LocalBuilder builder, string name)
    {
        Builder = builder;
        Name = name;
    }
}
