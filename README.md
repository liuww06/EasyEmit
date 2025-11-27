# EasyEmit

A high-level, fluent abstraction wrapper over native .NET `System.Reflection.Emit` that allows developers to create dynamic assemblies, types, and methods easily without dealing with complex `OpCodes` and manual stack management.

## Overview

EasyEmit provides a modern, fluent API for dynamic IL generation that abstracts away the complexity of low-level IL operations while maintaining the performance benefits of direct IL generation.

## Features

- **Fluent API**: Modern method chaining for intuitive type and method creation
- **High-Level Operations**: Think in terms of "Add", "Call", "If/Else" instead of `Ldarg_0`, `Stloc_S`, `Br_S`
- **Safety Features**: Built-in type checking and stack management to minimize `InvalidProgramException`
- **Control Flow Abstractions**: Easy-to-use if/else, while, for, and foreach loops without manual label management
- **Local Variable Management**: Declare and work with variables by type, not by index
- **Method Call Abstraction**: Simplified method calling with automatic overload resolution

## Quick Start

### Basic Calculator Example

```csharp
using EasyEmit;

// Create a simple calculator with an Add method
var assemblyBuilder = EasyEmit.NewAssembly("CalculatorAssembly");
var typeBuilder = assemblyBuilder.DefineType("Calculator");

var addMethod = typeBuilder.DefineMethod("Add", typeof(int), new[] { typeof(int), typeof(int) });
addMethod.Body(il =>
{
    il.LoadArgument(1)  // Load first parameter
      .LoadArgument(2)  // Load second parameter
      .Add()           // Add them
      .Return();       // Return result
});

var calculatorType = typeBuilder.CreateType();
var assembly = assemblyBuilder.Build();

// Use the dynamically created type
var calculator = Activator.CreateInstance(calculatorType);
var addMethodInfo = calculatorType.GetMethod("Add");
var result = addMethodInfo?.Invoke(calculator, new object[] { 5, 3 });

// result = 8
```

### Advanced Example with Loops

```csharp
using EasyEmit;
using EasyEmit.Loops;

// Create a type with a factorial method
var assemblyBuilder = EasyEmit.NewAssembly("MathAssembly");
var typeBuilder = assemblyBuilder.DefineType("MathOperations");

var factorialMethod = typeBuilder.DefineMethod("Factorial", typeof(long), new[] { typeof(int) });
factorialMethod.Body(il =>
{
    var result = il.DeclareLocal<long>("result");
    var n = il.DeclareLocal<int>("n");

    // result = 1
    il.LoadConstant(1L).StoreLocal(result);
    il.LoadArgument(1).StoreLocal(n);

    // For loop: for (i = 2; i <= n; i++)
    il.For(2, (loopIl, i) =>
    {
        // result = result * i
        loopIl.LoadLocal(result)
              .LoadLocal(i)
              .Multiply()
              .StoreLocal(result);
    });

    il.LoadLocal(result).Return();
});

var mathType = typeBuilder.CreateType();
var assembly = assemblyBuilder.Build();

// Test the factorial
var mathInstance = Activator.CreateInstance(mathType);
var factorialMethodInfo = mathType.GetMethod("Factorial");
var result = factorialMethodInfo?.Invoke(mathInstance, new object[] { 5 });

// result = 120
```

## API Reference

### EasyEmit

The entry point for creating dynamic assemblies.

```csharp
public static class EasyEmit
{
    public static AssemblyBuilder NewAssembly(string assemblyName)
    {
        return new AssemblyBuilder(assemblyName);
    }
}
```

### AssemblyBuilder

Builder for creating dynamic assemblies.

```csharp
public class AssemblyBuilder
{
    public TypeBuilder DefineType(string typeName, TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.Class)
    public Assembly Build()
}
```

### TypeBuilder

Builder for creating dynamic types.

```csharp
public class TypeBuilder
{
    public TypeBuilder InheritsFrom(Type parentType)
    public TypeBuilder Implements(params Type[] interfaces)
    public MethodBuilder DefineMethod(string methodName, Type returnType, Type[]? parameterTypes = null, MethodAttributes methodAttributes = MethodAttributes.Public)
    public FieldBuilder DefineField(string fieldName, Type fieldType, FieldAttributes fieldAttributes = FieldAttributes.Public)
    public Type CreateType()
}
```

### MethodBuilder

Builder for creating dynamic methods.

```csharp
public class MethodBuilder
{
    public MethodBuilder Body(Action<ILBuilder> bodyAction)
    public ILBuilder GetIL()
}
```

### ILBuilder

High-level wrapper for IL generation with fluent API.

#### Basic Operations

##### Arithmetic Operations
```csharp
il.LoadArgument(1).LoadArgument(2).Add().Return()
il.LoadArgument(1).LoadArgument(2).Subtract().Return()
il.LoadArgument(1).LoadArgument(2).Multiply().Return()
il.LoadArgument(1).LoadArgument(2).Divide().Return()
il.LoadArgument(1).Negate().Return()
```

##### Variable Management
```csharp
var result = il.DeclareLocal<int>("result");
il.LoadConstant(42).StoreLocal(result);
il.LoadLocal(result).Return();
```

##### Method Calls
```csharp
il.CallStatic(typeof(Math).GetMethod("Pow"))
il.CallVirtual(typeof(string).GetMethod("ToUpper"))
il.NewObj(typeof(MyClass).GetConstructor(typeof(string)))
```

### Control Flow Constructs

#### If/Else

```csharp
il.If(
    condition => condition.LoadLocal(x).LoadConstant(10).Cgt(),
    trueBody => trueBody.LoadConstant("True").Return(),
    falseBody => falseBody.LoadConstant("False").Return()
);
```

#### While Loop

```csharp
il.While(
    condition => condition.LoadLocal(i).LoadConstant(end).Clt(),
    body => loopBody(il.LoadLocal(i).Add().StoreLocal(i))
);
```

#### For Loop

##### Basic For Loop
```csharp
il.For(0, 10, loopIl =>
{
    loopIl.LoadLocal(sum).LoadLocal(i).Add().StoreLocal(sum);
});
```

##### For Loop with Counter Variable
```csharp
il.ForCounter(0, 10, (loopIl, counterVar) =>
{
    loopIl.LoadLocal(result).LoadLocal(counterVar).Add().StoreLocal(result);
});
```

##### ForEach Loop
```csharp
il.ForEach<int>(getArrayAction, (loopIl, element) =>
{
    loopIl.LoadLocal(sum).LoadLocal(element).Add().StoreLocal(sum);
});
```

##### Simple ForEach Loop
```csharp
il.ForEachElement<int>(getArrayAction, loopIl =>
{
    loopIl.LoadLocal(sum).LoadLocal(element).Add().StoreLocal(sum);
});
```

### Extension Methods

#### Loop Extensions

##### For Loop
```csharp
using EasyEmit.Loops;

il.For(0, 10, loopIl => { ... });
```

##### ForEach Loop
```csharp
il.ForEach<int>(getArrayAction, (loopIl, element) => { ... });
```

## Performance Considerations

EasyEmit is designed to be lightweight and efficient:
- The fluent API overhead is minimal and only occurs during type creation
- The generated IL is as efficient as hand-written IL
- Runtime performance of generated methods is identical to manually written IL

## Requirements

- .NET Standard 2.0 or .NET 6.0+
- No external dependencies

## Advanced Usage Examples

### Array Processing

```csharp
var assemblyBuilder = EasyEmit.NewAssembly("ArrayProcessorAssembly");
var typeBuilder = assemblyBuilder.DefineType("ArrayProcessor");

var sumMethod = typeBuilder.DefineMethod("SumArray", typeof(int), new[] { typeof(int[]) });
sumMethod.Body(il =>
{
    var sum = il.DeclareLocal<int>("sum");

    // Initialize sum = 0
    il.LoadConstant(0).StoreLocal(sum);

    // Sum each element
    il.ForEach<int>(arrayIl => arrayIl.LoadArgument(1), (loopIl, element) =>
    {
        loopIl.LoadLocal(sum).LoadLocal(element).Add().StoreLocal(sum);
    });

    il.LoadLocal(sum).Return();
});
```

### String Manipulation

```csharp
var concatMethod = typeBuilder.DefineMethod("JoinStrings", typeof(string), new[] { typeof(string[]) });
concatMethod.Body(il =>
{
    var result = il.DeclareLocal<string>("result");

    il.LoadConstant("").StoreLocal(result);

    il.ForEach<string>(arrayIl => arrayIl.LoadArgument(1), (loopIl, element) =>
    {
        // result = result + element
        loopIl.LoadLocal(result)
              .LoadLocal(element)
              .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }))
              .StoreLocal(result);
    });

    il.LoadLocal(result).Return();
});
```

### Complex Algorithm Implementation

```csharp
var sortMethod = typeBuilder.DefineMethod("QuickSort", typeof(int[]), new[] { typeof(int[]) });
sortMethod.Body(il =>
{
    // Simple QuickSort implementation using loops and comparisons
    var array = il.DeclareLocal<int[]>("array");
    var left = il.DeclareLocal<int>("left");
    var right = il.DeclareLocal<int>("right");
    var pivot = il.DeclareLocal<int>("pivot");

    // Load array from argument
    il.LoadArgument(1).StoreLocal(array);

    // Implement recursive QuickSort
    // This demonstrates the power of combining loops with local variables
    QuickSort(il, 0, array.Length - 1, left, right);
});
```

### LocalVariableInfo

The `LocalVariableInfo` class provides information about declared local variables:

```csharp
public class LocalVariableInfo
{
    public string Name { get; }
    public Type LocalType { get; }
}
```

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) file for guidelines.

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.