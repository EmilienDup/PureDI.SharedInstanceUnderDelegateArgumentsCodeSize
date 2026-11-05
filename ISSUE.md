# A singleton reached inside delegates with arguments gets a copy of its creation code for every delegate (code doubles per level)

**Pure.DI master (d3440f5)** and **2.5.7**, .NET SDK 10.0.403, macOS. Repro project attached (`dotnet build`; `-p:LocalFunctionsOn=true`, `-p:LetFunc=true`; `python3 generate.py <levels> [lifetime]` regenerates `Program.cs`).

Follow-up to #184. A singleton whose own delegates create objects that consume the delegate arguments gets a separate `Ensure…Exists()` function inside each enclosing delegate lambda, even with `.Hint(Hint.LocalFunctions, "On")`.

## Minimal code (3 of the 10 levels)

```csharp
using Pure.DI;
using static Pure.DI.Lifetime;

var composition = new Composition();
Console.WriteLine(composition.Root);

public sealed class A;
public sealed class B;

public sealed class Service0(Func<A, ItemA0> openA, Func<B, ItemB0> openB);
public sealed class ItemA0(A a, Service1 next);
public sealed class ItemB0(B b, Service1 next);

public sealed class Service1(Func<A, ItemA1> openA, Func<B, ItemB1> openB);
public sealed class ItemA1(A a, Service2 next);
public sealed class ItemB1(B b, Service2 next);

public sealed class Service2(Func<A, ItemA2> openA, Func<B, ItemB2> openB);
public sealed class ItemA2(A a);
public sealed class ItemB2(B b);

partial class Composition
{
    private static void Setup() => DI.Setup(nameof(Composition))
        .Hint(Hint.LocalFunctions, "On")
        .Bind().As(Singleton).To<Service0>()
        .Bind().As(Singleton).To<Service1>()
        .Bind().As(Singleton).To<Service2>()
        .Root<Service0>("Root");
}
```

## Expected

Each singleton has one `Ensure…Exists()` function. A singleton does not depend on the argument of the delegate it is reached from: every `A` or `B` its items consume is the argument of the singleton's own delegates.

## Actual

`Service2` gets one function inside the `Func<A, ItemA1>` lambda and another inside the `Func<B, ItemB1>` lambda, with the same body:

```csharp
System.Func<global::A, global::ItemA1> localFactory1= new System.Func<global::A, global::ItemA1>((global::A localArg1) =>
{
    global::A overriddenA = localArg1;
    EnsureService2Exists309d();
    void EnsureService2Exists309d()
    {
        if (_singletonService2309d is null)
            lock (_lock309d)
                if (_singletonService2309d is null)
                {
                    // Func<A, ItemA2> and Func<B, ItemB2> lambdas
                    _singletonService2309d = new global::Service2(perBlockFuncAItemA2, perBlockFuncBItemB2);
                }
    }
    return new global::ItemA1(overriddenA, _singletonService2309d);
});
System.Func<global::B, global::ItemB1> localFactory4= new System.Func<global::B, global::ItemB1>((global::B localArg1) =>
{
    global::B overriddenB = localArg1;
    EnsureService2Exists309d1();
    void EnsureService2Exists309d1()
    {
        // the same code again
        _singletonService2309d = new global::Service2(perBlockFuncAItemA21, perBlockFuncBItemB21);
    }
    return new global::ItemB1(overriddenB, _singletonService2309d);
});
```

`Service1` is copied the same way inside the two lambdas of `Service0`, so the code doubles with each level:

| levels | lifetime | version | `Func<T1, T>`, `LocalFunctions` | lines | `new Service<last>(` | `EnsureService<last>Exists` functions | build | DIW016 |
|---|---|---|---|---|---|---|---|---|
| 6 | Singleton | master | built-in, _Auto_ | 2,217 | 32 | 32 | 1 s | |
| 6 | Singleton | master | built-in, _On_ | 2,217 | 32 | 32 | 0 s | |
| 8 | Singleton | master | built-in, _Auto_ | 8,171 | 128 | 128 | 2 s | yes |
| 8 | Singleton | master | built-in, _On_ | 8,171 | 128 | 128 | 1 s | yes |
| 10 | Singleton | master | built-in, _Auto_ | 31,978 | 512 | 512 | 2 s | yes |
| 10 | Singleton | master | built-in, _On_ | 31,978 | 512 | 512 | 1 s | yes |
| 12 | Singleton | master | built-in, _Auto_ | 127,212 | 2,048 | 2,048 | 12 s | yes |
| 12 | Singleton | master | built-in, _On_ | 127,212 | 2,048 | 2,048 | 12 s | yes |
| 10 | Singleton | master | `ctx.Let` binding, _Auto_ | 23,794 | 512 | 512 | 1 s | yes |
| 10 | Singleton | master | `ctx.Let` binding, _On_ | 23,794 | 512 | 512 | 2 s | yes |
| 10 | Scoped | master | built-in, _On_ | 32,004 | 512 | 512 | 2 s | yes |
| 10 | Transient | master | built-in, _Auto_ | 18,668 | 512 | – | 1 s | yes |
| 10 | Singleton | 2.5.7 | built-in | 31,978 | 512 | 512 | 2 s | |

The `ctx.Let` binding is:

```csharp
.Bind<Func<TT1, TT>>()
    .As(PerBlock)
    .To(ctx => new Func<TT1, TT>((TT1 arg1) =>
    {
        ctx.Let<TT1>(arg1);
        ctx.Inject<TT>(ctx.Tag, out var value);
        return value;
    }))
```

When the items do not consume the delegate arguments (`ItemA0(Service1 next)`), master with _On_ generates 584 lines for 10 levels and one function per singleton. With _Auto_ it still generates 25,849 lines, with the creation code in place in every lambda.

The outer delegate's argument type does not matter. A singleton `ContextProvider(Func<User, Context> open)`, with `Context(User user)`, reached from three `Func<Condition, EvaluatorN>` delegates, gets three `EnsureContextProviderExists` functions.

DIW016 is reported, but its advice does not help: the services already have a shared lifetime, the `LocalFunctions` hint is on, and the `ctx.Let` binding doubles in the same way.

In a real application, the singletons on a cycle through such delegates are reached from every delegate with arguments in the graph. The cycle is a context provider that opens contexts with `Func<IUser, IProject, IContext>`, a configuration provider with `Lazy<IProjectionService>`, and a projection service that needs the context provider. One composition root came to 2.2 million lines (377 MB), with 32,056 `Ensure…Exists` functions for 224 singletons, and the C# compiler did not finish compiling it. The result is the same with `LocalFunctions` _On_ and _Auto_.

## Suggestion

A shared instance reads no override from the scope it is reached from when every consumer of the overridden type in its subgraph is created by a delegate that overrides that type itself. Such an instance needs one function at the root level, like a singleton outside any delegate.
