using System;

using BenchmarkDotNet.Attributes;

namespace DateKit.Benchmarks.IntegerArithmetic;

[StandardConfig]
public class Divisibility
{
	private const Int32 MaxValue = 15;

	[Benchmark]
	public Int32 IsDivisibleBy25_Unoptimized()
	{
		const Int32 divisor = 25;
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += IsDivisibleBy25(value) ? 1 : 0;
		return sum;

		static Boolean IsDivisibleBy25(Int32 value)
		{
			return (UInt32)value % divisor == 0;
		}
	}

	[Benchmark]
	public Int32 IsDivisibleBy25_Shift32()
	{
		const Int32 divisor = 25;
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += IsDivisibleBy25(value) ? 1 : 0;
		return sum;

		static Boolean IsDivisibleBy25(Int32 value)
		{
			const Int32 shift = 32;
			const Int32 multiplier = (Int32)((1L << shift) / divisor) + 1;
			return unchecked((UInt32)value * multiplier) < multiplier;
		}
	}
}
