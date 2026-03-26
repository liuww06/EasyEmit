using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit;

/// <summary>
/// Fluent builder for creating dynamic types
/// </summary>
public class DynamicTypeBuilder
{
    private readonly DynamicAssemblyBuilder _assemblyBuilder;
    private readonly string _typeName;
    private readonly TypeAttributes _typeAttributes;
    private Type? _parentType;
    private Type[]? _interfaces;
    private readonly List<DelayedMethodDefinition> _methodDefinitions;
    private readonly List<DelayedFieldDefinition> _fieldDefinitions;
    private readonly List<DelayedPropertyDefinition> _propertyDefinitions;

    internal DynamicTypeBuilder(DynamicAssemblyBuilder assemblyBuilder, string typeName, TypeAttributes typeAttributes)
    {
        _assemblyBuilder = assemblyBuilder;
        _typeName = typeName;
        _typeAttributes = typeAttributes;
        _methodDefinitions = new List<DelayedMethodDefinition>();
        _fieldDefinitions = new List<DelayedFieldDefinition>();
        _propertyDefinitions = new List<DelayedPropertyDefinition>();
    }

    /// <summary>
    /// Sets the parent type for inheritance
    /// </summary>
    public DynamicTypeBuilder InheritsFrom(Type parentType)
    {
        if (parentType == null) throw new ArgumentNullException(nameof(parentType));
        _parentType = parentType;
        return this;
    }

    /// <summary>
    /// Adds interfaces to implement
    /// </summary>
    public DynamicTypeBuilder Implements(params Type[] interfaces)
    {
        _interfaces = interfaces ?? throw new ArgumentNullException(nameof(interfaces));
        return this;
    }

    /// <summary>
    /// Defines a new method in the type
    /// </summary>
    public DynamicMethodBuilder DefineMethod(string methodName, Type returnType, Type[]? parameterTypes = null, MethodAttributes methodAttributes = MethodAttributes.Public)
    {
        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));
        if (returnType == null) throw new ArgumentNullException(nameof(returnType));

        var methodDef = new DelayedMethodDefinition(methodName, returnType, parameterTypes ?? Type.EmptyTypes, methodAttributes);
        _methodDefinitions.Add(methodDef);
        return new DynamicMethodBuilder(methodDef);
    }

    /// <summary>
    /// Defines a new field in the type
    /// </summary>
    public DynamicFieldBuilder DefineField(string fieldName, Type fieldType, FieldAttributes fieldAttributes = FieldAttributes.Public)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("Field name cannot be null or empty.", nameof(fieldName));
        if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

        var fieldDef = new DelayedFieldDefinition(fieldName, fieldType, fieldAttributes);
        _fieldDefinitions.Add(fieldDef);
        return new DynamicFieldBuilder(fieldDef);
    }

    /// <summary>
    /// Defines a new property in the type
    /// </summary>
    public DynamicPropertyBuilder DefineProperty(string propertyName, Type propertyType, PropertyAttributes propertyAttributes = PropertyAttributes.None)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));
        if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));

        var propertyDef = new DelayedPropertyDefinition(propertyName, propertyType, propertyAttributes);
        _propertyDefinitions.Add(propertyDef);
        return new DynamicPropertyBuilder(propertyDef);
    }

    /// <summary>
    /// Creates the type and returns the constructed Type
    /// </summary>
    public Type CreateType()
    {
        var moduleBuilder = _assemblyBuilder.GetModuleBuilder();
        var typeBuilder = moduleBuilder.DefineType(_typeName, _typeAttributes, _parentType, _interfaces);

        foreach (var fieldDef in _fieldDefinitions)
            fieldDef.Create(typeBuilder);

        foreach (var propertyDef in _propertyDefinitions)
            propertyDef.Create(typeBuilder);

        foreach (var methodDef in _methodDefinitions)
            methodDef.Create(typeBuilder);

        return typeBuilder.CreateTypeInfo()!.AsType();
    }
}

/// <summary>
/// Fluent builder for creating dynamic fields
/// </summary>
public class DynamicFieldBuilder
{
    private readonly DelayedFieldDefinition _fieldDefinition;

    internal DynamicFieldBuilder(DelayedFieldDefinition fieldDefinition)
    {
        _fieldDefinition = fieldDefinition;
    }

    internal DelayedFieldDefinition GetFieldDefinition() => _fieldDefinition;
}

/// <summary>
/// Fluent builder for creating dynamic properties
/// </summary>
public class DynamicPropertyBuilder
{
    private readonly DelayedPropertyDefinition _propertyDefinition;

    internal DynamicPropertyBuilder(DelayedPropertyDefinition propertyDefinition)
    {
        _propertyDefinition = propertyDefinition;
    }

    /// <summary>
    /// Sets a simple property with custom getter and setter
    /// </summary>
    public DynamicPropertyBuilder Custom(Action<PropertyBuilder> propertyAction)
    {
        _propertyDefinition.SetPropertyAction(propertyBuilder => propertyAction(propertyBuilder));
        return this;
    }

    /// <summary>
    /// Creates a simple auto-property with getter and setter
    /// </summary>
    public DynamicPropertyBuilder AutoProperty(string? backingFieldName = null)
    {
        var fieldName = backingFieldName ?? $"{_propertyDefinition.PropertyName}_field";
        _propertyDefinition.BackingFieldName = fieldName;
        _propertyDefinition.IsAutoProperty = true;
        return this;
    }
}
