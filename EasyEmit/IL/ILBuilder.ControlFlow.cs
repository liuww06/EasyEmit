using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Control flow extensions for ILBuilder
/// </summary>
public partial class ILBuilder
{
    /// <summary>
    /// Creates an if statement where the condition callback must leave exactly one bool value on the stack.
    /// For a safer API, use the If(left, comparison, right, trueBody) overload.
    /// </summary>
    /// <param name="condition">The condition action that leaves one bool value on the evaluation stack</param>
    /// <param name="trueBody">The action to execute when the condition is true</param>
    /// <returns>A control flow builder for chaining else/if statements</returns>
    public ControlFlowBuilder If(Action<ILBuilder> condition, Action<ILBuilder> trueBody)
    {
        if (condition == null) throw new ArgumentNullException(nameof(condition));
        if (trueBody == null) throw new ArgumentNullException(nameof(trueBody));

        var elseLabel = Generator.DefineLabel();
        var endLabel = Generator.DefineLabel();

        condition(this);
        Generator.Emit(OpCodes.Brfalse, elseLabel);

        trueBody(this);

        return new ControlFlowBuilder(this, elseLabel, endLabel);
    }

    /// <summary>
    /// Creates an if statement with explicit left operand, comparison, and right operand.
    /// This is the safer API that ensures correct stack behavior.
    /// </summary>
    /// <param name="left">Action that pushes the left operand onto the evaluation stack</param>
    /// <param name="comparison">A comparison function (e.g., ILBuilder.Clt, ILBuilder.Cgt, ILBuilder.Equal)</param>
    /// <param name="right">Action that pushes the right operand onto the evaluation stack</param>
    /// <param name="trueBody">The action to execute when the condition is true</param>
    /// <returns>A control flow builder for chaining else/if statements</returns>
    public ControlFlowBuilder If(
        Action<ILBuilder> left,
        Func<ILBuilder, ILBuilder> comparison,
        Action<ILBuilder> right,
        Action<ILBuilder> trueBody)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (comparison == null) throw new ArgumentNullException(nameof(comparison));
        if (right == null) throw new ArgumentNullException(nameof(right));
        if (trueBody == null) throw new ArgumentNullException(nameof(trueBody));

        return If(
            condition: il => { left(il); right(il); comparison(il); },
            trueBody: trueBody);
    }

    /// <summary>
    /// Creates a while loop with the specified condition and body.
    /// The condition callback must leave one bool value on the evaluation stack.
    /// </summary>
    /// <param name="condition">The condition to check each iteration</param>
    /// <param name="body">The loop body action</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder While(Action<ILBuilder> condition, Action<ILBuilder> body)
    {
        if (condition == null) throw new ArgumentNullException(nameof(condition));
        if (body == null) throw new ArgumentNullException(nameof(body));

        var loopStart = Generator.DefineLabel();
        var loopEnd = Generator.DefineLabel();

        Generator.MarkLabel(loopStart);

        condition(this);
        Generator.Emit(OpCodes.Brfalse, loopEnd);

        body(this);
        Generator.Emit(OpCodes.Br, loopStart);

        Generator.MarkLabel(loopEnd);

        return this;
    }

    /// <summary>
    /// Creates a for loop with explicit initialization, condition, increment, and body.
    /// The condition callback must leave one bool value on the evaluation stack.
    /// </summary>
    /// <param name="initializer">The initialization action</param>
    /// <param name="condition">The condition to check each iteration</param>
    /// <param name="increment">The increment action to execute after each iteration</param>
    /// <param name="body">The loop body action</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder For(Action<ILBuilder> initializer, Action<ILBuilder> condition, Action<ILBuilder> increment, Action<ILBuilder> body)
    {
        if (initializer == null) throw new ArgumentNullException(nameof(initializer));
        if (condition == null) throw new ArgumentNullException(nameof(condition));
        if (increment == null) throw new ArgumentNullException(nameof(increment));
        if (body == null) throw new ArgumentNullException(nameof(body));

        var loopStart = Generator.DefineLabel();
        var loopEnd = Generator.DefineLabel();

        initializer(this);

        Generator.MarkLabel(loopStart);
        condition(this);
        Generator.Emit(OpCodes.Brfalse, loopEnd);

        body(this);
        increment(this);

        Generator.Emit(OpCodes.Br, loopStart);
        Generator.MarkLabel(loopEnd);

        return this;
    }

    /// <summary>
    /// Creates a for loop with standard integer counter: for i = start; i &lt; end; i++
    /// </summary>
    /// <param name="start">The starting value of the counter</param>
    /// <param name="end">The exclusive upper bound of the counter</param>
    /// <param name="body">The loop body action that receives the current counter variable</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder For(int start, int end, Action<ILBuilder, LocalVariable> body)
    {
        return For(start, end, 1, body);
    }

    /// <summary>
    /// Creates a for loop with standard integer counter and custom step.
    /// For positive step: i = start; i &lt; end; i += step
    /// For negative step: i = start; i &gt; end; i += step
    /// </summary>
    /// <param name="start">The starting value of the counter</param>
    /// <param name="end">The end bound of the counter (exclusive for positive step, exclusive for negative step)</param>
    /// <param name="step">The increment value (can be negative)</param>
    /// <param name="body">The loop body action that receives the current counter variable</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder For(int start, int end, int step, Action<ILBuilder, LocalVariable> body)
    {
        if (body == null) throw new ArgumentNullException(nameof(body));
        if (step == 0) throw new ArgumentException("Step cannot be zero.", nameof(step));

        var counter = DeclareLocal<int>("i");
        var loopStart = Generator.DefineLabel();
        var loopEnd = Generator.DefineLabel();

        LoadConstant(start).StoreLocal(counter);

        Generator.MarkLabel(loopStart);

        // Check condition: positive step uses Clt, negative uses Cgt
        LoadLocal(counter).LoadConstant(end);
        Generator.Emit(step > 0 ? OpCodes.Clt : OpCodes.Cgt);
        Generator.Emit(OpCodes.Brfalse, loopEnd);

        body(this, counter);

        LoadLocal(counter).LoadConstant(step).Add().StoreLocal(counter);

        Generator.Emit(OpCodes.Br, loopStart);
        Generator.MarkLabel(loopEnd);

        return this;
    }
}
