using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// High-level IL generation wrapper
/// </summary>
public partial class ILBuilder
{
    internal ILGenerator? _generator;
    private readonly Dictionary<string, LocalBuilder> _locals;

    internal ILBuilder(ILGenerator? generator)
    {
        _generator = generator;
        _locals = new Dictionary<string, LocalBuilder>();
    }

    /// <summary>
    /// Gets the ILGenerator or throws if not available
    /// </summary>
    internal ILGenerator Generator => _generator
        ?? throw new InvalidOperationException("ILGenerator not available. Use ILBuilder within a Body() callback.");

    #region Local Variable Management

    /// <summary>
    /// Declares a local variable with the specified type
    /// </summary>
    public LocalVariable DeclareLocal<T>(string? name = null)
    {
        return DeclareLocal(typeof(T), name);
    }

    /// <summary>
    /// Declares a local variable with the specified type
    /// </summary>
    public LocalVariable DeclareLocal(Type localType, string? name = null)
    {
        if (localType == null) throw new ArgumentNullException(nameof(localType));

        var localBuilder = Generator.DeclareLocal(localType);
        var localVarInfo = new LocalVariable(localBuilder, name ?? $"local_{localType.Name}_{_locals.Count}");
        _locals[localVarInfo.Name] = localBuilder;
        return localVarInfo;
    }

    #endregion

    #region Argument Loading

    /// <summary>
    /// Loads the argument at the specified index onto the evaluation stack
    /// </summary>
    public ILBuilder LoadArgument(int index)
    {
        switch (index)
        {
            case 0: Generator.Emit(OpCodes.Ldarg_0); break;
            case 1: Generator.Emit(OpCodes.Ldarg_1); break;
            case 2: Generator.Emit(OpCodes.Ldarg_2); break;
            case 3: Generator.Emit(OpCodes.Ldarg_3); break;
            default:
                if (index <= 255)
                    Generator.Emit(OpCodes.Ldarg_S, index);
                else
                    Generator.Emit(OpCodes.Ldarg, index);
                break;
        }
        return this;
    }

    /// <summary>
    /// Loads the "this" argument (index 0) onto the evaluation stack
    /// </summary>
    public ILBuilder LoadThis() => LoadArgument(0);

    #endregion

    #region Local Variable Operations

    /// <summary>
    /// Loads the specified local variable onto the evaluation stack
    /// </summary>
    public ILBuilder LoadLocal(LocalVariable localVar)
    {
        if (localVar == null) throw new ArgumentNullException(nameof(localVar));
        Generator.Emit(OpCodes.Ldloc, localVar.Builder);
        return this;
    }

    /// <summary>
    /// Loads the local variable at the specified index onto the evaluation stack
    /// </summary>
    public ILBuilder LoadLocal(int index)
    {
        Generator.Emit(OpCodes.Ldloc, index);
        return this;
    }

    /// <summary>
    /// Stores the top value from the evaluation stack in the specified local variable
    /// </summary>
    public ILBuilder StoreLocal(LocalVariable localVar)
    {
        if (localVar == null) throw new ArgumentNullException(nameof(localVar));
        Generator.Emit(OpCodes.Stloc, localVar.Builder);
        return this;
    }

    #endregion

    #region Constant Loading

    /// <summary>
    /// Loads an integer constant onto the evaluation stack
    /// </summary>
    public ILBuilder LoadConstant(int value)
    {
        Generator.Emit(OpCodes.Ldc_I4, value);
        return this;
    }

    /// <summary>
    /// Loads a string constant onto the evaluation stack
    /// </summary>
    public ILBuilder LoadConstant(string? value)
    {
        if (value == null)
            Generator.Emit(OpCodes.Ldnull);
        else
            Generator.Emit(OpCodes.Ldstr, value);
        return this;
    }

    /// <summary>
    /// Loads a double constant onto the evaluation stack
    /// </summary>
    public ILBuilder LoadConstant(double value)
    {
        Generator.Emit(OpCodes.Ldc_R8, value);
        return this;
    }

    /// <summary>
    /// Loads a long constant onto the evaluation stack
    /// </summary>
    public ILBuilder LoadConstant(long value)
    {
        Generator.Emit(OpCodes.Ldc_I8, value);
        return this;
    }

    #endregion

    #region Arithmetic Operations

    /// <summary>
    /// Adds two values and pushes the result onto the evaluation stack
    /// </summary>
    public ILBuilder Add()
    {
        Generator.Emit(OpCodes.Add);
        return this;
    }

    /// <summary>
    /// Subtracts one value from another and pushes the result onto the evaluation stack
    /// </summary>
    public ILBuilder Subtract()
    {
        Generator.Emit(OpCodes.Sub);
        return this;
    }

    /// <summary>
    /// Multiplies two values and pushes the result onto the evaluation stack
    /// </summary>
    public ILBuilder Multiply()
    {
        Generator.Emit(OpCodes.Mul);
        return this;
    }

    /// <summary>
    /// Divides two values and pushes the result onto the evaluation stack
    /// </summary>
    public ILBuilder Divide()
    {
        Generator.Emit(OpCodes.Div);
        return this;
    }

    /// <summary>
    /// Negates the value on the evaluation stack
    /// </summary>
    public ILBuilder Negate()
    {
        Generator.Emit(OpCodes.Neg);
        return this;
    }

    /// <summary>
    /// Computes the remainder of dividing two values
    /// </summary>
    public ILBuilder Remainder()
    {
        Generator.Emit(OpCodes.Rem);
        return this;
    }

    #endregion

    #region Comparisons

    /// <summary>
    /// Compares two values: less than
    /// </summary>
    public ILBuilder Clt()
    {
        Generator.Emit(OpCodes.Clt);
        return this;
    }

    /// <summary>
    /// Compares two values: greater than
    /// </summary>
    public ILBuilder Cgt()
    {
        Generator.Emit(OpCodes.Cgt);
        return this;
    }

    /// <summary>
    /// Compares two values for equality
    /// </summary>
    public ILBuilder Equal()
    {
        Generator.Emit(OpCodes.Ceq);
        return this;
    }

    /// <summary>
    /// Compares two values for inequality
    /// </summary>
    public ILBuilder NotEqual()
    {
        Generator.Emit(OpCodes.Ceq);
        Generator.Emit(OpCodes.Ldc_I4_0);
        Generator.Emit(OpCodes.Ceq);
        return this;
    }

    /// <summary>
    /// Compares two values: less than or equal
    /// </summary>
    public ILBuilder Clte()
    {
        Generator.Emit(OpCodes.Cgt);
        Generator.Emit(OpCodes.Ldc_I4_0);
        Generator.Emit(OpCodes.Ceq);
        return this;
    }

    /// <summary>
    /// Compares two values: greater than or equal
    /// </summary>
    public ILBuilder Cgte()
    {
        Generator.Emit(OpCodes.Clt);
        Generator.Emit(OpCodes.Ldc_I4_0);
        Generator.Emit(OpCodes.Ceq);
        return this;
    }

    #endregion

    #region Return & Stack

    /// <summary>
    /// Returns from the current method
    /// </summary>
    public ILBuilder Return()
    {
        Generator.Emit(OpCodes.Ret);
        return this;
    }

    /// <summary>
    /// Pops the top value from the evaluation stack
    /// </summary>
    public ILBuilder Pop()
    {
        Generator.Emit(OpCodes.Pop);
        return this;
    }

    /// <summary>
    /// Duplicates the top value on the evaluation stack
    /// </summary>
    public ILBuilder Duplicate()
    {
        Generator.Emit(OpCodes.Dup);
        return this;
    }

    #endregion

    #region Advanced Operations

    /// <summary>
    /// Emits a specific opcode directly (for advanced scenarios)
    /// </summary>
    public ILBuilder Emit(OpCode opcode)
    {
        Generator.Emit(opcode);
        return this;
    }

    /// <summary>
    /// Emits a specific opcode with a type parameter (for advanced scenarios)
    /// </summary>
    public ILBuilder Emit(OpCode opcode, Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        Generator.Emit(opcode, type);
        return this;
    }

    #endregion
}
