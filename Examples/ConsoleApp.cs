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

        // Example 1: Simple Calculator
        Console.WriteLine("1. Simple Calculator:");
        CreateSimpleCalculator();
        Console.WriteLine();

        // Example 2: Advanced Math Operations
        Console.WriteLine("2. Advanced Math Operations:");
        CreateAdvancedMathOperations();
        Console.WriteLine();

        // Example 3: String Utilities
        Console.WriteLine("3. String Utilities:");
        CreateStringUtilities();
        Console.WriteLine();

        // Example 4: Factorial Calculator (with local variables and loops)
        Console.WriteLine("4. Factorial Calculator:");
        CreateFactorialCalculator();
        Console.WriteLine();

        Console.WriteLine("All examples completed successfully!");
    }

    /// <summary>
    /// Creates a simple calculator with basic arithmetic operations
    /// </summary>
    static void CreateSimpleCalculator()
    {
        var assemblyBuilder = EasyEmit.NewAssembly("SimpleCalculatorAssembly");
        var typeBuilder = assemblyBuilder.DefineType("SimpleCalculator");

        // Add method
        var addMethod = typeBuilder.DefineMethod("Add", typeof(int), new[] { typeof(int), typeof(int) });
        addMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(2).Add().Return();
        });

        // Subtract method
        var subtractMethod = typeBuilder.DefineMethod("Subtract", typeof(int), new[] { typeof(int), typeof(int) });
        subtractMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(2).Subtract().Return();
        });

        // Multiply method
        var multiplyMethod = typeBuilder.DefineMethod("Multiply", typeof(int), new[] { typeof(int), typeof(int) });
        multiplyMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(2).Multiply().Return();
        });

        var calculatorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Test the calculator
        var calculator = Activator.CreateInstance(calculatorType);

        var addResult = calculatorType.GetMethod("Add")?.Invoke(calculator, new object[] { 10, 5 });
        var subtractResult = calculatorType.GetMethod("Subtract")?.Invoke(calculator, new object[] { 10, 3 });
        var multiplyResult = calculatorType.GetMethod("Multiply")?.Invoke(calculator, new object[] { 4, 6 });

        Console.WriteLine($"  10 + 5 = {addResult}");
        Console.WriteLine($"  10 - 3 = {subtractResult}");
        Console.WriteLine($"  4 * 6 = {multiplyResult}");
    }

    /// <summary>
    /// Creates advanced math operations including division and power
    /// </summary>
    static void CreateAdvancedMathOperations()
    {
        var assemblyBuilder = EasyEmit.NewAssembly("AdvancedMathAssembly");
        var typeBuilder = assemblyBuilder.DefineType("AdvancedMath");

        // Division method
        var divideMethod = typeBuilder.DefineMethod("Divide", typeof(double), new[] { typeof(double), typeof(double) });
        divideMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(2).Divide().Return();
        });

        // Square method
        var squareMethod = typeBuilder.DefineMethod("Square", typeof(double), new[] { typeof(double) });
        squareMethod.Body(il =>
        {
            il.LoadArgument(1).LoadArgument(1).Multiply().Return();
        });

        // Absolute value method
        var absMethod = typeBuilder.DefineMethod("Abs", typeof(double), new[] { typeof(double) });
        absMethod.Body(il =>
        {
            il.LoadArgument(0)
              .LoadConstant(0.0)
              .If(
                  condition => condition.LoadArgument(1).LoadConstant(0.0),
                  trueBody => trueBody.LoadArgument(1).Return()
              )
              .Else(falseBody =>
              {
                  falseBody.LoadArgument(1).Negate().Return();
              });
        });

        var mathType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Test the advanced math operations
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

    /// <summary>
    /// Creates string utility methods
    /// </summary>
    static void CreateStringUtilities()
    {
        var assemblyBuilder = EasyEmit.NewAssembly("StringUtilsAssembly");
        var typeBuilder = assemblyBuilder.DefineType("StringUtils");

        // Greeter method
        var greetMethod = typeBuilder.DefineMethod("Greet", typeof(string), new[] { typeof(string) });
        greetMethod.Body(il =>
        {
            il.LoadConstant("Hello, ")
              .LoadArgument(1)
              .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }))
              .Return();
        });

        // String length checker
        var lengthMethod = typeBuilder.DefineMethod("GetLength", typeof(int), new[] { typeof(string) });
        lengthMethod.Body(il =>
        {
            il.LoadArgument(1)
              .CallVirtual(typeof(string).GetProperty("Length")?.GetGetMethod())
              .Return();
        });

        var stringType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Test string utilities
        var stringUtils = Activator.CreateInstance(stringType);

        var greetResult = stringType.GetMethod("Greet")?.Invoke(stringUtils, new object[] { "World" });
        var lengthResult = stringType.GetMethod("GetLength")?.Invoke(stringUtils, new object[] { "EasyEmit" });

        Console.WriteLine($"  Greet(\"World\") = \"{greetResult}\"");
        Console.WriteLine($"  GetLength(\"EasyEmit\") = {lengthResult}");
    }

    /// <summary>
    /// Creates a factorial calculator demonstrating local variables and loops
    /// </summary>
    static void CreateFactorialCalculator()
    {
        var assemblyBuilder = EasyEmit.NewAssembly("FactorialAssembly");
        var typeBuilder = assemblyBuilder.DefineType("FactorialCalculator");

        var factorialMethod = typeBuilder.DefineMethod("Factorial", typeof(int), new[] { typeof(int) });
        factorialMethod.Body(il =>
        {
            var result = il.DeclareLocal<int>("result");
            var i = il.DeclareLocal<int>("i");

            // Initialize result = 1
            il.LoadConstant(1).StoreLocal(result);

            // Initialize i = 1
            il.LoadConstant(1).StoreLocal(i);

            // while (i <= n)
            il.While(
                condition => condition.LoadLocal(i).LoadArgument(1),
                body =>
                {
                    // result = result * i
                    body.LoadLocal(result)
                         .LoadLocal(i)
                         .Multiply()
                         .StoreLocal(result);

                    // i = i + 1
                    body.LoadLocal(i)
                         .LoadConstant(1)
                         .Add()
                         .StoreLocal(i);
                });

            // Return result
            il.LoadLocal(result).Return();
        });

        var factorialType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Test factorial calculator
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