using System;

using BenchmarkDotNet.Attributes;

namespace DateKit.Benchmarks.IntegerArithmetic;

[StandardConfig]
public class Division
{
	private const Int32 MaxValue = 15;

	[Benchmark]
	public Int32 Div10_Unoptimized()
	{
		const Int32 divisor = 10;
		Int32 sum = 0;
		for (Int32 value = MaxValue * divisor; value >= 0; value -= divisor)
			sum += Div10(value);
		return sum;

		static Int32 Div10(Int32 value)
		{
			return (Int32)((UInt32)value / divisor);
		}
	}

	[Benchmark]
	public Int32 Div10_Shift16()
	{
		const Int32 divisor = 10;
		Int32 sum = 0;
		for (Int32 value = MaxValue * divisor; value >= 0; value -= divisor)
			sum += Div10(value);
		return sum;

		static Int32 Div10(Int32 value)
		{
			const Int32 shift = 16;
			const Int32 multiplier = (1 << shift) / divisor + 1;
			return (value * multiplier) >>> shift;
		}
	}

	[Benchmark]
	public Int32 Div12_Unoptimized()
	{
		const Int32 divisor = 12;
		Int32 sum = 0;
		for (Int32 value = MaxValue * divisor; value >= 0; value -= divisor)
			sum += Div12(value);
		return sum;

		static Int32 Div12(Int32 value)
		{
			return (Int32)((UInt32)value / divisor);
		}
	}

	[Benchmark]
	public Int32 Div100_Unoptimized()
	{
		const Int32 divisor = 100;
		Int32 sum = 0;
		for (Int32 value = MaxValue * divisor; value >= 0; value -= divisor)
			sum += Div100(value);
		return sum;

		static Int32 Div100(Int32 value)
		{
			return (Int32)((UInt32)value / divisor);
		}
	}

	[Benchmark]
	public Int32 Div100_Shift19()
	{
		const Int32 divisor = 100;
		Int32 sum = 0;
		for (Int32 value = MaxValue * divisor; value >= 0; value -= divisor)
			sum += Div100(value);
		return sum;

		static Int32 Div100(Int32 value)
		{
			const Int32 shift = 19;
			const Int32 multiplier = (1 << shift) / divisor + 1;
			return (value * multiplier) >>> shift;
		}
	}

	[Benchmark]
	public Int32 Div100_Shift32()
	{
		const Int32 divisor = 100;
		Int32 sum = 0;
		for (Int32 value = MaxValue * divisor; value >= 0; value -= divisor)
			sum += Div100(value);
		return sum;

		static Int32 Div100(Int32 value)
		{
			const Int32 shift = 32;
			const Int32 multiplier = (Int32)((1L << shift) / divisor) + 1;
			return (Int32)(((UInt64)(UInt32)value * multiplier) >>> shift);
		}
	}
}
