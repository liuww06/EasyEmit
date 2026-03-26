using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Loop extensions for ILBuilder (ForEach over arrays and enumerables)
/// </summary>
public partial class ILBuilder
{
    /// <summary>
    /// Creates a foreach loop for arrays where the array is provided by an action.
    /// </summary>
    /// <typeparam name="T">The type of array elements</typeparam>
    /// <param name="getArray">Action that pushes array onto the evaluation stack</param>
    /// <param name="body">The loop body action that receives the current element</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder ForEach<T>(Action<ILBuilder> getArray, Action<ILBuilder, LocalVariable> body)
    {
        if (getArray == null) throw new ArgumentNullException(nameof(getArray));
        if (body == null) throw new ArgumentNullException(nameof(body));

        var array = DeclareLocal<T[]>("array");
        var index = DeclareLocal<int>("index");
        var length = DeclareLocal<int>("length");
        var element = DeclareLocal<T>("element");
        var loopStart = Generator.DefineLabel();
        var loopEnd = Generator.DefineLabel();

        getArray(this);
        Generator.Emit(OpCodes.Stloc, array.Builder);

        Generator.Emit(OpCodes.Ldc_I4_0);
        Generator.Emit(OpCodes.Stloc, index.Builder);

        Generator.Emit(OpCodes.Ldloc, array.Builder);
        Generator.Emit(OpCodes.Ldlen);
        Generator.Emit(OpCodes.Stloc, length.Builder);

        Generator.MarkLabel(loopStart);

        Generator.Emit(OpCodes.Ldloc, index.Builder);
        Generator.Emit(OpCodes.Ldloc, length.Builder);
        Generator.Emit(OpCodes.Clt);
        Generator.Emit(OpCodes.Brfalse, loopEnd);

        Generator.Emit(OpCodes.Ldloc, array.Builder);
        Generator.Emit(OpCodes.Ldloc, index.Builder);
        Generator.Emit(OpCodes.Ldelem, typeof(T));
        Generator.Emit(OpCodes.Stloc, element.Builder);

        body(this, element);

        Generator.Emit(OpCodes.Ldloc, index.Builder);
        Generator.Emit(OpCodes.Ldc_I4_1);
        Generator.Emit(OpCodes.Add);
        Generator.Emit(OpCodes.Stloc, index.Builder);

        Generator.Emit(OpCodes.Br, loopStart);
        Generator.MarkLabel(loopEnd);

        return this;
    }

    /// <summary>
    /// Creates a foreach loop for any generic enumerable collection using the enumerator pattern.
    /// Properly disposes the enumerator if it implements IDisposable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable</typeparam>
    /// <param name="getEnumerable">Action that pushes the enumerable collection onto the evaluation stack</param>
    /// <param name="body">The loop body action that receives the current element</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder ForEachEnumerable<T>(Action<ILBuilder> getEnumerable, Action<ILBuilder, LocalVariable> body)
    {
        if (getEnumerable == null) throw new ArgumentNullException(nameof(getEnumerable));
        if (body == null) throw new ArgumentNullException(nameof(body));

        var enumerable = DeclareLocal(typeof(System.Collections.Generic.IEnumerable<T>), "enumerable");
        var enumerator = DeclareLocal(typeof(System.Collections.Generic.IEnumerator<T>), "enumerator");
        var element = DeclareLocal<T>("element");
        var loopStart = Generator.DefineLabel();
        var loopEnd = Generator.DefineLabel();

        getEnumerable(this);
        Generator.Emit(OpCodes.Stloc, enumerable.Builder);

        Generator.Emit(OpCodes.Ldloc, enumerable.Builder);
        Generator.Emit(OpCodes.Callvirt, typeof(System.Collections.Generic.IEnumerable<T>).GetMethod("GetEnumerator")!);
        Generator.Emit(OpCodes.Stloc, enumerator.Builder);

        Generator.MarkLabel(loopStart);

        Generator.Emit(OpCodes.Ldloc, enumerator.Builder);
        Generator.Emit(OpCodes.Callvirt, typeof(System.Collections.IEnumerator).GetMethod("MoveNext")!);
        Generator.Emit(OpCodes.Brfalse, loopEnd);

        Generator.Emit(OpCodes.Ldloc, enumerator.Builder);
        Generator.Emit(OpCodes.Callvirt, typeof(System.Collections.Generic.IEnumerator<T>).GetProperty("Current")!.GetGetMethod()!);
        Generator.Emit(OpCodes.Stloc, element.Builder);

        body(this, element);

        Generator.Emit(OpCodes.Br, loopStart);
        Generator.MarkLabel(loopEnd);

        EmitDisposeEnumerator(enumerator);

        return this;
    }

    /// <summary>
    /// Creates a foreach loop for any non-generic enumerable collection using the enumerator pattern.
    /// Properly disposes the enumerator if it implements IDisposable.
    /// </summary>
    /// <param name="elementType">The type of elements in the enumerable</param>
    /// <param name="getEnumerable">Action that pushes the enumerable collection onto the evaluation stack</param>
    /// <param name="body">The loop body action that receives the current element</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder ForEachEnumerable(Type elementType, Action<ILBuilder> getEnumerable, Action<ILBuilder, LocalVariable> body)
    {
        if (elementType == null) throw new ArgumentNullException(nameof(elementType));
        if (getEnumerable == null) throw new ArgumentNullException(nameof(getEnumerable));
        if (body == null) throw new ArgumentNullException(nameof(body));

        var enumerable = DeclareLocal(typeof(System.Collections.IEnumerable), "enumerable");
        var enumerator = DeclareLocal(typeof(System.Collections.IEnumerator), "enumerator");
        var element = DeclareLocal(elementType, "element");
        var loopStart = Generator.DefineLabel();
        var loopEnd = Generator.DefineLabel();

        getEnumerable(this);
        Generator.Emit(OpCodes.Stloc, enumerable.Builder);

        Generator.Emit(OpCodes.Ldloc, enumerable.Builder);
        CallVirtual(typeof(System.Collections.IEnumerable).GetMethod("GetEnumerator")!);
        Generator.Emit(OpCodes.Stloc, enumerator.Builder);

        Generator.MarkLabel(loopStart);

        Generator.Emit(OpCodes.Ldloc, enumerator.Builder);
        CallVirtual(typeof(System.Collections.IEnumerator).GetMethod("MoveNext")!);
        Generator.Emit(OpCodes.Brfalse, loopEnd);

        Generator.Emit(OpCodes.Ldloc, enumerator.Builder);
        CallVirtual(typeof(System.Collections.IEnumerator).GetProperty("Current")!.GetGetMethod()!);

        if (elementType.IsValueType)
            Generator.Emit(OpCodes.Unbox_Any, elementType);
        else
            Generator.Emit(OpCodes.Castclass, elementType);

        Generator.Emit(OpCodes.Stloc, element.Builder);

        body(this, element);

        Generator.Emit(OpCodes.Br, loopStart);
        Generator.MarkLabel(loopEnd);

        EmitDisposeEnumerator(enumerator);

        return this;
    }

    /// <summary>
    /// Emits the dispose pattern for an enumerator: checks if IDisposable and calls Dispose.
    /// </summary>
    private void EmitDisposeEnumerator(LocalVariable enumerator)
    {
        var disposable = DeclareLocal(typeof(IDisposable), "disposable");
        var skipDispose = Generator.DefineLabel();
        var disposableType = typeof(IDisposable);

        Generator.Emit(OpCodes.Ldloc, enumerator.Builder);
        Generator.Emit(OpCodes.Isinst, disposableType);
        Generator.Emit(OpCodes.Stloc, disposable.Builder);
        Generator.Emit(OpCodes.Ldloc, disposable.Builder);
        Generator.Emit(OpCodes.Brfalse_S, skipDispose);
        Generator.Emit(OpCodes.Ldloc, disposable.Builder);
        Generator.Emit(OpCodes.Callvirt, disposableType.GetMethod("Dispose")!);
        Generator.MarkLabel(skipDispose);
    }
}
