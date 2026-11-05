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
public sealed class ItemA2(A a, Service3 next);
public sealed class ItemB2(B b, Service3 next);

public sealed class Service3(Func<A, ItemA3> openA, Func<B, ItemB3> openB);
public sealed class ItemA3(A a, Service4 next);
public sealed class ItemB3(B b, Service4 next);

public sealed class Service4(Func<A, ItemA4> openA, Func<B, ItemB4> openB);
public sealed class ItemA4(A a, Service5 next);
public sealed class ItemB4(B b, Service5 next);

public sealed class Service5(Func<A, ItemA5> openA, Func<B, ItemB5> openB);
public sealed class ItemA5(A a, Service6 next);
public sealed class ItemB5(B b, Service6 next);

public sealed class Service6(Func<A, ItemA6> openA, Func<B, ItemB6> openB);
public sealed class ItemA6(A a, Service7 next);
public sealed class ItemB6(B b, Service7 next);

public sealed class Service7(Func<A, ItemA7> openA, Func<B, ItemB7> openB);
public sealed class ItemA7(A a, Service8 next);
public sealed class ItemB7(B b, Service8 next);

public sealed class Service8(Func<A, ItemA8> openA, Func<B, ItemB8> openB);
public sealed class ItemA8(A a, Service9 next);
public sealed class ItemB8(B b, Service9 next);

public sealed class Service9(Func<A, ItemA9> openA, Func<B, ItemB9> openB);
public sealed class ItemA9(A a);
public sealed class ItemB9(B b);

partial class Composition
{
    private static void Setup() => DI.Setup(nameof(Composition))
#if LOCAL_FUNCTIONS_ON
        .Hint(Hint.LocalFunctions, "On")
#endif
#if LET_FUNC
        .Bind<Func<TT1, TT>>()
            .As(PerBlock)
            .To(ctx => new Func<TT1, TT>((TT1 arg1) =>
            {
                ctx.Let<TT1>(arg1);
                ctx.Inject<TT>(ctx.Tag, out var value);
                return value;
            }))
#endif
        .Bind().As(Singleton).To<Service0>()
        .Bind().As(Singleton).To<Service1>()
        .Bind().As(Singleton).To<Service2>()
        .Bind().As(Singleton).To<Service3>()
        .Bind().As(Singleton).To<Service4>()
        .Bind().As(Singleton).To<Service5>()
        .Bind().As(Singleton).To<Service6>()
        .Bind().As(Singleton).To<Service7>()
        .Bind().As(Singleton).To<Service8>()
        .Bind().As(Singleton).To<Service9>()
        .Root<Service0>("Root");
}
