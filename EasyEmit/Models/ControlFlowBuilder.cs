using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Builder for control flow constructs like if/else
/// </summary>
public class ControlFlowBuilder
{
    private readonly ILBuilder _ilBuilder;
    private readonly Label _elseLabel;
    private readonly Label _endLabel;

    internal ControlFlowBuilder(ILBuilder ilBuilder, Label elseLabel, Label endLabel)
    {
        _ilBuilder = ilBuilder;
        _elseLabel = elseLabel;
        _endLabel = endLabel;
    }

    /// <summary>
    /// Adds an else clause to the if statement
    /// </summary>
    /// <param name="falseBody">The action to execute when the condition is false</param>
    /// <returns>The IL builder for continuing with the method body</returns>
    public ILBuilder Else(Action<ILBuilder> falseBody)
    {
        // Branch to skip the else block when condition was true
        _ilBuilder.Generator.Emit(OpCodes.Br, _endLabel);

        // Mark the start of the else block
        _ilBuilder.Generator.MarkLabel(_elseLabel);

        // Execute else body
        falseBody(_ilBuilder);

        // Mark the end of the entire if/else
        _ilBuilder.Generator.MarkLabel(_endLabel);

        return _ilBuilder;
    }
}
