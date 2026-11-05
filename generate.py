# python3 generate.py <levels> [lifetime]: regenerates Program.cs
import sys

levels = int(sys.argv[1])
lifetime = sys.argv[2] if len(sys.argv) > 2 else 'Singleton'
out = [
    'using Pure.DI;',
    'using static Pure.DI.Lifetime;',
    '',
    'var composition = new Composition();',
    'Console.WriteLine(composition.Root);',
    '',
    'public sealed class A;',
    'public sealed class B;',
    '',
]
for i in range(levels):
    out.append(f'public sealed class Service{i}(Func<A, ItemA{i}> openA, Func<B, ItemB{i}> openB);')
    nxt = f', Service{i + 1} next' if i + 1 < levels else ''
    out.append(f'public sealed class ItemA{i}(A a{nxt});')
    out.append(f'public sealed class ItemB{i}(B b{nxt});')
    out.append('')
out += [
    'partial class Composition',
    '{',
    '    private static void Setup() => DI.Setup(nameof(Composition))',
    '#if LOCAL_FUNCTIONS_ON',
    '        .Hint(Hint.LocalFunctions, "On")',
    '#endif',
    '#if LET_FUNC',
    '        .Bind<Func<TT1, TT>>()',
    '            .As(PerBlock)',
    '            .To(ctx => new Func<TT1, TT>((TT1 arg1) =>',
    '            {',
    '                ctx.Let<TT1>(arg1);',
    '                ctx.Inject<TT>(ctx.Tag, out var value);',
    '                return value;',
    '            }))',
    '#endif',
]
for i in range(levels):
    out.append(f'        .Bind().As({lifetime}).To<Service{i}>()')
out += ['        .Root<Service0>("Root");', '}']
open('Program.cs', 'w').write('\n'.join(out) + '\n')
