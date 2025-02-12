#if !NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class ModuleInitializerAttribute : Attribute
{
}
#endif
