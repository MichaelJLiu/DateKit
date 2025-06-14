using System;
using System.Diagnostics;
#if NETCOREAPP3_0_OR_GREATER
using BitOperations = System.Numerics.BitOperations;
#endif

namespace DateKit;

internal static class BitCounting
{
	/// <summary>
	/// Returns the zero-based index of the least significant 1 bit in a specified nonzero eight-bit integer value.
	/// </summary>
	public static Int32 GetLowestSetBit(Int32 value)
	{
		Debug.Assert(value >= 1);
		Debug.Assert(value <= 255);

#if NETCOREAPP3_0_OR_GREATER
		return BitOperations.TrailingZeroCount(value);
#else
		// Clear all bits to the left of the rightmost 1 bit:
		value &= -value;
		// Result: One of 2^[0..7] = (1, 2, 4, 8, 16, 32, 64, 128)

		// Map each of the eight possible values to a unique code using a de Bruijn sequence of order 3:
		value = unchecked((Byte)(value * 0b00010111)) >>> 5; // (0, 1, 2, 5, 3, 7, 6, 4)

		// Map the code to the index of the least significant 1 bit:
		return (0x56374210 >>> (value * 4)) & 0xF; // (0, 1, 2, 3, 4, 5, 6, 7)
#endif
	}

	/// <summary>
	/// Returns the zero-based index of the most significant 1 bit in a specified nonzero eight-bit integer value.
	/// </summary>
	public static Int32 GetHighestSetBit(Int32 value)
	{
		Debug.Assert(value >= 1);
		Debug.Assert(value <= 255);

#if NETCOREAPP3_0_OR_GREATER
		return BitOperations.Log2((UInt32)value);
#else
		// Propagate the leftmost 1 bit to the right:
		value |= value >>> 1;
		value |= value >>> 2;
		value |= value >>> 4;
		// Result: One of 2^[1..8] - 1 = (1, 3, 7, 15, 31, 63, 127, 255)

		// Map each of the eight possible values to a unique code using a de Bruijn sequence of order 3:
		value = unchecked((Byte)(value * 0b00011101)) >>> 5; // (0, 2, 6, 5, 4, 1, 3, 7)

		// Map the code to the index of the most significant 1 bit:
		return (0x72346150 >>> (value * 4)) & 0xF; // (0, 1, 2, 3, 4, 5, 6, 7)
#endif
	}
}
