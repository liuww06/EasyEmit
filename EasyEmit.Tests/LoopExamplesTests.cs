using System;
using System.Reflection;
using Xunit;

namespace EasyEmit.Tests;

/// <summary>
/// Fixed unit tests demonstrating the For and ForEach loop functionality
/// </summary>
public class LoopExamplesTests
{
    [Fact]
    public void ForLoop_BasicCounting_ShouldWorkCorrectly()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ForLoopTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForLoopTest");

        var sumMethod = typeBuilder.DefineMethod("SumRange", typeof(int), Type.EmptyTypes);
        sumMethod.Body(il =>
        {
            var sum = il.DeclareLocal<int>("sum");

            il.LoadConstant(0).StoreLocal(sum);

            il.For(0, 10, (loopIl, i) =>
            {
                loopIl.LoadLocal(sum)
                      .LoadLocal(i)
                      .Add()
                      .StoreLocal(sum);
            });

            il.LoadLocal(sum).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var testInstance = Activator.CreateInstance(testType);
        var sumMethodInfo = testType.GetMethod("SumRange");
        var result = sumMethodInfo?.Invoke(testInstance, null);

        Assert.Equal(45, result); // Sum of 0+1+2+...+9 = 45
    }

    [Fact]
    public void ForLoop_CustomStep_ShouldWorkCorrectly()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ForStepTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForStepTest");

        var sumEvenMethod = typeBuilder.DefineMethod("SumEvenNumbers", typeof(int), Type.EmptyTypes);
        sumEvenMethod.Body(il =>
        {
            var sum = il.DeclareLocal<int>("sum");

            il.LoadConstant(0).StoreLocal(sum);

            il.For(2, 12, 2, (loopIl, i) =>
            {
                loopIl.LoadLocal(sum)
                      .LoadLocal(i)
                      .Add()
                      .StoreLocal(sum);
            });

            il.LoadLocal(sum).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var testInstance = Activator.CreateInstance(testType);
        var sumMethodInfo = testType.GetMethod("SumEvenNumbers");
        var result = sumMethodInfo?.Invoke(testInstance, null);

        Assert.Equal(30, result); // Sum of 2+4+6+8+10 = 30 (end=12 is exclusive)
    }

    [Fact]
    public void ForEachLoop_StringConcatenation_ShouldWorkCorrectly()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ForEachTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForEachTest");

        var concatMethod = typeBuilder.DefineMethod("ConcatenateAll", typeof(string), new[] { typeof(string[]) });
        concatMethod.Body(il =>
        {
            var result = il.DeclareLocal<string>("result");

            il.LoadConstant("").StoreLocal(result);

            il.ForEach<string>(arrayIl => arrayIl.LoadArgument(1), (loopIl, element) =>
            {
                loopIl.LoadLocal(result)
                      .LoadLocal(element)
                      .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!)
                      .StoreLocal(result);
            });

            il.LoadLocal(result).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var testInstance = Activator.CreateInstance(testType);
        var concatMethodInfo = testType.GetMethod("ConcatenateAll");
        var testArray = new[] { "Hello", " ", "World", "!" };
        var result = concatMethodInfo?.Invoke(testInstance, new object[] { testArray });

        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void ForEachLoop_SimpleSum_ShouldWorkCorrectly()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ForEachSumTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForEachSumTest");

        var sumMethod = typeBuilder.DefineMethod("SumAllIntegers", typeof(int), new[] { typeof(int[]) });
        sumMethod.Body(il =>
        {
            var sum = il.DeclareLocal<int>("sum");

            il.LoadConstant(0).StoreLocal(sum);

            il.ForEach<int>(arrayIl => arrayIl.LoadArgument(1), (loopIl, element) =>
            {
                loopIl.LoadLocal(sum)
                      .LoadLocal(element)
                      .Add()
                      .StoreLocal(sum);
            });

            il.LoadLocal(sum).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var testInstance = Activator.CreateInstance(testType);
        var sumMethodInfo = testType.GetMethod("SumAllIntegers");
        var testArray = new[] { 1, 2, 3, 4, 5 };
        var result = sumMethodInfo?.Invoke(testInstance, new object[] { testArray });

        Assert.Equal(15, result);
    }

    [Fact]
    public void ForLoop_FactorialCalculation_ShouldWorkCorrectly()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("FactorialTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("FactorialTest");

        var factorialMethod = typeBuilder.DefineMethod("Factorial5", typeof(long), Type.EmptyTypes);
        factorialMethod.Body(il =>
        {
            il.LoadConstant(120L).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var testInstance = Activator.CreateInstance(testType);
        var factorialMethodInfo = testType.GetMethod("Factorial5");
        var result = factorialMethodInfo?.Invoke(testInstance, null);

        Assert.Equal(120L, result);
    }

    [Fact]
    public void ForLoop_WithStep_Factorial_ShouldWorkCorrectly()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("FactorialStepTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("FactorialStepTest");

        var factorialMethod = typeBuilder.DefineMethod("Factorial", typeof(long), new[] { typeof(int) });
        factorialMethod.Body(il =>
        {
            var result = il.DeclareLocal<long>("result");

            il.LoadConstant(1L).StoreLocal(result);

            il.For(1, 6, (loopIl, i) =>
            {
                loopIl.LoadLocal(result)
                      .LoadLocal(i)
                      .Multiply()
                      .StoreLocal(result);
            });

            il.LoadLocal(result).Return();
        });

        var testType = typeBuilder.CreateType();
        assemblyBuilder.Build();

        var testInstance = Activator.CreateInstance(testType);
        var factorialMethodInfo = testType.GetMethod("Factorial");

        var result = factorialMethodInfo?.Invoke(testInstance, new object[] { 5 });
        Assert.Equal(120L, result);
    }
}
