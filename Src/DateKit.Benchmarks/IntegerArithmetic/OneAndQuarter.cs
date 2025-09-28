using System;

using BenchmarkDotNet.Attributes;

namespace DateKit.Benchmarks.IntegerArithmetic;

[StandardConfig]
public class OneAndQuarter
{
	private const Int32 MaxValue = 15;

	[Benchmark]
	public Int32 OnePlusQuarter_AddFourth()
	{
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += OnePlusQuarter(value);
		return sum;

		static Int32 OnePlusQuarter(Int32 value)
		{
			return value + (value >>> 2);
		}
	}

	[Benchmark]
	public Int32 OnePlusQuarter_MultiplyByFiveFourths()
	{
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += OnePlusQuarter(value);
		return sum;

		static Int32 OnePlusQuarter(Int32 value)
		{
			return (value * 5) >>> 2;
		}
	}

	[Benchmark]
	public Int32 OneMinusQuarter_SubtractFourth()
	{
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += OneMinusQuarter(value);
		return sum;

		static Int32 OneMinusQuarter(Int32 value)
		{
			return value - (value >>> 2);
		}
	}

	[Benchmark]
	public Int32 OneMinusQuarter_MultiplyByThreeFourths()
	{
		Int32 sum = 0;
		for (Int32 value = MaxValue; value >= 0; --value)
			sum += OneMinusQuarter(value);
		return sum;

		static Int32 OneMinusQuarter(Int32 value)
		{
			return ((value + 1) * 3) >>> 2;
		}
	}
}
