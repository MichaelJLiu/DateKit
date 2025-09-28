using System;

using BenchmarkDotNet.Attributes;

namespace DateKit.Benchmarks.IntegerArithmetic;

[StandardConfig]
public class Remainder
{
	private const Int32 MaxValue = 15;

	[Benchmark]
	public Int32 Mod7_Unoptimized()
	{
		const Int32 divisor = 7;
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += Mod7(value);
		return sum;

		static Int32 Mod7(Int32 value)
		{
			return (Int32)((UInt32)value % divisor);
		}
	}

	[Benchmark]
	public Int32 Mod7_Shift8()
	{
		const Int32 divisor = 7;
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += Mod7(value);
		return sum;

		static Int32 Mod7(Int32 value)
		{
			const Int32 shift = 8;
			const Int32 multiplier = (1 << shift) / divisor + 1;
			return (Int32)(((UInt32)value * multiplier % (1 << shift) * divisor) >>> shift);
		}
	}

	[Benchmark]
	public Int32 Mod7_Minus1Shift29()
	{
		const Int32 divisor = 7;
		Int32 sum = 0;
		for (Int32 value = MaxValue + 1; value > 0; --value)
			sum += Minus1Mod7(value);
		return sum;

		static Int32 Minus1Mod7(Int32 value)
		{
			const Int32 multiplier = (Int32)((1L << 32) / divisor);
			return (unchecked(value * multiplier) >>> 29) - 1;
		}
	}

	[Benchmark]
	public Int32 Mod10_Unoptimized()
	{
		const Int32 divisor = 10;
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += Mod10(value);
		return sum;

		static Int32 Mod10(Int32 value)
		{
			return (Int32)((UInt32)value % divisor);
		}
	}

	[Benchmark]
	public Int32 Mod10_Shift16()
	{
		const Int32 divisor = 10;
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += Mod10(value);
		return sum;

		static Int32 Mod10(Int32 value)
		{
			const Int32 shift = 16;
			const Int32 multiplier = (1 << shift) / divisor + 1;
			return (Int32)(((UInt32)value * multiplier % (1 << shift) * divisor) >>> shift);
		}
	}

	[Benchmark]
	public Int32 Mod100_Unoptimized()
	{
		const Int32 divisor = 100;
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += Mod100(value);
		return sum;

		static Int32 Mod100(Int32 value)
		{
			return (Int32)((UInt32)value % divisor);
		}
	}

	[Benchmark]
	public Int32 Mod100_Shift19()
	{
		const Int32 divisor = 100;
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += Mod100(value);
		return sum;

		static Int32 Mod100(Int32 value)
		{
			const Int32 shift = 19;
			const Int32 multiplier = (1 << shift) / divisor + 1;
			return (Int32)(((UInt32)value * multiplier % (1 << shift) * divisor) >>> shift);
		}
	}

	[Benchmark]
	public Int32 Mod100_Shift32()
	{
		const Int32 divisor = 100;
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += Mod100(value);
		return sum;

		static Int32 Mod100(Int32 value)
		{
			const Int32 shift = 32;
			const Int32 multiplier = (Int32)((1L << shift) / divisor) + 1;
			return (Int32)(((UInt64)((UInt32)value * multiplier) * divisor) >>> shift);
		}
	}
}
