using System;
using System.Reflection;
using EasyEmit;

namespace EasyEmit.Examples;

/// <summary>
/// Advanced examples demonstrating complex EasyEmit patterns
/// </summary>
public class AdvancedExamples
{
    public static void RunAdvancedExamples()
    {
        Console.WriteLine("=== Advanced EasyEmit Examples ===\n");

        // Example 1: Array Sum Calculator
        Console.WriteLine("1. Array Sum Calculator:");
        CreateArraySumCalculator();
        Console.WriteLine();

        // Example 2: Fibonacci Sequence Generator
        Console.WriteLine("2. Fibonacci Sequence Generator:");
        CreateFibonacciGenerator();
        Console.WriteLine();

        // Example 3: Greeting with Conditional Logic
        Console.WriteLine("3. Conditional Greeting:");
        CreateConditionalGreeter();
        Console.WriteLine();

        // Example 4: Simple State Machine
        Console.WriteLine("4. Simple State Machine:");
        CreateStateMachine();
        Console.WriteLine();
    }

    /// <summary>
    /// Creates a calculator that sums all elements in an integer array
    /// </summary>
    static void CreateArraySumCalculator()
    {
        var assemblyBuilder = EasyEmit.NewAssembly("ArrayCalculatorAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ArrayCalculator");

        var sumMethod = typeBuilder.DefineMethod("Sum", typeof(int), new[] { typeof(int[]) });
        sumMethod.Body(il =>
        {
            var sum = il.DeclareLocal<int>("sum");
            var i = il.DeclareLocal<int>("i");
            var length = il.DeclareLocal<int>("length");

            // sum = 0
            il.LoadConstant(0).StoreLocal(sum);

            // length = array.Length
            il.LoadArgument(1)
              .CallVirtual(typeof(Array).GetProperty("Length")?.GetGetMethod())
              .StoreLocal(length);

            // Initialize i = 0
            il.LoadConstant(0).StoreLocal(i);

            // while (i < length)
            il.While(
                condition => condition.LoadLocal(i).LoadLocal(length),
                body =>
                {
                    // sum += array[i]
                    body.LoadLocal(sum)
                         .LoadArgument(1)
                         .LoadLocal(i)
                         .Callvirt(typeof(int[]).GetMethod("Get"))
                         .Add()
                         .StoreLocal(sum);

                    // i++
                    body.LoadLocal(i)
                         .LoadConstant(1)
                         .Add()
                         .StoreLocal(i);
                });

            // return sum
            il.LoadLocal(sum).Return();
        });

        var calculatorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Test the calculator
        var calculator = Activator.CreateInstance(calculatorType);
        var sumMethodInfo = calculatorType.GetMethod("Sum");

        var testArrays = new[]
        {
            new[] { 1, 2, 3, 4, 5 },
            new[] { 10, -5, 7, 0, 3 },
            new[] { 100, 200, 300 },
            Array.Empty<int>()
        };

        foreach (var arr in testArrays)
        {
            var result = sumMethodInfo?.Invoke(calculator, new object[] { arr });
            Console.WriteLine($"  Sum([{string.Join(", ", arr)}]) = {result}");
        }
    }

    /// <summary>
    /// Creates a Fibonacci sequence generator
    /// </summary>
    static void CreateFibonacciGenerator()
    {
        var assemblyBuilder = EasyEmit.NewAssembly("FibonacciAssembly");
        var typeBuilder = assemblyBuilder.DefineType("FibonacciGenerator");

        var fibonacciMethod = typeBuilder.DefineMethod("Generate", typeof(int[]), new[] { typeof(int) });
        fibonacciMethod.Body(il =>
        {
            var n = il.DeclareLocal<int>("n");
            var fibArray = il.DeclareLocal<int[]>("fibArray");
            var i = il.DeclareLocal<int>("i");

            // n = input parameter
            il.LoadArgument(1).StoreLocal(n);

            // Create array of size n
            il.LoadLocal(n)
              .NewObj(typeof(int[]).GetConstructor(new[] { typeof(int) }))
              .StoreLocal(fibArray);

            // if (n >= 1) fibArray[0] = 0
            il.If(
                condition => condition.LoadLocal(n).LoadConstant(1),
                trueBody =>
                {
                    trueBody.LoadLocal(fibArray)
                             .LoadConstant(0)
                             .LoadConstant(0)
                             .Callvirt(typeof(int[]).GetMethod("Set"));
                });

            // if (n >= 2) fibArray[1] = 1
            il.If(
                condition => condition.LoadLocal(n).LoadConstant(2),
                trueBody =>
                {
                    trueBody.LoadLocal(fibArray)
                             .LoadConstant(1)
                             .LoadConstant(1)
                             .Callvirt(typeof(int[]).GetMethod("Set"));
                });

            // Initialize i = 2
            il.LoadConstant(2).StoreLocal(i);

            // while (i < n)
            il.While(
                condition => condition.LoadLocal(i).LoadLocal(n),
                body =>
                {
                    // fibArray[i] = fibArray[i-1] + fibArray[i-2]
                    body.LoadLocal(fibArray)
                         .LoadLocal(i)
                         .LoadLocal(fibArray)
                         .LoadLocal(i)
                         .LoadConstant(1)
                         .Subtract()
                         .Callvirt(typeof(int[]).GetMethod("Get"))
                         .LoadLocal(fibArray)
                         .LoadLocal(i)
                         .LoadConstant(2)
                         .Subtract()
                         .Callvirt(typeof(int[]).GetMethod("Get"))
                         .Add()
                         .Callvirt(typeof(int[]).GetMethod("Set"));

                    // i++
                    body.LoadLocal(i)
                         .LoadConstant(1)
                         .Add()
                         .StoreLocal(i);
                });

            // Return the array
            il.LoadLocal(fibArray).Return();
        });

        var generatorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Test the generator
        var generator = Activator.CreateInstance(generatorType);
        var generateMethodInfo = generatorType.GetMethod("Generate");

        var lengthsToTest = new[] { 0, 1, 2, 5, 10 };
        foreach (var length in lengthsToTest)
        {
            var result = generateMethodInfo?.Invoke(generator, new object[] { length }) as int[];
            Console.WriteLine($"  Fibonacci({length}) = [{string.Join(", ", result ?? Array.Empty<int>())}]");
        }
    }

    /// <summary>
    /// Creates a conditional greeter that changes message based on time of day
    /// </summary>
    static void CreateConditionalGreeter()
    {
        var assemblyBuilder = EasyEmit.NewAssembly("ConditionalGreeterAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ConditionalGreeter");

        var greetMethod = typeBuilder.DefineMethod("Greet", typeof(string), new[] { typeof(string), typeof(int) });
        greetMethod.Body(il =>
        {
            var name = il.DeclareLocal<string>("name");
            var hour = il.DeclareLocal<int>("hour");

            // name = parameter 1
            il.LoadArgument(1).StoreLocal(name);

            // hour = parameter 2
            il.LoadArgument(2).StoreLocal(hour);

            // if (hour < 12) -> Good morning
            il.If(
                condition => condition.LoadLocal(hour).LoadConstant(12),
                trueBody =>
                {
                    trueBody.LoadConstant("Good morning, ")
                             .LoadLocal(name)
                             .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }))
                             .Return();
                })
             // else if (hour < 18) -> Good afternoon
             .Else(
                 elseBody =>
                 {
                     elseBody.If(
                         condition => condition.LoadLocal(hour).LoadConstant(18),
                         trueBody =>
                         {
                             trueBody.LoadConstant("Good afternoon, ")
                                      .LoadLocal(name)
                                      .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }))
                                      .Return();
                         })
                     // else -> Good evening
                     .Else(finalBody =>
                     {
                         finalBody.LoadConstant("Good evening, ")
                                  .LoadLocal(name)
                                  .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }))
                                  .Return();
                     });
                 });
        });

        var greeterType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Test the greeter
        var greeter = Activator.CreateInstance(greeterType);
        var greetMethodInfo = greeterType.GetMethod("Greet");

        var testCases = new[]
        {
            ("Alice", 8),    // Morning
            ("Bob", 14),     // Afternoon
            ("Charlie", 20),  // Evening
            ("Diana", 11),    // Still morning
            ("Eve", 18)       // Evening (exact boundary)
        };

        foreach (var (name, hour) in testCases)
        {
            var result = greetMethodInfo?.Invoke(greeter, new object[] { name, hour });
            Console.WriteLine($"  Hour {hour}: {result}");
        }
    }

    /// <summary>
    /// Creates a simple state machine with states: Idle, Running, Paused, Stopped
    /// </summary>
    static void CreateStateMachine()
    {
        var assemblyBuilder = EasyEmit.NewAssembly("StateMachineAssembly");
        var typeBuilder = assemblyBuilder.DefineType("SimpleStateMachine");

        // Define states as constants
        var stateField = typeBuilder.DefineField("_currentState", typeof(int), FieldAttributes.Private);

        var startMethod = typeBuilder.DefineMethod("Start", typeof(void), Type.EmptyTypes);
        startMethod.Body(il =>
        {
            il.LoadThis()
              .LoadConstant(1) // Running state
              .StoreLocal(stateField.GetFieldDefinition())
              .Return();
        });

        var pauseMethod = typeBuilder.DefineMethod("Pause", typeof(void), Type.EmptyTypes);
        pauseMethod.Body(il =>
        {
            il.LoadThis()
              .LoadConstant(2) // Paused state
              .StoreLocal(stateField.GetFieldDefinition())
              .Return();
        });

        var stopMethod = typeBuilder.DefineMethod("Stop", typeof(void), Type.EmptyTypes);
        stopMethod.Body(il =>
        {
            il.LoadThis()
              .LoadConstant(3) // Stopped state
              .StoreLocal(stateField.GetFieldDefinition())
              .Return();
        });

        var getStateMethod = typeBuilder.DefineMethod("GetState", typeof(string), Type.EmptyTypes);
        getStateMethod.Body(il =>
        {
            var currentState = il.DeclareLocal<int>("currentState");

            // currentState = _currentState
            il.LoadThis()
              .LoadLocal(stateField.GetFieldDefinition())
              .StoreLocal(currentState);

            // Switch on current state
            il.If(
                condition => condition.LoadLocal(currentState).LoadConstant(0),
                trueBody => trueBody.LoadConstant("Idle").Return())
             .Else(
                 elseBody =>
                 {
                     elseBody.If(
                         condition => condition.LoadLocal(currentState).LoadConstant(1),
                         trueBody => trueBody.LoadConstant("Running").Return())
                     .Else(
                         nestedElse =>
                         {
                             nestedElse.If(
                                 condition => condition.LoadLocal(currentState).LoadConstant(2),
                                 trueBody => trueBody.LoadConstant("Paused").Return())
                             .Else(
                                 finalElse => finalElse.LoadConstant("Stopped").Return());
                         });
                 });
        });

        var stateMachineType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Test the state machine
        var stateMachine = Activator.CreateInstance(stateMachineType);
        var startMethodInfo = stateMachineType.GetMethod("Start");
        var pauseMethodInfo = stateMachineType.GetMethod("Pause");
        var stopMethodInfo = stateMachineType.GetMethod("Stop");
        var getStateMethodInfo = stateMachineType.GetMethod("GetState");

        Console.WriteLine($"  Initial state: {getStateMethodInfo?.Invoke(stateMachine, null)}");

        startMethodInfo?.Invoke(stateMachine, null);
        Console.WriteLine($"  After Start(): {getStateMethodInfo?.Invoke(stateMachine, null)}");

        pauseMethodInfo?.Invoke(stateMachine, null);
        Console.WriteLine($"  After Pause(): {getStateMethodInfo?.Invoke(stateMachine, null)}");

        stopMethodInfo?.Invoke(stateMachine, null);
        Console.WriteLine($"  After Stop(): {getStateMethodInfo?.Invoke(stateMachine, null)}");
    }
}