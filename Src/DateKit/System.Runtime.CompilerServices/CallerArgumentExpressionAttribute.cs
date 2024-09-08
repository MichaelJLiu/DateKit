#if !NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter)]
[ExcludeFromCodeCoverage]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
	public CallerArgumentExpressionAttribute(String parameterName)
	{
		this.ParameterName = parameterName;
	}

	public String ParameterName { get; }
}
#endif
