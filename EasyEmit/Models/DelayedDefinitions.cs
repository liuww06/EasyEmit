using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Represents a delayed method definition
/// </summary>
internal class DelayedMethodDefinition
{
    public string MethodName { get; }
    public Type ReturnType { get; }
    public Type[] ParameterTypes { get; }
    public MethodAttributes MethodAttributes { get; }
    private Action<MethodBuilder>? _methodBodyAction;

    public DelayedMethodDefinition(string methodName, Type returnType, Type[] parameterTypes, MethodAttributes methodAttributes)
    {
        MethodName = methodName;
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
        MethodAttributes = methodAttributes;
    }

    public void SetBodyAction(Action<MethodBuilder> action)
    {
        _methodBodyAction = action;
    }

    public void Create(TypeBuilder typeBuilder)
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

    public void Create(TypeBuilder typeBuilder)
    {
        typeBuilder.DefineField(FieldName, FieldType, FieldAttributes);
    }
}

/// <summary>
/// Represents a delayed property definition
/// </summary>
internal class DelayedPropertyDefinition
{
    public string PropertyName { get; }
    public Type PropertyType { get; }
    public PropertyAttributes PropertyAttributes { get; }
    public string? BackingFieldName { get; set; }
    public bool IsAutoProperty { get; set; }
    private Action<PropertyBuilder>? _propertyAction;

    public DelayedPropertyDefinition(string propertyName, Type propertyType, PropertyAttributes propertyAttributes)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
        PropertyAttributes = propertyAttributes;
    }

    public void SetPropertyAction(Action<PropertyBuilder> action)
    {
        _propertyAction = action;
    }

    public void Create(TypeBuilder typeBuilder)
    {
        FieldBuilder? backingFieldBuilder = null;
        if (IsAutoProperty && !string.IsNullOrEmpty(BackingFieldName))
        {
            backingFieldBuilder = typeBuilder.DefineField(BackingFieldName, PropertyType, FieldAttributes.Private);
        }

        var propertyBuilder = typeBuilder.DefineProperty(PropertyName, PropertyAttributes, PropertyType, null);

        if (IsAutoProperty && backingFieldBuilder != null)
        {
            var getterMethodBuilder = typeBuilder.DefineMethod(
                $"get_{PropertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                PropertyType,
                Type.EmptyTypes);

            var getterIL = getterMethodBuilder.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, backingFieldBuilder);
            getterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethodBuilder);

            var setterMethodBuilder = typeBuilder.DefineMethod(
                $"set_{PropertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(void),
                new[] { PropertyType });

            var setterIL = setterMethodBuilder.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, backingFieldBuilder);
            setterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setterMethodBuilder);
        }

        _propertyAction?.Invoke(propertyBuilder);
    }
}
