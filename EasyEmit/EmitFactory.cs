namespace EasyEmit;

/// <summary>
/// Factory class for creating dynamic assemblies using EasyEmit
/// </summary>
public static class EmitFactory
{
    /// <summary>
    /// Creates a new dynamic assembly builder with the specified name
    /// </summary>
    /// <param name="assemblyName">The name of the dynamic assembly</param>
    /// <returns>A fluent assembly builder</returns>
    public static DynamicAssemblyBuilder NewAssembly(string assemblyName)
    {
        return new DynamicAssemblyBuilder(assemblyName);
    }
}
