using System;
using System.Collections.Generic;
using System.Linq;
using EasyEmit;
using Xunit;

namespace EasyEmit.Tests;

/// <summary>
/// Tests for the ForEachEnumerable functionality
/// </summary>
public class ForEachEnumerableTests
{
    [Fact]
    public void ForEachEnumerable_GenericList_ShouldWorkCorrectly()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ForEachEnumerableTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForEachEnumerableTest");

        var sumMethod = typeBuilder.DefineMethod("SumListIntegers", typeof(int), new[] { typeof(List<int>) });
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

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var testInstance = Activator.CreateInstance(testType);
        var sumMethodInfo = testType.GetMethod("SumListIntegers");
        var testList = new List<int> { 1, 2, 3, 4, 5 };
        var result = sumMethodInfo?.Invoke(testInstance, new object[] { testList });

        Assert.Equal(15, result);
    }

    [Fact]
    public void ForEachEnumerable_NonGenericDictionary_ShouldWorkCorrectly()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ForEachDictTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForEachDictTest");

        var countMethod = typeBuilder.DefineMethod("CountItems", typeof(int), new[] { typeof(object) });
        countMethod.Body(il =>
        {
            var count = il.DeclareLocal<int>("count");
            var enumerable = il.DeclareLocal(typeof(System.Collections.IEnumerable), "enumerable");

            il.LoadConstant(0).StoreLocal(count);

            il.LoadArgument(1)
              .Emit(System.Reflection.Emit.OpCodes.Castclass, typeof(System.Collections.IEnumerable))
              .StoreLocal(enumerable);

            il.ForEachEnumerable(
                elementType: typeof(object),
                getEnumerable: getEnum => getEnum.LoadLocal(enumerable),
                body: (loopIl, element) =>
                {
                    loopIl.LoadLocal(count)
                          .LoadConstant(1)
                          .Add()
                          .StoreLocal(count);
                });

            il.LoadLocal(count).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var testInstance = Activator.CreateInstance(testType);
        var countMethodInfo = testType.GetMethod("CountItems");
        var testDict = new Dictionary<string, int>
        {
            ["one"] = 1,
            ["two"] = 2,
            ["three"] = 3
        };
        var result = countMethodInfo?.Invoke(testInstance, new object[] { testDict });

        Assert.Equal(3, result);
    }

    [Fact]
    public void ForEachEnumerable_String_ShouldWorkCorrectly()
    {
        var assemblyBuilder = EmitFactory.NewAssembly("ForEachStringTestAssembly");
        var typeBuilder = assemblyBuilder.DefineType("ForEachStringTest");

        var countCharactersMethod = typeBuilder.DefineMethod("CountCharacters", typeof(int), new[] { typeof(string) });
        countCharactersMethod.Body(il =>
        {
            var count = il.DeclareLocal<int>("count");

            il.LoadConstant(0).StoreLocal(count);

            il.ForEachEnumerable<char>(
                getEnumerable: enumerable => enumerable.LoadArgument(1),
                body: (loopIl, character) =>
                {
                    loopIl.LoadLocal(count)
                          .LoadConstant(1)
                          .Add()
                          .StoreLocal(count);
                });

            il.LoadLocal(count).Return();
        });

        var testType = typeBuilder.CreateType();
        var assembly = assemblyBuilder.Build();

        var testInstance = Activator.CreateInstance(testType);
        var countCharactersMethodInfo = testType.GetMethod("CountCharacters");
        var testString = "EasyEmit";
        var result = countCharactersMethodInfo?.Invoke(testInstance, new object[] { testString });

        Assert.Equal(8, result);
    }
}
