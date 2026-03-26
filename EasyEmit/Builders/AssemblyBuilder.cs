using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Fluent builder for creating dynamic assemblies
/// </summary>
public class DynamicAssemblyBuilder
{
    private readonly string _name;
    private readonly AssemblyBuilderAccess _access;
    private AssemblyBuilder? _assemblyBuilder;
    private ModuleBuilder? _moduleBuilder;

    internal DynamicAssemblyBuilder(string name, AssemblyBuilderAccess access = AssemblyBuilderAccess.RunAndCollect)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Assembly name cannot be null or empty.", nameof(name));

        _name = name;
        _access = access;
    }

    /// <summary>
    /// Defines a new type in the assembly
    /// </summary>
    /// <param name="typeName">The name of the type to define</param>
    /// <param name="typeAttributes">The type attributes (default: Public, Class)</param>
    /// <returns>A fluent type builder</returns>
    public DynamicTypeBuilder DefineType(string typeName, TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.Class)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));

        return new DynamicTypeBuilder(this, typeName, typeAttributes);
    }

    /// <summary>
    /// Builds the assembly and returns the dynamically created assembly
    /// </summary>
    /// <returns>The built assembly</returns>
    public Assembly Build()
    {
        EnsureAssemblyCreated();
        return _assemblyBuilder!;
    }

    /// <summary>
    /// Gets the underlying AssemblyBuilder (creates if needed)
    /// </summary>
    internal AssemblyBuilder GetAssemblyBuilder()
    {
        EnsureAssemblyCreated();
        return _assemblyBuilder!;
    }

    /// <summary>
    /// Gets the module builder for creating types (creates if needed)
    /// </summary>
    internal ModuleBuilder GetModuleBuilder()
    {
        EnsureAssemblyCreated();
        return _moduleBuilder!;
    }

    private void EnsureAssemblyCreated()
    {
        if (_assemblyBuilder == null)
        {
            var assemblyName = new AssemblyName(_name);
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, _access);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule");
        }
    }
}
