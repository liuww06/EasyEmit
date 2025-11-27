using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using EasyEmit.Loops;
using System.Linq;

namespace EasyEmit;

/// <summary>
/// Main entry point for creating dynamic assemblies using EasyEmit
/// </summary>
public static class EasyEmit
{
    /// <summary>
    /// Creates a new dynamic assembly builder with the specified name
    /// </summary>
    /// <param name="assemblyName">The name of the dynamic assembly</param>
    /// <returns>A fluent assembly builder</returns>
    public static AssemblyBuilder NewAssembly(string assemblyName)
    {
        return new AssemblyBuilder(assemblyName);
    }
}

/// <summary>
/// Fluent builder for creating dynamic assemblies
/// </summary>
public class AssemblyBuilder
{
    private readonly string _name;
    private readonly AssemblyBuilderAccess _access;
    private readonly List<DelayedTypeDefinition> _typeDefinitions;
    private System.Reflection.Emit.AssemblyBuilder? _assemblyBuilder;
    private ModuleBuilder? _moduleBuilder;

    internal AssemblyBuilder(string name, AssemblyBuilderAccess access = AssemblyBuilderAccess.Run)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Assembly name cannot be null or empty.", nameof(name));

        _name = name;
        _access = access;
        _typeDefinitions = new List<DelayedTypeDefinition>();
    }

    /// <summary>
    /// Defines a new type in the assembly
    /// </summary>
    /// <param name="typeName">The name of the type to define</param>
    /// <param name="typeAttributes">The type attributes (default: Public, Class)</param>
    /// <returns>A fluent type builder</returns>
    public TypeBuilder DefineType(string typeName, TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.Class)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));

        return new TypeBuilder(this, typeName, typeAttributes);
    }

    /// <summary>
    /// Builds the assembly and returns the dynamically created assembly
    /// </summary>
    /// <returns>The built assembly</returns>
    public Assembly Build()
    {
        EnsureAssemblyCreated();

        // Create all defined types
        foreach (var typeDef in _typeDefinitions)
        {
            typeDef.Create(_moduleBuilder!);
        }

        return _assemblyBuilder!;
    }

    /// <summary>
    /// Gets the underlying System.Reflection.Emit.AssemblyBuilder (creates if needed)
    /// </summary>
    internal System.Reflection.Emit.AssemblyBuilder GetAssemblyBuilder()
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

    /// <summary>
    /// Ensures the assembly and module builders are created
    /// </summary>
    private void EnsureAssemblyCreated()
    {
        if (_assemblyBuilder == null)
        {
            var assemblyName = new AssemblyName(_name);
            _assemblyBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(assemblyName, _access);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule");
        }
    }

    /// <summary>
    /// Registers a type definition for delayed creation
    /// </summary>
    internal void RegisterType(DelayedTypeDefinition typeDefinition)
    {
        _typeDefinitions.Add(typeDefinition);
    }
}

/// <summary>
/// Represents a delayed type definition that can be created later
/// </summary>
internal class DelayedTypeDefinition
{
    private readonly string _typeName;
    private readonly TypeAttributes _typeAttributes;
    private readonly Type? _parentType;
    private readonly Type[]? _interfaces;
    private readonly List<Action<System.Reflection.Emit.TypeBuilder>> _typeBodyActions;

    public DelayedTypeDefinition(string typeName, TypeAttributes typeAttributes, Type? parentType, Type[]? interfaces)
    {
        _typeName = typeName;
        _typeAttributes = typeAttributes;
        _parentType = parentType;
        _interfaces = interfaces;
        _typeBodyActions = new List<Action<System.Reflection.Emit.TypeBuilder>>();
    }

    /// <summary>
    /// Adds an action to be performed on the TypeBuilder
    /// </summary>
    public void AddBodyAction(Action<System.Reflection.Emit.TypeBuilder> action)
    {
        _typeBodyActions.Add(action);
    }

    /// <summary>
    /// Creates the actual type
    /// </summary>
    public Type Create(ModuleBuilder moduleBuilder)
    {
        var typeBuilder = moduleBuilder.DefineType(_typeName, _typeAttributes, _parentType, _interfaces);

        // Execute all body actions
        foreach (var action in _typeBodyActions)
        {
            action(typeBuilder);
        }

        return typeBuilder.CreateTypeInfo()!.AsType();
    }
}

/// <summary>
/// Fluent builder for creating dynamic types
/// </summary>
public class TypeBuilder
{
    private readonly AssemblyBuilder _assemblyBuilder;
    private readonly string _typeName;
    private readonly TypeAttributes _typeAttributes;
    private Type? _parentType;
    private Type[]? _interfaces;
    private readonly List<DelayedMethodDefinition> _methodDefinitions;
    private readonly List<DelayedFieldDefinition> _fieldDefinitions;

    internal TypeBuilder(AssemblyBuilder assemblyBuilder, string typeName, TypeAttributes typeAttributes)
    {
        _assemblyBuilder = assemblyBuilder;
        _typeName = typeName;
        _typeAttributes = typeAttributes;
        _methodDefinitions = new List<DelayedMethodDefinition>();
        _fieldDefinitions = new List<DelayedFieldDefinition>();
    }

    /// <summary>
    /// Sets the parent type for inheritance
    /// </summary>
    /// <param name="parentType">The parent type to inherit from</param>
    /// <returns>The type builder for fluent chaining</returns>
    public TypeBuilder InheritsFrom(Type parentType)
    {
        _parentType = parentType;
        return this;
    }

    /// <summary>
    /// Adds interfaces to implement
    /// </summary>
    /// <param name="interfaces">The interfaces to implement</param>
    /// <returns>The type builder for fluent chaining</returns>
    public TypeBuilder Implements(params Type[] interfaces)
    {
        _interfaces = interfaces;
        return this;
    }

    /// <summary>
    /// Defines a new method in the type
    /// </summary>
    /// <param name="methodName">The name of the method</param>
    /// <param name="returnType">The return type of the method</param>
    /// <param name="parameterTypes">The parameter types of the method</param>
    /// <param name="methodAttributes">The method attributes (default: Public)</param>
    /// <returns>A fluent method builder</returns>
    public MethodBuilder DefineMethod(string methodName, Type returnType, Type[]? parameterTypes = null, MethodAttributes methodAttributes = MethodAttributes.Public)
    {
        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));
        if (returnType == null)
            throw new ArgumentNullException(nameof(returnType));

        var methodDef = new DelayedMethodDefinition(methodName, returnType, parameterTypes ?? Type.EmptyTypes, methodAttributes);
        _methodDefinitions.Add(methodDef);
        return new MethodBuilder(methodDef);
    }

    /// <summary>
    /// Defines a new field in the type
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="fieldType">The type of the field</param>
    /// <param name="fieldAttributes">The field attributes (default: Public)</param>
    /// <returns>A fluent field builder</returns>
    public FieldBuilder DefineField(string fieldName, Type fieldType, FieldAttributes fieldAttributes = FieldAttributes.Public)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("Field name cannot be null or empty.", nameof(fieldName));
        if (fieldType == null)
            throw new ArgumentNullException(nameof(fieldType));

        var fieldDef = new DelayedFieldDefinition(fieldName, fieldType, fieldAttributes);
        _fieldDefinitions.Add(fieldDef);
        return new FieldBuilder(fieldDef);
    }

    /// <summary>
    /// Creates the type and returns the constructed Type
    /// </summary>
    /// <returns>The created Type</returns>
    public Type CreateType()
    {
        var moduleBuilder = _assemblyBuilder.GetModuleBuilder();
        var typeBuilder = moduleBuilder.DefineType(_typeName, _typeAttributes, _parentType, _interfaces);

        // Create all fields
        foreach (var fieldDef in _fieldDefinitions)
        {
            fieldDef.Create(typeBuilder);
        }

        // Create all methods
        foreach (var methodDef in _methodDefinitions)
        {
            methodDef.Create(typeBuilder);
        }

        return typeBuilder.CreateTypeInfo()!.AsType();
    }
}

/// <summary>
/// Represents a delayed method definition
/// </summary>
internal class DelayedMethodDefinition
{
    public string MethodName { get; }
    public Type ReturnType { get; }
    public Type[] ParameterTypes { get; }
    public MethodAttributes MethodAttributes { get; }
    private Action<System.Reflection.Emit.MethodBuilder>? _methodBodyAction;

    public DelayedMethodDefinition(string methodName, Type returnType, Type[] parameterTypes, MethodAttributes methodAttributes)
    {
        MethodName = methodName;
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
        MethodAttributes = methodAttributes;
    }

    /// <summary>
    /// Sets the method body action
    /// </summary>
    public void SetBodyAction(Action<System.Reflection.Emit.MethodBuilder> action)
    {
        _methodBodyAction = action;
    }

    /// <summary>
    /// Creates the actual method
    /// </summary>
    public void Create(System.Reflection.Emit.TypeBuilder typeBuilder)
    {
        var methodBuilder = typeBuilder.DefineMethod(MethodName, MethodAttributes, ReturnType, ParameterTypes);
        _methodBodyAction?.Invoke(methodBuilder);
    }
}

/// <summary>
/// Represents a delayed field definition
/// </summary>
internal class DelayedFieldDefinition
{
    public string FieldName { get; }
    public Type FieldType { get; }
    public FieldAttributes FieldAttributes { get; }

    public DelayedFieldDefinition(string fieldName, Type fieldType, FieldAttributes fieldAttributes)
    {
        FieldName = fieldName;
        FieldType = fieldType;
        FieldAttributes = fieldAttributes;
    }

    /// <summary>
    /// Creates the actual field
    /// </summary>
    public void Create(System.Reflection.Emit.TypeBuilder typeBuilder)
    {
        typeBuilder.DefineField(FieldName, FieldType, FieldAttributes);
    }
}

/// <summary>
/// Fluent builder for creating dynamic methods
/// </summary>
public class MethodBuilder
{
    private readonly DelayedMethodDefinition _methodDefinition;
    private Action<ILBuilder>? _bodyAction;

    internal MethodBuilder(DelayedMethodDefinition methodDefinition)
    {
        _methodDefinition = methodDefinition;
    }

    /// <summary>
    /// Gets the IL generator for writing method body using a fluent API
    /// </summary>
    /// <param name="bodyAction">Action that defines the method body using ILBuilder</param>
    /// <returns>The method builder for fluent chaining</returns>
    public MethodBuilder Body(Action<ILBuilder> bodyAction)
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

    /// <summary>
    /// Gets the IL generator for writing method body (legacy API)
    /// </summary>
    /// <returns>An IL builder for fluent IL generation</returns>
    public ILBuilder GetIL()
    {
        // This returns a temporary ILBuilder for inline usage
        // Users should preferably use the Body() method
        return new ILBuilder(null);
    }
}

/// <summary>
/// Fluent builder for creating dynamic fields
/// </summary>
public class FieldBuilder
{
    private readonly DelayedFieldDefinition _fieldDefinition;

    internal FieldBuilder(DelayedFieldDefinition fieldDefinition)
    {
        _fieldDefinition = fieldDefinition;
    }

    /// <summary>
    /// Gets the field definition
    /// </summary>
    internal DelayedFieldDefinition GetFieldDefinition()
    {
        return _fieldDefinition;
    }
}

/// <summary>
/// High-level IL generation wrapper
/// </summary>
public class ILBuilder
{
    internal readonly ILGenerator? _generator;
    private readonly Dictionary<string, LocalBuilder> _locals;
    private int _labelCounter;

    internal ILBuilder(ILGenerator? generator)
    {
        _generator = generator;
        _locals = new Dictionary<string, LocalBuilder>();
        _labelCounter = 0;
    }

    #region Local Variable Management

    /// <summary>
    /// Declares a local variable with the specified type
    /// </summary>
    /// <typeparam name="T">The type of the local variable</typeparam>
    /// <param name="name">Optional name for the local variable (for debugging)</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public LocalVariableInfo DeclareLocal<T>(string? name = null)
    {
        return DeclareLocal(typeof(T), name);
    }

    /// <summary>
    /// Declares a local variable with the specified type
    /// </summary>
    /// <param name="localType">The type of the local variable</param>
    /// <param name="name">Optional name for the local variable (for debugging)</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public LocalVariableInfo DeclareLocal(Type localType, string? name = null)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        var localBuilder = _generator.DeclareLocal(localType);
        var localVarInfo = new LocalVariableInfo(localBuilder, name ?? $"local_{localType.Name}_{_locals.Count}");
        _locals[localVarInfo.Name] = localBuilder;
        return localVarInfo;
    }

    #endregion

    #region Argument Loading

    /// <summary>
    /// Loads the argument at the specified index onto the evaluation stack
    /// </summary>
    /// <param name="index">The index of the argument to load</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder LoadArgument(int index)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        // Use proper opcode based on index
        switch (index)
        {
            case 0:
                _generator.Emit(OpCodes.Ldarg_0);
                break;
            case 1:
                _generator.Emit(OpCodes.Ldarg_1);
                break;
            case 2:
                _generator.Emit(OpCodes.Ldarg_2);
                break;
            case 3:
                _generator.Emit(OpCodes.Ldarg_3);
                break;
            default:
                if (index <= 255)
                    _generator.Emit(OpCodes.Ldarg_S, index);
                else
                    _generator.Emit(OpCodes.Ldarg, index);
                break;
        }
        return this;
    }

    /// <summary>
    /// Loads the "this" argument (index 0) onto the evaluation stack
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder LoadThis()
    {
        return LoadArgument(0);
    }

    #endregion

    #region Local Variable Operations

    /// <summary>
    /// Loads the specified local variable onto the evaluation stack
    /// </summary>
    /// <param name="localVar">The local variable to load</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder LoadLocal(LocalVariableInfo localVar)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Ldloc, localVar.Builder);
        return this;
    }

    /// <summary>
    /// Loads the local variable at the specified index onto the evaluation stack
    /// </summary>
    /// <param name="index">The index of the local variable to load</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder LoadLocal(int index)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Ldloc, index);
        return this;
    }

    /// <summary>
    /// Stores the top value from the evaluation stack in the specified local variable
    /// </summary>
    /// <param name="localVar">The local variable to store in</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder StoreLocal(LocalVariableInfo localVar)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Stloc, localVar.Builder);
        return this;
    }

    #endregion

    #region Constant Loading

    /// <summary>
    /// Loads an integer constant onto the evaluation stack
    /// </summary>
    /// <param name="value">The integer value to load</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder LoadConstant(int value)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Ldc_I4, value);
        return this;
    }

    /// <summary>
    /// Loads a string constant onto the evaluation stack
    /// </summary>
    /// <param name="value">The string value to load</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder LoadConstant(string? value)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        if (value == null)
            _generator.Emit(OpCodes.Ldnull);
        else
            _generator.Emit(OpCodes.Ldstr, value);
        return this;
    }

    /// <summary>
    /// Loads a double constant onto the evaluation stack
    /// </summary>
    /// <param name="value">The double value to load</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder LoadConstant(double value)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Ldc_R8, value);
        return this;
    }

    #endregion

    #region Arithmetic Operations

    /// <summary>
    /// Adds two values and pushes the result onto the evaluation stack
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Add()
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Add);
        return this;
    }

    /// <summary>
    /// Subtracts one value from another and pushes the result onto the evaluation stack
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Subtract()
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Sub);
        return this;
    }

    /// <summary>
    /// Multiplies two values and pushes the result onto the evaluation stack
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Multiply()
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Mul);
        return this;
    }

    /// <summary>
    /// Divides two values and pushes the result onto the evaluation stack
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Divide()
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Div);
        return this;
    }

    /// <summary>
    /// Negates the value on the evaluation stack
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Negate()
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Neg);
        return this;
    }

    /// <summary>
    /// Compares two values: less than
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Clt()
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Clt);
        return this;
    }

    /// <summary>
    /// Compares two values: greater than
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Cgt()
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Cgt);
        return this;
    }

    /// <summary>
    /// Compares two values for equality
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Equal()
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Ceq);
        return this;
    }

    #endregion

    #region Method Calls

    /// <summary>
    /// Calls a static method
    /// </summary>
    /// <param name="methodInfo">The method to call</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder CallStatic(MethodInfo methodInfo)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Call, methodInfo);
        return this;
    }

    /// <summary>
    /// Calls an instance method
    /// </summary>
    /// <param name="methodInfo">The method to call</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder CallVirtual(MethodInfo methodInfo)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Callvirt, methodInfo);
        return this;
    }

    /// <summary>
    /// Calls a constructor
    /// </summary>
    /// <param name="constructorInfo">The constructor to call</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder NewObj(ConstructorInfo constructorInfo)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");
        if (constructorInfo == null)
            throw new ArgumentNullException(nameof(constructorInfo));

        _generator.Emit(OpCodes.Newobj, constructorInfo);
        return this;
    }

    /// <summary>
    /// Calls a method (generic call that works with both static and instance methods)
    /// </summary>
    /// <param name="methodInfo">The method to call</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Call(MethodInfo methodInfo)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");
        if (methodInfo == null)
            throw new ArgumentNullException(nameof(methodInfo));

        _generator.Emit(OpCodes.Call, methodInfo);
        return this;
    }

    /// <summary>
    /// Stores a value in an array element
    /// </summary>
    /// <param name="elementType">The type of array elements</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder StoreElement(Type elementType)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");
        if (elementType == null)
            throw new ArgumentNullException(nameof(elementType));

        _generator.Emit(OpCodes.Stelem, elementType);
        return this;
    }

    #endregion

    #region Control Flow

    /// <summary>
    /// Creates an if statement with the specified condition and body
    /// </summary>
    /// <param name="condition">The condition action that sets up the comparison</param>
    /// <param name="trueBody">The action to execute when the condition is true</param>
    /// <returns>A control flow builder for chaining else/if statements</returns>
    public ControlFlowBuilder If(Action<ILBuilder> condition, Action<ILBuilder> trueBody)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        var label = _generator.DefineLabel();

        // Execute condition
        condition(this);

        // Branch if false to after the if block
        _generator.Emit(OpCodes.Brfalse, label);

        // Execute true body
        trueBody(this);

        return new ControlFlowBuilder(this, label);
    }

    /// <summary>
    /// Creates a while loop with the specified condition and body
    /// </summary>
    /// <param name="condition">The condition to check each iteration</param>
    /// <param name="body">The loop body action</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder While(Action<ILBuilder> condition, Action<ILBuilder> body)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        var loopStart = _generator.DefineLabel();
        var loopEnd = _generator.DefineLabel();

        // Mark the start of the loop
        _generator.MarkLabel(loopStart);

        // Check condition
        condition(this);

        // Branch if false to exit loop
        _generator.Emit(OpCodes.Brfalse, loopEnd);

        // Execute loop body
        body(this);

        // Jump back to condition check
        _generator.Emit(OpCodes.Br, loopStart);

        // Mark the end of the loop
        _generator.MarkLabel(loopEnd);

        return this;
    }

    /// <summary>
    /// Creates a for loop with initialization, condition, increment, and body
    /// </summary>
    /// <param name="initializer">The initialization action (typically variable declaration and assignment)</param>
    /// <param name="condition">The condition to check each iteration</param>
    /// <param name="increment">The increment action to execute after each iteration</param>
    /// <param name="body">The loop body action</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder For(Action<ILBuilder> initializer, Action<ILBuilder> condition, Action<ILBuilder> increment, Action<ILBuilder> body)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        var loopStart = _generator.DefineLabel();
        var loopEnd = _generator.DefineLabel();

        // Execute initialization
        initializer(this);

        // Mark the start of the loop
        _generator.MarkLabel(loopStart);

        // Check condition
        condition(this);

        // Branch if false to exit loop
        _generator.Emit(OpCodes.Brfalse, loopEnd);

        // Execute loop body
        body(this);

        // Execute increment
        increment(this);

        // Jump back to condition check
        _generator.Emit(OpCodes.Br, loopStart);

        // Mark the end of the loop
        _generator.MarkLabel(loopEnd);

        return this;
    }

    /// <summary>
    /// Creates a for loop with standard integer counter (for i = start; i < end; i++)
    /// </summary>
    /// <param name="start">The starting value of the counter</param>
    /// <param name="end">The exclusive upper bound of the counter</param>
    /// <param name="body">The loop body action that receives the current counter value</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder For(int start, int end, Action<ILBuilder> body)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        var counter = DeclareLocal<int>("i");
        var loopStart = _generator.DefineLabel();
        var loopEnd = _generator.DefineLabel();

        // Initialize counter: i = start
        LoadConstant(start).StoreLocal(counter);

        // Mark the start of the loop
        _generator.MarkLabel(loopStart);

        // Check condition: i < end
        LoadLocal(counter).LoadConstant(end).Clt();

        // Branch if false to exit loop
        _generator.Emit(OpCodes.Brfalse, loopEnd);

        // Execute loop body
        body(this);

        // Increment: i++
        LoadLocal(counter).LoadConstant(1).Add().StoreLocal(counter);

        // Jump back to condition check
        _generator.Emit(OpCodes.Br, loopStart);

        // Mark the end of the loop
        _generator.MarkLabel(loopEnd);

        return this;
    }

    /// <summary>
    /// Creates a for loop with standard integer counter and passes the counter variable to the body
    /// </summary>
    /// <param name="start">The starting value of the counter</param>
    /// <param name="end">The exclusive upper bound of the counter</param>
    /// <param name="body">The loop body action that receives the current counter value</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder ForCounter(int start, int end, Action<ILBuilder, LocalVariableInfo> body)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        var counter = DeclareLocal<int>("i");
        var loopStart = _generator.DefineLabel();
        var loopEnd = _generator.DefineLabel();

        // Initialize counter: i = start
        LoadConstant(start).StoreLocal(counter);

        // Mark the start of the loop
        _generator.MarkLabel(loopStart);

        // Check condition: i < end
        LoadLocal(counter).LoadConstant(end).Clt();

        // Branch if false to exit loop
        _generator.Emit(OpCodes.Brfalse, loopEnd);

        // Execute loop body
        body(this, counter);

        // Increment: i++
        LoadLocal(counter).LoadConstant(1).Add().StoreLocal(counter);

        // Jump back to condition check
        _generator.Emit(OpCodes.Br, loopStart);

        // Mark the end of the loop
        _generator.MarkLabel(loopEnd);

        return this;
    }

    /// <summary>
    /// Creates a foreach loop for arrays where the array is provided by an action
    /// </summary>
    /// <typeparam name="T">The type of array elements</typeparam>
    /// <param name="getArray">Action that pushes array onto the evaluation stack</param>
    /// <param name="body">The loop body action that receives the current element</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder ForEach<T>(Action<ILBuilder> getArray, Action<ILBuilder> body)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");
        if (getArray == null)
            throw new ArgumentNullException(nameof(getArray));
        if (body == null)
            throw new ArgumentNullException(nameof(body));

        return ForEach<T>(getArray, (il, element) => body(il));
    }

    /// <summary>
    /// Creates a foreach loop for arrays where the array is provided by an action and passes the element to the body
    /// </summary>
    /// <typeparam name="T">The type of array elements</typeparam>
    /// <param name="getArray">Action that pushes array onto the evaluation stack</param>
    /// <param name="body">The loop body action that receives the current element</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder ForEach<T>(Action<ILBuilder> getArray, Action<ILBuilder, LocalVariableInfo> body)
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");
        if (getArray == null)
            throw new ArgumentNullException(nameof(getArray));
        if (body == null)
            throw new ArgumentNullException(nameof(body));

        var array = DeclareLocal<T[]>("array");
        var index = DeclareLocal<int>("index");
        var length = DeclareLocal<int>("length");
        var element = DeclareLocal<T>("element");
        var loopStart = _generator.DefineLabel();
        var loopEnd = _generator.DefineLabel();

        // initializer: get array, set index = 0, length = array.Length
        getArray(this);
        _generator.Emit(OpCodes.Stloc, array.Builder);

        _generator.Emit(OpCodes.Ldc_I4_0);
        _generator.Emit(OpCodes.Stloc, index.Builder);

        _generator.Emit(OpCodes.Ldloc, array.Builder);
        _generator.Emit(OpCodes.Ldlen);
        _generator.Emit(OpCodes.Stloc, length.Builder);

        // Mark the start of the loop
        _generator.MarkLabel(loopStart);

        // Check condition: index < length
        _generator.Emit(OpCodes.Ldloc, index.Builder);
        _generator.Emit(OpCodes.Ldloc, length.Builder);
        _generator.Emit(OpCodes.Clt);
        _generator.Emit(OpCodes.Brfalse, loopEnd);

        // Execute loop body: element = array[index]; execute body
        _generator.Emit(OpCodes.Ldloc, array.Builder);
        _generator.Emit(OpCodes.Ldloc, index.Builder);
        _generator.Emit(OpCodes.Ldelem, typeof(T));
        _generator.Emit(OpCodes.Stloc, element.Builder);

        body(this, element);

        // Increment: index++
        _generator.Emit(OpCodes.Ldloc, index.Builder);
        _generator.Emit(OpCodes.Ldc_I4_1);
        _generator.Emit(OpCodes.Add);
        _generator.Emit(OpCodes.Stloc, index.Builder);

        // Jump back to condition check
        _generator.Emit(OpCodes.Br, loopStart);

        // Mark the end of the loop
        _generator.MarkLabel(loopEnd);

        return this;
    }

    /// <summary>
    /// Creates a simple foreach loop that automatically declares the element variable
    /// </summary>
    /// <typeparam name="T">The type of array elements</typeparam>
    /// <param name="getArray">Action that pushes the array onto the evaluation stack</param>
    /// <param name="body">The loop body action</param>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder ForEachElement<T>(Action<ILBuilder> getArray, Action<ILBuilder> body)
    {
        return ForEach<T>(getArray, body);
    }

    #endregion

    #region Return

    /// <summary>
    /// Returns from the current method, pushing a return value (if any)
    /// </summary>
    /// <returns>The IL builder for fluent chaining</returns>
    public ILBuilder Return()
    {
        if (_generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        _generator.Emit(OpCodes.Ret);
        return this;
    }

    #endregion
}

/// <summary>
/// Represents information about a declared local variable
/// </summary>
public class LocalVariableInfo
{
    internal LocalBuilder Builder { get; }
    public string Name { get; }
    public Type LocalType => Builder.LocalType;

    internal LocalVariableInfo(LocalBuilder builder, string name)
    {
        Builder = builder;
        Name = name;
    }
}

/// <summary>
/// Builder for control flow constructs like if/else
/// </summary>
public class ControlFlowBuilder
{
    private readonly ILBuilder _ilBuilder;
    private readonly Label _endIfLabel;

    internal ControlFlowBuilder(ILBuilder ilBuilder, Label endIfLabel)
    {
        _ilBuilder = ilBuilder;
        _endIfLabel = endIfLabel;
    }

    /// <summary>
    /// Adds an else clause to the if statement
    /// </summary>
    /// <param name="falseBody">The action to execute when the condition is false</param>
    /// <returns>The IL builder for continuing with the method body</returns>
    public ILBuilder Else(Action<ILBuilder> falseBody)
    {
        if (_ilBuilder._generator == null)
            throw new InvalidOperationException("ILGenerator not available");

        falseBody(_ilBuilder);
        _ilBuilder._generator.MarkLabel(_endIfLabel);
        return _ilBuilder;
    }
}