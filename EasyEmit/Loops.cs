using System;
using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit.Loops;

/// <summary>
/// Extension methods for ILBuilder to provide simple For and ForEach loop constructs
/// </summary>
public static class Loops
{
    /// <summary>
    /// Creates a simple for loop (for i = start; i < end; i++)
    /// </summary>
    /// <param name="il">The IL builder instance</param>
    /// <param name="start">The starting value of the counter</param>
    /// <param name="end">The exclusive upper bound of the counter</param>
    /// <param name="body">The loop body action that receives the current counter value</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public static ILBuilder For(this ILBuilder il, int start, int end, Action<ILBuilder> body)
    {
        return For(il, start, end, 1, (innerIl, counter) => body(innerIl));
    }

    /// <summary>
    /// Creates a simple for loop (for i = start; i < end; i += step)
    /// </summary>
    /// <param name="il">The IL builder instance</param>
    /// <param name="start">The starting value of the counter</param>
    /// <param name="end">The exclusive upper bound of the counter</param>
    /// <param name="step">The step increment (default: 1)</param>
    /// <param name="body">The loop body action that receives the current counter value</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public static ILBuilder For(this ILBuilder il, int start, int end, int step, Action<ILBuilder> body)
    {
        return For(il, start, end, step, (innerIl, counter) => body(innerIl));
    }

    /// <summary>
    /// Creates a simple for loop that passes the counter variable to the body
    /// </summary>
    /// <param name="il">The IL builder instance</param>
    /// <param name="start">The starting value of the counter</param>
    /// <param name="end">The exclusive upper bound of the counter</param>
    /// <param name="body">The loop body action that receives the current counter value</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public static ILBuilder ForCounter(this ILBuilder il, int start, int end, Action<ILBuilder, LocalVariableInfo> body)
    {
        return For(il, start, end, 1, body);
    }

    /// <summary>
    /// Creates a simple for loop that passes the counter variable to the body with custom step
    /// </summary>
    /// <param name="il">The IL builder instance</param>
    /// <param name="start">The starting value of the counter</param>
    /// <param name="end">The exclusive upper bound of the counter</param>
    /// <param name="step">The step increment</param>
    /// <param name="body">The loop body action that receives the current counter value</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public static ILBuilder For(this ILBuilder il, int start, int end, int step, Action<ILBuilder, LocalVariableInfo> body)
    {
        if (il._generator == null)
            throw new InvalidOperationException("ILGenerator not available");
        if (body == null)
            throw new ArgumentNullException(nameof(body));

        var counter = il.DeclareLocal<int>("i");
        var loopStart = il._generator.DefineLabel();
        var loopEnd = il._generator.DefineLabel();

        // Initialize counter: i = start
        il.LoadConstant(start).StoreLocal(counter);

        // Mark the start of the loop
        il._generator.MarkLabel(loopStart);

        // Check condition: i < end (for positive step) or i > end (for negative step)
        if (step > 0)
        {
            il.LoadLocal(counter).LoadConstant(end).Clt();
        }
        else
        {
            il.LoadLocal(counter).LoadConstant(end).Cgt();
        }

        // Branch if false to exit loop
        il._generator.Emit(OpCodes.Brfalse, loopEnd);

        // Execute loop body
        body(il, counter);

        // Increment: i += step
        il.LoadLocal(counter).LoadConstant(step).Add().StoreLocal(counter);

        // Jump back to condition check
        il._generator.Emit(OpCodes.Br, loopStart);

        // Mark the end of the loop
        il._generator.MarkLabel(loopEnd);

        return il;
    }

    /// <summary>
    /// Creates a simple foreach loop for arrays
    /// </summary>
    /// <typeparam name="T">The type of array elements</typeparam>
    /// <param name="il">The IL builder instance</param>
    /// <param name="getArray">Action that pushes array onto the evaluation stack</param>
    /// <param name="body">The loop body action that receives the current element</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public static ILBuilder ForEach<T>(this ILBuilder il, Action<ILBuilder> getArray, Action<ILBuilder, LocalVariableInfo> body)
    {
        if (il._generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        var array = il.DeclareLocal<T[]>("array");
        var index = il.DeclareLocal<int>("index");
        var length = il.DeclareLocal<int>("length");
        var element = il.DeclareLocal<T>("element");
        var loopStart = il._generator.DefineLabel();
        var loopEnd = il._generator.DefineLabel();

        // Initialize: getArray(), set index = 0, length = array.Length
        getArray(il);
        il._generator.Emit(OpCodes.Stloc, array.Builder);
        il._generator.Emit(OpCodes.Ldc_I4_0);
        il._generator.Emit(OpCodes.Stloc, index.Builder);
        il._generator.Emit(OpCodes.Ldloc, array.Builder);
        il._generator.Emit(OpCodes.Ldlen);
        il._generator.Emit(OpCodes.Stloc, length.Builder);

        // Mark the start of the loop
        il._generator.MarkLabel(loopStart);

        // Check condition: index < length
        il._generator.Emit(OpCodes.Ldloc, index.Builder);
        il._generator.Emit(OpCodes.Ldloc, length.Builder);
        il._generator.Emit(OpCodes.Clt);
        il._generator.Emit(OpCodes.Brfalse, loopEnd);

        // Execute loop body: element = array[index]; body(element)
        il._generator.Emit(OpCodes.Ldloc, array.Builder);
        il._generator.Emit(OpCodes.Ldloc, index.Builder);
        il._generator.Emit(OpCodes.Ldelem, typeof(T));
        il._generator.Emit(OpCodes.Stloc, element.Builder);
        body(il, element);

        // Increment: index++
        il._generator.Emit(OpCodes.Ldloc, index.Builder);
        il._generator.Emit(OpCodes.Ldc_I4_1);
        il._generator.Emit(OpCodes.Add);
        il._generator.Emit(OpCodes.Stloc, index.Builder);

        // Jump back to condition check
        il._generator.Emit(OpCodes.Br, loopStart);

        // Mark the end of the loop
        il._generator.MarkLabel(loopEnd);

        return il;
    }

    /// <summary>
    /// Creates a simple foreach loop that automatically declares the element variable
    /// </summary>
    /// <typeparam name="T">The type of array elements</typeparam>
    /// <param name="il">The IL builder instance</param>
    /// <param name="getArray">Action that pushes array onto the evaluation stack</param>
    /// <param name="body">The loop body action that receives the current element</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public static ILBuilder ForEachElement<T>(this ILBuilder il, Action<ILBuilder> getArray, Action<ILBuilder> body)
    {
        return ForEach<T>(il, getArray, (innerIl, element) => body(innerIl));
    }
}