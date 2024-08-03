using System;
using System.Collections.Generic;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit;

partial class DateTests
{
	[TestCase(1999, 7, 16, -1)] // earlier year
	[TestCase(2000, 5, 16, -1)] // earlier month
	[TestCase(2000, 6, 14, -1)] // earlier day
	[TestCase(2000, 6, 15, 0)] // same date
	[TestCase(2000, 6, 16, 1)] // later day
	[TestCase(2000, 7, 14, 1)] // later month
	[TestCase(2001, 5, 14, 1)] // later year
	public void Compare_ReturnsExpected(Int32 year, Int32 month, Int32 day, Int32 expectedSign)
	{
		// Arrange:
		Date date1 = new(year, month, day);
		Date date2 = new(2000, 6, 15);

		// Act:
		Int32 actualCompareToDateResult = date1.CompareTo(date2);
		Int32 actualCompareToObjectResult = date1.CompareTo((Object)date2);
		Boolean actualLessThanResult = date1 < date2;
		Boolean actualLessThanOrEqualResult = date1 <= date2;
		Boolean actualGreaterThanResult = date1 > date2;
		Boolean actualGreaterThanOrEqualResult = date1 >= date2;

		// Assert:
		Math.Sign(actualCompareToDateResult).Should().Be(expectedSign);
		Math.Sign(actualCompareToObjectResult).Should().Be(expectedSign);
		actualLessThanResult.Should().Be(expectedSign < 0);
		actualLessThanOrEqualResult.Should().Be(expectedSign <= 0);
		actualGreaterThanResult.Should().Be(expectedSign > 0);
		actualGreaterThanOrEqualResult.Should().Be(expectedSign >= 0);
	}

	[Test]
	public void CompareToObject_WithNull_ReturnsPositiveInteger()
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		// Act:
		Int32 actualResult = date.CompareTo(null);

		// Assert:
		actualResult.Should().BePositive();
	}

	[Test]
	public void CompareToObject_WithInvalidArgument_ThrowsException()
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		// Act:
		Func<Int32> func = () => date.CompareTo("");

		// Assert:
		func.Should().Throw<ArgumentException>().WithParameterName("value");
	}

	/// <summary>
	/// Tests the <see cref="Date.Equals(Date)" /> and <see cref="Date.Equals(Date, Date)" /> methods,
	/// as well as the <see cref="Date.op_Equality" /> and <see cref="Date.op_Inequality" /> operators.
	/// </summary>
	[TestCase(2000, 6, 15, true)] // same date
	[TestCase(2001, 6, 15, false)] // different year
	[TestCase(2000, 7, 15, false)] // different month
	[TestCase(2000, 6, 16, false)] // different day
	public void Equals_ReturnsExpected(Int32 year, Int32 month, Int32 day, Boolean expectedResult)
	{
		// Arrange:
		Date date1 = new(year, month, day);
		Date date2 = new(2000, 6, 15);

		// Act:
		Boolean actualEqualsResult = date1.Equals(date2);
		Boolean actualEqualityResult = date1 == date2;
		Boolean actualInequalityResult = date1 != date2;

		// Assert:
		actualEqualsResult.Should().Be(expectedResult);
		actualEqualityResult.Should().Be(expectedResult);
		actualInequalityResult.Should().Be(!expectedResult);
	}

	[Test]
	public void Equals_WithNull_ReturnsFalse()
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		// Act:
		Boolean actualResult = date.Equals(null);

		// Assert:
		actualResult.Should().BeFalse();
	}

	[Test]
	public void EqualsAndGetHashCode_SupportDictionaryKeys()
	{
		// Arrange:
		Date date1 = Date.Empty;
		Date date2 = new(2000, 6, 15);
		Date date3 = new(2000, 6, 16);
		const Int32 value1 = 1;
		const Int32 value2 = 2;
		const Int32 value3 = 3;
		Dictionary<Object, Int32> dictionary = [];

		// Act:
		dictionary.Add(date1, value1);
		dictionary.Add(date2, value2);
		dictionary.Add(date3, value3);
		Int32 actualValue1 = dictionary[date1];
		Int32 actualValue2 = dictionary[date2];
		Int32 actualValue3 = dictionary[date3];

		// Assert:
		actualValue1.Should().Be(value1);
		actualValue2.Should().Be(value2);
		actualValue3.Should().Be(value3);
	}
}
