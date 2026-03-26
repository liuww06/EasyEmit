using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Fluent builder for creating dynamic methods
/// </summary>
public class DynamicMethodBuilder
{
    private readonly DelayedMethodDefinition _methodDefinition;
    private Action<ILBuilder>? _bodyAction;

    internal DynamicMethodBuilder(DelayedMethodDefinition methodDefinition)
    {
        _methodDefinition = methodDefinition;
    }

    /// <summary>
    /// Gets the IL generator for writing method body using a fluent API
    /// </summary>
    public DynamicMethodBuilder Body(Action<ILBuilder> bodyAction)
    {
        _bodyAction = bodyAction;
        _methodDefinition.SetBodyAction(methodBuilder =>
        {
            var ilGenerator = methodBuilder.GetILGenerator();
            var ilBuilder = new ILBuilder(ilGenerator);
            bodyAction(ilBuilder);
        });
        return this;
    }
}
