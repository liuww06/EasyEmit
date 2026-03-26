using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Field operation extensions for ILBuilder
/// </summary>
public partial class ILBuilder
{
    /// <summary>
    /// Loads an instance field onto the evaluation stack
    /// </summary>
    public ILBuilder LoadField(FieldInfo fieldInfo)
    {
        if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));
        Generator.Emit(OpCodes.Ldfld, fieldInfo);
        return this;
    }

    /// <summary>
    /// Stores a value in an instance field from the evaluation stack
    /// </summary>
    public ILBuilder StoreField(FieldInfo fieldInfo)
    {
        if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));
        Generator.Emit(OpCodes.Stfld, fieldInfo);
        return this;
    }

    /// <summary>
    /// Loads a static field onto the evaluation stack
    /// </summary>
    public ILBuilder LoadStaticField(FieldInfo fieldInfo)
    {
        if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));
        Generator.Emit(OpCodes.Ldsfld, fieldInfo);
        return this;
    }

    /// <summary>
    /// Stores a value in a static field from the evaluation stack
    /// </summary>
    public ILBuilder StoreStaticField(FieldInfo fieldInfo)
    {
        if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));
        Generator.Emit(OpCodes.Stsfld, fieldInfo);
        return this;
    }
}
