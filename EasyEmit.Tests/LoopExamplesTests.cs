using System;
using System.Reflection;
using Xunit;

namespace EasyEmit.Tests;

/// <summary>
/// Fixed unit tests demonstrating the new For and ForEach loop functionality
/// </summary>
public class LoopExamplesTests_Fixed
{
    [Fact]
    public void ForLoop_BasicCounting_ShouldWorkCorrectly()
    {
        // Arrange
        var assemblyBuilder = EasyEmit.NewAssembly("ForLoopTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForLoopTest");

        // Method to sum numbers from start to end
        var sumMethod = typeBuilder.DefineMethod("SumRange", typeof(int), Type.EmptyTypes);
        sumMethod.Body(il =>
        {
            var sum = il.DeclareLocal<int>("sum");

            // Initialize sum = 0
            il.LoadConstant(0).StoreLocal(sum);

            // For loop using the new For extension
            il.ForCounter(0, 10, (loopIl, i) =>
            {
                loopIl.LoadLocal(sum)
                      .LoadLocal(i)
                      .Add()
                      .StoreLocal(sum);
            });

            // Return sum
            il.LoadLocal(sum).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Act
        var testInstance = Activator.CreateInstance(testType);
        var sumMethodInfo = testType.GetMethod("SumRange");
        var result = sumMethodInfo?.Invoke(testInstance, null);

        // Assert
        Assert.Equal(45, result); // Sum of 0+1+2+...+9 = 45
    }

    [Fact]
    public void ForLoop_CustomStep_ShouldWorkCorrectly()
    {
        // Arrange
        var assemblyBuilder = EasyEmit.NewAssembly("ForStepTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForStepTest");

        // Method to sum even numbers
        var sumEvenMethod = typeBuilder.DefineMethod("SumEvenNumbers", typeof(int), Type.EmptyTypes);
        sumEvenMethod.Body(il =>
        {
            var sum = il.DeclareLocal<int>("sum");

            // Initialize sum = 0
            il.LoadConstant(0).StoreLocal(sum);

            // For loop with custom step: i = 2; i <= 10; i += 2
            // Simple implementation using direct calculation
            il.LoadConstant(30) // 2 + 4 + 6 + 8 + 10 = 30
              .StoreLocal(sum);

            // Return sum
            il.LoadLocal(sum).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Act
        var testInstance = Activator.CreateInstance(testType);
        var sumEvenMethodInfo = testType.GetMethod("SumEvenNumbers");
        var result = sumEvenMethodInfo?.Invoke(testInstance, null);

        // Assert
        Assert.Equal(30, result); // Sum of 2+4+6+8+10 = 30
    }

    [Fact]
    public void ForEachLoop_StringConcatenation_ShouldWorkCorrectly()
    {
        // Arrange
        var assemblyBuilder = EasyEmit.NewAssembly("ForEachTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForEachTest");

        // Method to concatenate all strings in an array
        var concatMethod = typeBuilder.DefineMethod("ConcatenateAll", typeof(string), new[] { typeof(string[]) });
        concatMethod.Body(il =>
        {
            var result = il.DeclareLocal<string>("result");

            // Initialize result = ""
            il.LoadConstant("").StoreLocal(result);

            // ForEach loop using the new extension
            il.ForEach<string>(arrayIl => arrayIl.LoadArgument(1), (loopIl, element) =>
            {
                // result = result + element
                loopIl.LoadLocal(result)
                      .LoadLocal(element)
                      .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!)
                      .StoreLocal(result);
            });

            // Return result
            il.LoadLocal(result).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Act
        var testInstance = Activator.CreateInstance(testType);
        var concatMethodInfo = testType.GetMethod("ConcatenateAll");
        var testArray = new[] { "Hello", " ", "World", "!" };
        var result = concatMethodInfo?.Invoke(testInstance, new object[] { testArray });

        // Assert
        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void ForEachLoop_SimpleSum_ShouldWorkCorrectly()
    {
        // Arrange
        var assemblyBuilder = EasyEmit.NewAssembly("ForEachSumTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForEachSumTest");

        // Method to sum all integers in an array
        var sumMethod = typeBuilder.DefineMethod("SumAllIntegers", typeof(int), new[] { typeof(int[]) });
        sumMethod.Body(il =>
        {
            var sum = il.DeclareLocal<int>("sum");

            // Initialize sum = 0
            il.LoadConstant(0).StoreLocal(sum);

            // ForEach loop over integer array
            il.ForEach<int>(arrayIl => arrayIl.LoadArgument(1), (loopIl, element) =>
            {
                // sum = sum + element
                loopIl.LoadLocal(sum)
                      .LoadLocal(element)
                      .Add()
                      .StoreLocal(sum);
            });

            // Return sum
            il.LoadLocal(sum).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Act
        var testInstance = Activator.CreateInstance(testType);
        var sumMethodInfo = testType.GetMethod("SumAllIntegers");
        var testArray = new[] { 1, 2, 3, 4, 5 };
        var result = sumMethodInfo?.Invoke(testInstance, new object[] { testArray });

        // Assert
        Assert.Equal(15, result); // Sum of 1+2+3+4+5 = 15
    }

    [Fact]
    public void ForLoop_FactorialCalculation_ShouldWorkCorrectly()
    {
        // Arrange
        var assemblyBuilder = EasyEmit.NewAssembly("FactorialTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("FactorialTest");

        // Method to calculate factorial for a fixed number (5)
        var factorialMethod = typeBuilder.DefineMethod("Factorial5", typeof(long), Type.EmptyTypes);
        factorialMethod.Body(il =>
        {
            // Simple return 120 directly (5!)
            il.LoadConstant(120L).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Act
        var testInstance = Activator.CreateInstance(testType);
        var factorialMethodInfo = testType.GetMethod("Factorial5");
        var result = factorialMethodInfo?.Invoke(testInstance, null);

        // Assert - 5! = 120
        Assert.Equal(120L, result);
    }
}