using System;
using System.Collections.Generic;
using System.Linq;
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

        Console.WriteLine("1. Array Sum Calculator:");
        CreateArraySumCalculator();
        Console.WriteLine();

        Console.WriteLine("2. Fibonacci Sequence Generator:");
        CreateFibonacciGenerator();
        Console.WriteLine();

        Console.WriteLine("3. Greeting with Conditional Logic:");
        CreateConditionalGreeter();
        Console.WriteLine();

        Console.WriteLine("4. Simple State Machine:");
        CreateStateMachine();
        Console.WriteLine();

        Console.WriteLine("5. ForEach with Enumerable Collections:");
        CreateEnumerableProcessor();
        Console.WriteLine();
    }

    static void CreateArraySumCalculator()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ArrayCalculatorAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ArrayCalculator");

        var sumMethod = typeBuilder.DefineMethod("Sum", typeof(int), new[] { typeof(int[]) });
        sumMethod.Body(il =>
        {
            var sum = il.DeclareLocal<int>("sum");
            var length = il.DeclareLocal<int>("length");

            il.LoadConstant(0).StoreLocal(sum);

            il.LoadArgument(1)
              .CallVirtual(typeof(Array).GetProperty("Length")?.GetGetMethod())
              .StoreLocal(length);

            il.For(0, il => il.LoadLocal(length), (body, idx) =>
            {
                body.LoadLocal(sum)
                     .LoadArgument(1)
                     .LoadLocal(idx)
                     .Callvirt(typeof(int[]).GetMethod("Get"))
                     .Add()
                     .StoreLocal(sum);
            });

            il.LoadLocal(sum).Return();
        });

        var calculatorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

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

    static void CreateFibonacciGenerator()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("FibonacciAssembly");
        var typeBuilder = assemblyBuilder.DefineType("FibonacciGenerator");

        var fibonacciMethod = typeBuilder.DefineMethod("Generate", typeof(int[]), new[] { typeof(int) });
        fibonacciMethod.Body(il =>
        {
            var n = il.DeclareLocal<int>("n");
            var fibArray = il.DeclareLocal<int[]>("fibArray");
            var i = il.DeclareLocal<int>("i");

            il.LoadArgument(1).StoreLocal(n);

            il.LoadLocal(n)
              .NewObj(typeof(int[]).GetConstructor(new[] { typeof(int) }))
              .StoreLocal(fibArray);

            // if (n >= 1) fibArray[0] = 0
            il.If(
                left: l => l.LoadLocal(n),
                comparison: ILBuilder.Cgte,
                right: r => r.LoadConstant(1),
                trueBody =>
                {
                    trueBody.LoadLocal(fibArray)
                             .LoadConstant(0)
                             .LoadConstant(0)
                             .Callvirt(typeof(int[]).GetMethod("Set"));
                });

            // if (n >= 2) fibArray[1] = 1
            il.If(
                left: l => l.LoadLocal(n),
                comparison: ILBuilder.Cgte,
                right: r => r.LoadConstant(2),
                trueBody =>
                {
                    trueBody.LoadLocal(fibArray)
                             .LoadConstant(1)
                             .LoadConstant(1)
                             .Callvirt(typeof(int[]).GetMethod("Set"));
                });

            il.LoadConstant(2).StoreLocal(i);

            il.While(
                condition: c => c.LoadLocal(i).LoadLocal(n).Clt(),
                body =>
                {
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

                    body.LoadLocal(i)
                         .LoadConstant(1)
                         .Add()
                         .StoreLocal(i);
                });

            il.LoadLocal(fibArray).Return();
        });

        var generatorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var generator = Activator.CreateInstance(generatorType);
        var generateMethodInfo = generatorType.GetMethod("Generate");

        var lengthsToTest = new[] { 0, 1, 2, 5, 10 };
        foreach (var length in lengthsToTest)
        {
            var result = generateMethodInfo?.Invoke(generator, new object[] { length }) as int[];
            Console.WriteLine($"  Fibonacci({length}) = [{string.Join(", ", result ?? Array.Empty<int>())}]");
        }
    }

    static void CreateConditionalGreeter()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ConditionalGreeterAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ConditionalGreeter");

        var greetMethod = typeBuilder.DefineMethod("Greet", typeof(string), new[] { typeof(string), typeof(int) });
        greetMethod.Body(il =>
        {
            var name = il.DeclareLocal<string>("name");
            var hour = il.DeclareLocal<int>("hour");

            il.LoadArgument(1).StoreLocal(name);
            il.LoadArgument(2).StoreLocal(hour);

            // Using the safe If API with comparison
            il.If(
                left: l => l.LoadLocal(hour),
                comparison: ILBuilder.Clt,
                right: r => r.LoadConstant(12),
                trueBody =>
                {
                    trueBody.LoadConstant("Good morning, ")
                             .LoadLocal(name)
                             .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }))
                             .Return();
                })
             .Else(
                 elseBody =>
                 {
                     elseBody.If(
                         left: l => l.LoadLocal(hour),
                         comparison: ILBuilder.Clt,
                         right: r => r.LoadConstant(18),
                         trueBody =>
                         {
                             trueBody.LoadConstant("Good afternoon, ")
                                      .LoadLocal(name)
                                      .CallStatic(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }))
                                      .Return();
                         })
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

        var greeter = Activator.CreateInstance(greeterType);
        var greetMethodInfo = greeterType.GetMethod("Greet");

        var testCases = new[]
        {
            ("Alice", 8),
            ("Bob", 14),
            ("Charlie", 20),
            ("Diana", 11),
            ("Eve", 18)
        };

        foreach (var (name, hour) in testCases)
        {
            var result = greetMethodInfo?.Invoke(greeter, new object[] { name, hour });
            Console.WriteLine($"  Hour {hour}: {result}");
        }
    }

    static void CreateStateMachine()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("StateMachineAssembly");
        var typeBuilder = assemblyBuilder.DefineType("SimpleStateMachine");

        var stateField = typeBuilder.DefineField("_currentState", typeof(int), FieldAttributes.Private);
        var stateFieldInfo = typeof(SimpleStateMachine).GetField("_currentState");

        var startMethod = typeBuilder.DefineMethod("Start", typeof(void), Type.EmptyTypes);
        startMethod.Body(il =>
        {
            // For now we use a workaround since the field info is only available after CreateType
            // In practice users would get the FieldInfo from the created type
            il.LoadThis()
              .LoadConstant(1)
              .Pop()
              .Pop()
              .Return();
        });

        var pauseMethod = typeBuilder.DefineMethod("Pause", typeof(void), Type.EmptyTypes);
        pauseMethod.Body(il =>
        {
            il.LoadThis()
              .LoadConstant(2)
              .Pop()
              .Pop()
              .Return();
        });

        var stopMethod = typeBuilder.DefineMethod("Stop", typeof(void), Type.EmptyTypes);
        stopMethod.Body(il =>
        {
            il.LoadThis()
              .LoadConstant(3)
              .Pop()
              .Pop()
              .Return();
        });

        var getStateMethod = typeBuilder.DefineMethod("GetState", typeof(string), Type.EmptyTypes);
        getStateMethod.Body(il =>
        {
            // Simple return based on stored state using the safe If API
            il.LoadThis()
              .Emit(System.Reflection.Emit.OpCodes.Ldfld, 0)
              .Return();
        });

        var stateMachineType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        // Get the field info from the created type
        var currentStateField = stateMachineType.GetField("_currentState")!;

        // Rebuild with correct field access
        var assemblyBuilder2 = EmitFactory.NewAssembly("StateMachineAssembly2");
        var typeBuilder2 = assemblyBuilder2.DefineType("SimpleStateMachine2");

        typeBuilder2.DefineField("_currentState", typeof(int), FieldAttributes.Private);

        var startMethod2 = typeBuilder2.DefineMethod("Start", typeof(void), Type.EmptyTypes);
        startMethod2.Body(il =>
        {
            il.LoadThis()
              .LoadConstant(1)
              .StoreField(currentStateField)
              .Return();
        });

        var pauseMethod2 = typeBuilder2.DefineMethod("Pause", typeof(void), Type.EmptyTypes);
        pauseMethod2.Body(il =>
        {
            il.LoadThis()
              .LoadConstant(2)
              .StoreField(currentStateField)
              .Return();
        });

        var stopMethod2 = typeBuilder2.DefineMethod("Stop", typeof(void), Type.EmptyTypes);
        stopMethod2.Body(il =>
        {
            il.LoadThis()
              .LoadConstant(3)
              .StoreField(currentStateField)
              .Return();
        });

        var getStateMethod2 = typeBuilder2.DefineMethod("GetState", typeof(string), Type.EmptyTypes);
        getStateMethod2.Body(il =>
        {
            var currentState = il.DeclareLocal<int>("currentState");

            il.LoadThis()
              .LoadField(currentStateField)
              .StoreLocal(currentState);

            il.If(
                left: l => l.LoadLocal(currentState),
                comparison: ILBuilder.Equal,
                right: r => r.LoadConstant(0),
                trueBody => trueBody.LoadConstant("Idle").Return())
             .Else(
                 elseBody =>
                 {
                     elseBody.If(
                         left: l => l.LoadLocal(currentState),
                         comparison: ILBuilder.Equal,
                         right: r => r.LoadConstant(1),
                         trueBody => trueBody.LoadConstant("Running").Return())
                     .Else(
                         nestedElse =>
                         {
                             nestedElse.If(
                                 left: l => l.LoadLocal(currentState),
                                 comparison: ILBuilder.Equal,
                                 right: r => r.LoadConstant(2),
                                 trueBody => trueBody.LoadConstant("Paused").Return())
                             .Else(
                                 finalElse => finalElse.LoadConstant("Stopped").Return());
                         });
                 });
        });

        var stateMachineType2 = typeBuilder2.CreateType();
        assemblyBuilder2.Build();

        var stateMachine = Activator.CreateInstance(stateMachineType2);
        var startMethodInfo = stateMachineType2.GetMethod("Start");
        var pauseMethodInfo = stateMachineType2.GetMethod("Pause");
        var stopMethodInfo = stateMachineType2.GetMethod("Stop");
        var getStateMethodInfo = stateMachineType2.GetMethod("GetState");

        Console.WriteLine($"  Initial state: {getStateMethodInfo?.Invoke(stateMachine, null)}");

        startMethodInfo?.Invoke(stateMachine, null);
        Console.WriteLine($"  After Start(): {getStateMethodInfo?.Invoke(stateMachine, null)}");

        pauseMethodInfo?.Invoke(stateMachine, null);
        Console.WriteLine($"  After Pause(): {getStateMethodInfo?.Invoke(stateMachine, null)}");

        stopMethodInfo?.Invoke(stateMachine, null);
        Console.WriteLine($"  After Stop(): {getStateMethodInfo?.Invoke(stateMachine, null)}");
    }

    static void CreateEnumerableProcessor()
    {
        Console.WriteLine("  Generic List Processor:");
        CreateListSumProcessor();

        Console.WriteLine();

        Console.WriteLine("  Dictionary Processor:");
        CreateDictionaryProcessor();

        Console.WriteLine();

        Console.WriteLine("  String Processor:");
        CreateStringProcessor();
    }

    static void CreateListSumProcessor()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ListProcessorAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ListProcessor");

        var sumMethod = typeBuilder.DefineMethod("Sum", typeof(int), new[] { typeof(List<int>) });
        sumMethod.Body(il =>
        {
            var sum = il.DeclareLocal<int>("sum");

            il.LoadConstant(0).StoreLocal(sum);

            il.ForEachEnumerable<int>(
                getEnumerable: enumerable => enumerable.LoadArgument(1),
                body: (loopIl, element) =>
                {
                    loopIl.LoadLocal(sum)
                          .LoadLocal(element)
                          .Add()
                          .StoreLocal(sum);
                });

            il.LoadLocal(sum).Return();
        });

        var processorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var processor = Activator.CreateInstance(processorType);
        var sumMethodInfo = processorType.GetMethod("Sum");

        var testLists = new[]
        {
            new List<int> { 1, 2, 3, 4, 5 },
            new List<int> { 10, -5, 7, 0, 3 },
            new List<int> { 100, 200, 300 },
            new List<int>()
        };

        foreach (var list in testLists)
        {
            var result = sumMethodInfo?.Invoke(processor, new object[] { list });
            Console.WriteLine($"    Sum([{string.Join(", ", list)}]) = {result}");
        }
    }

    static void CreateDictionaryProcessor()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("DictionaryProcessorAssembly");
        var typeBuilder = assemblyBuilder.DefineType("DictionaryProcessor");

        var getKeysMethod = typeBuilder.DefineMethod("GetKeys", typeof(string[]), new[] { typeof(object) });
        getKeysMethod.Body(il =>
        {
            var dictionary = il.DeclareLocal(typeof(IEnumerable), "dictionary");
            var keysList = il.DeclareLocal(typeof(List<string>), "keysList");

            il.LoadArgument(1)
              .Emit(System.Reflection.Emit.OpCodes.Castclass, typeof(IEnumerable))
              .StoreLocal(dictionary);

            il.NewObj(typeof(List<string>).GetConstructor(Type.EmptyTypes))
              .StoreLocal(keysList);

            il.ForEachEnumerable(
                elementType: typeof(object),
                getEnumerable: enumerable => enumerable.LoadLocal(dictionary),
                body: (loopIl, element) =>
                {
                    loopIl.LoadLocal(element)
                          .Emit(System.Reflection.Emit.OpCodes.Castclass, typeof(KeyValuePair<string, int>))
                          .CallVirtual(typeof(KeyValuePair<string, int>).GetProperty("Key")!.GetGetMethod()!)
                          .LoadLocal(keysList)
                          .CallVirtual(typeof(List<string>).GetMethod("Add", new[] { typeof(string) })!)
                          .Pop();
                });

            il.LoadLocal(keysList)
              .CallVirtual(typeof(List<string>).GetMethod("ToArray")!)
              .Return();
        });

        var processorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var processor = Activator.CreateInstance(processorType);
        var getKeysMethodInfo = processorType.GetMethod("GetKeys");

        var testDict = new Dictionary<string, int>
        {
            ["apple"] = 5,
            ["banana"] = 3,
            ["cherry"] = 8,
            ["date"] = 1
        };

        var result = getKeysMethodInfo?.Invoke(processor, new object[] { testDict }) as string[];
        Console.WriteLine($"    Dictionary keys: [{string.Join(", ", result ?? Array.Empty<string>())}]");
    }

    static void CreateStringProcessor()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("StringProcessorAssembly");
        var typeBuilder = assemblyBuilder.DefineType("StringProcessor");

        var countVowelsMethod = typeBuilder.DefineMethod("CountVowels", typeof(int), new[] { typeof(string) });
        countVowelsMethod.Body(il =>
        {
            var vowelCount = il.DeclareLocal<int>("vowelCount");
            var vowels = il.DeclareLocal(typeof(HashSet<char>), "vowels");

            il.LoadConstant(0).StoreLocal(vowelCount);

            il.NewObj(typeof(HashSet<char>).GetConstructor(Type.EmptyTypes))
              .LoadConstant('a')
              .CallVirtual(typeof(HashSet<char>).GetMethod("Add", new[] { typeof(char) })!)
              .Pop()
              .LoadLocal(vowels)
              .LoadConstant('e')
              .CallVirtual(typeof(HashSet<char>).GetMethod("Add", new[] { typeof(char) })!)
              .Pop()
              .LoadLocal(vowels)
              .LoadConstant('i')
              .CallVirtual(typeof(HashSet<char>).GetMethod("Add", new[] { typeof(char) })!)
              .Pop()
              .LoadLocal(vowels)
              .LoadConstant('o')
              .CallVirtual(typeof(HashSet<char>).GetMethod("Add", new[] { typeof(char) })!)
              .Pop()
              .LoadLocal(vowels)
              .LoadConstant('u')
              .CallVirtual(typeof(HashSet<char>).GetMethod("Add", new[] { typeof(char> })!)
              .Pop()
              .StoreLocal(vowels);

            il.ForEachEnumerable<char>(
                getEnumerable: enumerable => enumerable.LoadArgument(1),
                body: (loopIl, character) =>
                {
                    loopIl.LoadLocal(character)
                          .CallStatic(typeof(char).GetMethod("ToLowerInvariant")!)
                          .LoadLocal(vowels)
                          .CallVirtual(typeof(HashSet<char>).GetMethod("Contains", new[] { typeof(char) })!)
                          .If(
                              condition: c => { }, // condition already on stack
                              trueBody =>
                              {
                                  trueBody.LoadLocal(vowelCount)
                                           .LoadConstant(1)
                                           .Add()
                                           .StoreLocal(vowelCount);
                              });
                });

            il.LoadLocal(vowelCount).Return();
        });

        var processorType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var processor = Activator.CreateInstance(processorType);
        var countVowelsMethodInfo = processorType.GetMethod("CountVowels");

        var testStrings = new[]
        {
            "Hello World",
            "EasyEmit",
            "C# Programming",
            "abcdefghijklmnopqrstuvwxyz",
            "AEIOU aeiou",
            "",
            "Rhythm"
        };

        foreach (var str in testStrings)
        {
            var result = countVowelsMethodInfo?.Invoke(processor, new object[] { str });
            Console.WriteLine($"    \"{str}\": {result} vowels");
        }
    }
}
