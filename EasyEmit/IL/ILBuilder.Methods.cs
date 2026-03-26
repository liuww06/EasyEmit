using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Method call extensions for ILBuilder
/// </summary>
public partial class ILBuilder
{
    /// <summary>
    /// Calls a static method
    /// </summary>
    public ILBuilder CallStatic(MethodInfo methodInfo)
    {
        if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
        Generator.Emit(OpCodes.Call, methodInfo);
        return this;
    }

    /// <summary>
    /// Calls an instance method
    /// </summary>
    public ILBuilder CallVirtual(MethodInfo methodInfo)
    {
        if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
        Generator.Emit(OpCodes.Callvirt, methodInfo);
        return this;
    }

    /// <summary>
    /// Calls a method (generic call that works with both static and instance methods)
    /// </summary>
    public ILBuilder Call(MethodInfo methodInfo)
    {
        if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
        Generator.Emit(OpCodes.Call, methodInfo);
        return this;
    }

    /// <summary>
    /// Calls a constructor
    /// </summary>
    public ILBuilder NewObj(ConstructorInfo constructorInfo)
    {
        if (constructorInfo == null) throw new ArgumentNullException(nameof(constructorInfo));
        Generator.Emit(OpCodes.Newobj, constructorInfo);
        return this;
    }

    /// <summary>
    /// Stores a value in an array element
    /// </summary>
    public ILBuilder StoreElement(Type elementType)
    {
        if (elementType == null) throw new ArgumentNullException(nameof(elementType));
        Generator.Emit(OpCodes.Stelem, elementType);
        return this;
    }

    /// <summary>
    /// Loads an array element onto the evaluation stack
    /// </summary>
    public ILBuilder LoadElement(Type elementType)
    {
        if (elementType == null) throw new ArgumentNullException(nameof(elementType));
        Generator.Emit(OpCodes.Ldelem, elementType);
        return this;
    }
}
