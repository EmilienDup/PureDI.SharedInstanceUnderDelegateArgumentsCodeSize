# Pure.DI: a singleton reached inside delegates with arguments gets a copy of its creation code for every delegate

Minimal repro for Pure.DI master (d3440f5) and 2.5.7. There is a chain of 10 singletons. Each opens its items through two delegates, `Func<A, ItemA>` and `Func<B, ItemB>`. Each item consumes its own delegate's argument and depends on the next singleton. The creation code of every singleton is repeated in every delegate lambda it is reached from: the last singleton gets 512 `EnsureService9Exists…()` functions (31,978 lines). The `LocalFunctions` hint and binding `Func<T1, T>` with `ctx.Let` make no difference.

[`ISSUE.md`](ISSUE.md) is the text to paste into a Pure.DI GitHub issue; it has the measurements from 6 to 12 levels.

```
dotnet build                                                                                   # 2.5.7 from nuget.org
dotnet build -p:PureDIVersion=<master build version> -p:RestoreAdditionalProjectSources=<folder with the package>
dotnet build -p:LocalFunctionsOn=true                                                          # .Hint(Hint.LocalFunctions, "On")
dotnet build -p:LetFunc=true                                                                   # Func<T1, T> bound with ctx.Let instead of the built-in binding
python3 generate.py <levels> [Singleton|Scoped|PerResolve|Transient]                           # regenerates Program.cs
```

The generated composition is written under `obj/Debug/net10.0/generated/Pure.DI`; `grep -c "void EnsureService9Exists" Composition.g.cs` gives 512.
