# EasyEmit - Dynamic IL Generation Library

A high-level, fluent abstraction wrapper over native .NET `System.Reflection.Emit` that allows developers to create dynamic assemblies, types, and methods easily without dealing with complex `OpCodes` and manual stack management.

## Architecture

- **File Structure**: Code is organized into `Builders/`, `IL/`, and `Models/` subdirectories
- **Class Naming**: All public builder classes use `Dynamic` prefix to avoid conflicts with `System.Reflection.Emit` types (e.g., `DynamicAssemblyBuilder`, `DynamicTypeBuilder`, `DynamicMethodBuilder`)
- **Local Variables**: `LocalVariable` class wraps `LocalBuilder` for type-safe IL variable access
- **Partial Classes**: `ILBuilder` is split across multiple files (`ILBuilder.cs`, `ILBuilder.ControlFlow.cs`, `ILBuilder.Loops.cs`, `ILBuilder.Methods.cs`, `ILBuilder.Fields.cs`)

## Key Classes

| Class | Purpose |
|-------|---------|
| `EmitFactory` | Entry point - `NewAssembly(name)` |
| `DynamicAssemblyBuilder` | Assembly-level builder |
| `DynamicTypeBuilder` | Type definition with methods, fields, properties |
| `DynamicMethodBuilder` | Method definition with fluent `Body()` |
| `DynamicFieldBuilder` | Field definition |
| `DynamicPropertyBuilder` | Property definition with `AutoProperty()` |
| `ILBuilder` | Fluent IL generation (partial class) |
| `LocalVariable` | Type-safe local variable wrapper |
| `ControlFlowBuilder` | If/Else control flow |

## Testing Results
- **17/17 Tests Pass**: All core, loop, and enumerable tests

## Requirements
- **.NET 6.0+**: Modern .NET platform support
- **No Dependencies**: Pure .NET implementation
