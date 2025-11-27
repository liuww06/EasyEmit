using System;
using System.Reflection;
using Xunit;

namespace EasyEmit.Tests;

/// <summary>
/// Unit tests for the EasyEmit library
/// </summary>
public class EasyEmitTests
{
    [Fact]
    public void AssemblyCreation_ShouldCreateValidAssembly()
    {
        // Arrange & Act
        var assemblyBuilder = EasyEmit.NewAssembly("TestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("TestClass");
        var type = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Assert
        Assert.NotNull(assembly);
        Assert.NotNull(type);
        Assert.Equal("TestClass", type.Name);
        Assert.Equal("TestAssembly", assembly.GetName().Name);
    }

    [Fact]
    public void CreateSimpleCalculator_AddMethod_ShouldReturnCorrectSum()
    {
        // Arrange & Act
        var assemblyBuilder = EasyEmit.NewAssembly("TestCalculatorAssembly");
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

        // Act
        var calculator = Activator.CreateInstance(calculatorType);
        var addMethodInfo = calculatorType.GetMethod("Add");
        var result = addMethodInfo?.Invoke(calculator, new object[] { 5, 3 });

        // Assert
        Assert.Equal(8, result);
    }

    [Fact]
    public void CreateSimpleCalculator_SubtractMethod_ShouldReturnCorrectDifference()
    {
        // Arrange & Act
        var assemblyBuilder = EasyEmit.NewAssembly("TestCalculatorAssembly");
        var typeBuilder = assemblyBuilder.DefineType("Calculator");

        var subtractMethod = typeBuilder.DefineMethod("Subtract", typeof(int), new[] { typeof(int), typeof(int) });
        subtractMethod.Body(il =>
        {
            il.LoadArgument(1)  // Load first parameter
              .LoadArgument(2)  // Load second parameter
              .Subtract()      // Subtract second from first
              .Return();       // Return result
        });

        var calculatorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Act
        var calculator = Activator.CreateInstance(calculatorType);
        var subtractMethodInfo = calculatorType.GetMethod("Subtract");
        var result = subtractMethodInfo?.Invoke(calculator, new object[] { 10, 4 });

        // Assert
        Assert.Equal(6, result);
    }

    [Fact]
    public void CreateMathOperations_MultiplyAndDivide_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var assemblyBuilder = EasyEmit.NewAssembly("TestMathAssembly");
        var typeBuilder = assemblyBuilder.DefineType("MathOperations");

        // Create multiply method
        var multiplyMethod = typeBuilder.DefineMethod("Multiply", typeof(double), new[] { typeof(double), typeof(double) });
        multiplyMethod.Body(il =>
        {
            il.LoadArgument(1)
              .LoadArgument(2)
              .Multiply()
              .Return();
        });

        // Create divide method
        var divideMethod = typeBuilder.DefineMethod("Divide", typeof(double), new[] { typeof(double), typeof(double) });
        divideMethod.Body(il =>
        {
            il.LoadArgument(1)
              .LoadArgument(2)
              .Divide()
              .Return();
        });

        var mathType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Act
        var mathInstance = Activator.CreateInstance(mathType);
        var multiplyMethodInfo = mathType.GetMethod("Multiply");
        var divideMethodInfo = mathType.GetMethod("Divide");

        var multiplyResult = multiplyMethodInfo?.Invoke(mathInstance, new object[] { 6.0, 4.0 });
        var divideResult = divideMethodInfo?.Invoke(mathInstance, new object[] { 15.0, 3.0 });

        // Assert
        Assert.Equal(24.0, multiplyResult);
        Assert.Equal(5.0, divideResult);
    }

    [Fact]
    public void TypeBuilder_WithInheritance_ShouldInheritFromBaseType()
    {
        // Arrange & Act
        var assemblyBuilder = EasyEmit.NewAssembly("TestInheritanceAssembly");
        var typeBuilder = assemblyBuilder.DefineType("DerivedClass");
        typeBuilder.InheritsFrom(typeof(object)); // All classes inherit from object by default

        var type = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Assert
        Assert.NotNull(type);
        Assert.NotNull(type.BaseType);
        Assert.Equal(typeof(object), type.BaseType);
    }

    [Fact]
    public void CreateTypeWithField_ShouldDefineFieldCorrectly()
    {
        // Arrange & Act
        var assemblyBuilder = EasyEmit.NewAssembly("TestFieldAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ClassWithField");

        var fieldBuilder = typeBuilder.DefineField("Value", typeof(int), FieldAttributes.Public);
        var type = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Act
        var instance = Activator.CreateInstance(type);
        var field = type.GetField("Value");

        // Assert
        Assert.NotNull(instance);
        Assert.NotNull(field);
        Assert.Equal(typeof(int), field.FieldType);
    }

    [Fact]
    public void CreateTypeWithMultipleMethods_ShouldCreateAllMethods()
    {
        // Arrange & Act
        var assemblyBuilder = EasyEmit.NewAssembly("TestMultiMethodAssembly");
        var typeBuilder = assemblyBuilder.DefineType("MultiMethodClass");

        // Create multiple methods
        var addMethod = typeBuilder.DefineMethod("Add", typeof(int), new[] { typeof(int), typeof(int) });
        addMethod.Body(il => il.LoadArgument(1).LoadArgument(2).Add().Return());

        var subtractMethod = typeBuilder.DefineMethod("Subtract", typeof(int), new[] { typeof(int), typeof(int) });
        subtractMethod.Body(il => il.LoadArgument(1).LoadArgument(2).Subtract().Return());

        var multiplyMethod = typeBuilder.DefineMethod("Multiply", typeof(int), new[] { typeof(int), typeof(int) });
        multiplyMethod.Body(il => il.LoadArgument(1).LoadArgument(2).Multiply().Return());

        var type = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Act
        var instance = Activator.CreateInstance(type);
        var addMethodInfo = type.GetMethod("Add");
        var subtractMethodInfo = type.GetMethod("Subtract");
        var multiplyMethodInfo = type.GetMethod("Multiply");

        var addResult = addMethodInfo?.Invoke(instance, new object[] { 5, 3 });
        var subtractResult = subtractMethodInfo?.Invoke(instance, new object[] { 10, 4 });
        var multiplyResult = multiplyMethodInfo?.Invoke(instance, new object[] { 6, 7 });

        // Assert
        Assert.Equal(8, addResult);
        Assert.Equal(6, subtractResult);
        Assert.Equal(42, multiplyResult);
    }
}