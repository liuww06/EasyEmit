using System;
using System.Reflection;
using EasyEmit;

namespace EasyEmit.Examples;

/// <summary>
/// Demonstration of EasyEmit library capabilities
/// </summary>
public class ConsoleApp
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== EasyEmit Library Demonstration ===\n");

        Console.WriteLine("1. Simple Calculator:");
        CreateSimpleCalculator();
        Console.WriteLine();

        Console.WriteLine("2. Advanced Math Operations:");
        CreateAdvancedMathOperations();
        Console.WriteLine();

        Console.WriteLine("3. String Utilities:");
        CreateStringUtilities();
        Console.WriteLine();

        Console.WriteLine("4. Factorial Calculator (with local variables and loops):");
        CreateFactorialCalculator();
        Console.WriteLine();

        Console.WriteLine("All examples completed successfully!");
    }

    static void CreateSimpleCalculator()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("SimpleCalculatorAssembly");
        var typeBuilder = assemblyBuilder.DefineType("SimpleCalculator");

        var addMethod = typeBuilder.DefineMethod("Add", typeof(int), new[] { typeof(int), typeof(int) });
        addMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(2).Add().Return();
        });

        var subtractMethod = typeBuilder.DefineMethod("Subtract", typeof(int), new[] { typeof(int), typeof(int) });
        subtractMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(2).Subtract().Return();
        });

        var multiplyMethod = typeBuilder.DefineMethod("Multiply", typeof(int), new[] { typeof(int), typeof(int) });
        multiplyMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(2).Multiply().Return();
        });

        var calculatorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var calculator = Activator.CreateInstance(calculatorType);

        var addResult = calculatorType.GetMethod("Add")?.Invoke(calculator, new object[] { 10, 5 });
        var subtractResult = calculatorType.GetMethod("Subtract")?.Invoke(calculator, new object[] { 10, 3 });
        var multiplyResult = calculatorType.GetMethod("Multiply")?.Invoke(calculator, new object[] { 4, 6 });

        Console.WriteLine($"  10 + 5 = {addResult}");
        Console.WriteLine($"  10 - 3 = {subtractResult}");
        Console.WriteLine($"  4 * 6 = {multiplyResult}");
    }

    static void CreateAdvancedMathOperations()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("AdvancedMathAssembly");
        var typeBuilder = assemblyBuilder.DefineType("AdvancedMath");

        var divideMethod = typeBuilder.DefineMethod("Divide", typeof(double), new[] { typeof(double), typeof(double) });
        divideMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(2).Divide().Return();
        });

        var squareMethod = typeBuilder.DefineMethod("Square", typeof(double), new[] { typeof(double) });
        squareMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(1).Multiply().Return();
        });

        // Abs using the safe If API with comparison
        var absMethod = typeBuilder.DefineMethod("Abs", typeof(double), new[] { typeof(double) });
        absMethod.Body(il =>
        {
            // if (arg1 >= 0) return arg1; else return -arg1;
            il.If(
                left: l => l.LoadArgument(1),
                comparison: ILBuilder.Cgte,
                right: r => r.LoadConstant(0.0),
                trueBody: b => b.LoadArgument(1).Return())
             .Else(elseBody =>
             {
                 elseBody.LoadArgument(1).Negate().Return();
             });
        });

        var mathType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var math = Activator.CreateInstance(mathType);

        var divideResult = mathType.GetMethod("Divide")?.Invoke(math, new object[] { 15.0, 3.0 });
        var squareResult = mathType.GetMethod("Square")?.Invoke(math, new object[] { 5.0 });
        var absPositiveResult = mathType.GetMethod("Abs")?.Invoke(math, new object[] { 7.5 });
        var absNegativeResult = mathType.GetMethod("Abs")?.Invoke(math, new object[] { -3.2 });

        Console.WriteLine($"  15.0 / 3.0 = {divideResult}");
        Console.WriteLine($"  Square(5.0) = {squareResult}");
        Console.WriteLine($"  Abs(7.5) = {absPositiveResult}");
        Console.WriteLine($"  Abs(-3.2) = {absNegativeResult}");
    }

    static void CreateStringUtilities()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("StringUtilsAssembly");
        var typeBuilder = assemblyBuilder.DefineType("StringUtils");

        var greetMethod = typeBuilder.DefineMethod("Greet", typeof(string), new[] { typeof(string) });
        greetMethod.Body(il =>
        {
            il.LoadConstant("Hello, ")
              .LoadArgument(1)
              .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }))
              .Return();
        });

        var lengthMethod = typeBuilder.DefineMethod("GetLength", typeof(int), new[] { typeof(string) });
        lengthMethod.Body(il =>
        {
            il.LoadArgument(1)
              .CallVirtual(typeof(string).GetProperty("Length")?.GetGetMethod())
              .Return();
        });

        var stringType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var stringUtils = Activator.CreateInstance(stringType);

        var greetResult = stringType.GetMethod("Greet")?.Invoke(stringUtils, new object[] { "World" });
        var lengthResult = stringType.GetMethod("GetLength")?.Invoke(stringUtils, new object[] { "EasyEmit" });

        Console.WriteLine($"  Greet(\"World\") = \"{greetResult}\"");
        Console.WriteLine($"  GetLength(\"EasyEmit\") = {lengthResult}");
    }

    static void CreateFactorialCalculator()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("FactorialAssembly");
        var typeBuilder = assemblyBuilder.DefineType("FactorialCalculator");

        var factorialMethod = typeBuilder.DefineMethod("Factorial", typeof(int), new[] { typeof(int) });
        factorialMethod.Body(il =>
        {
            var result = il.DeclareLocal<int>("result");
            var i = il.DeclareLocal<int>("i");

            il.LoadConstant(1).StoreLocal(result);
            il.LoadConstant(1).StoreLocal(i);

            il.While(
                condition: c => c.LoadLocal(i).LoadArgument(1).Clt(),
                body =>
                {
                    body.LoadLocal(result)
                         .LoadLocal(i)
                         .Multiply()
                         .StoreLocal(result);

                    body.LoadLocal(i)
                         .LoadConstant(1)
                         .Add()
                         .StoreLocal(i);
                });

            il.LoadLocal(result).Return();
        });

        var factorialType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var calculator = Activator.CreateInstance(factorialType);
        var factorialMethodInfo = factorialType.GetMethod("Factorial");

        var numbersToTest = new[] { 0, 1, 3, 5, 7 };
        foreach (var num in numbersToTest)
        {
            var result = factorialMethodInfo?.Invoke(calculator, new object[] { num });
            Console.WriteLine($"  Factorial({num}) = {result}");
        }
    }
}
