using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit;

/// <summary>
/// Tests the <see cref="DateConverter" /> class.
/// </summary>
[TestFixture]
public class DateConverterTests
{
	private readonly TypeConverter _dateConverter = TypeDescriptor.GetConverter(typeof(Date));

	[TestCase(typeof(Boolean), false)]
	[TestCase(typeof(InstanceDescriptor), true)]
	[TestCase(typeof(String), true)]
	public void CanConvertFrom_ReturnsExpected(Type sourceType, Boolean expectedResult)
	{
		// Act:
		Object actualResult = _dateConverter.CanConvertFrom(sourceType);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	[TestCase(typeof(Boolean), false)]
	[TestCase(typeof(InstanceDescriptor), true)]
	[TestCase(typeof(String), true)]
	public void CanConvertTo_ReturnsExpected(Type destinationType, Boolean expectedResult)
	{
		// Act:
		Object actualResult = _dateConverter.CanConvertTo(destinationType);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	#region ConvertFrom

	[Test]
	public void ConvertFrom_WithInvalidValue_ThrowsException()
	{
		// Arrange:
		Object value = false;

		// Act:
		Func<Object?> func = () => _dateConverter.ConvertFrom(value);

		// Assert:
		func.Should().Throw<NotSupportedException>();
	}

	[Test]
	public void ConvertFromInvariantString_WithEmptyString_ReturnsEmptyDate()
	{
		// Arrange:
		const String value = "";

		// Act:
		Object? actualResult = _dateConverter.ConvertFromInvariantString(value);

		// Assert:
		actualResult.Should().Be(Date.Empty);
	}

	[Test]
	public void ConvertFromInvariantString_WithValidDateString_ReturnsExpected()
	{
		// Arrange:
		const String value = "2000-06-15";

		// Act:
		Object? actualResult = _dateConverter.ConvertFromInvariantString(value);

		// Assert:
		actualResult.Should().Be(new Date(2000, 6, 15));
	}

	[Test]
	public void ConvertFromString_WithEmptyString_ReturnsEmptyDate()
	{
		// Arrange:
		const String value = "";

		// Act:
		Object? actualResult = _dateConverter.ConvertFromString(value);

		// Assert:
		actualResult.Should().Be(Date.Empty);
	}

	[Test]
	[SetCulture("en-US")]
	public void ConvertFromString_WithValidDateString_ReturnsExpected()
	{
		// Arrange:
		const String value = "6/15/2000";

		// Act:
		Object? actualResult = _dateConverter.ConvertFromString(value);

		// Assert:
		actualResult.Should().Be(new Date(2000, 6, 15));
	}

	#endregion

	#region ConvertTo

	[Test]
	public void ConvertTo_WithInvalidType_ThrowsException()
	{
		// Arrange:
		Object value = new Date(2000, 6, 15);

		// Act:
		Func<Object?> func = () => _dateConverter.ConvertTo(value, typeof(Boolean));

		// Assert:
		func.Should().Throw<NotSupportedException>();
	}

	[Test]
	public void ConvertTo_WithEmptyDateAndInstanceDescriptorType_ReturnsExpected()
	{
		// Arrange:
		Object value = Date.Empty;

		// Act:
		Object? actualResult = _dateConverter.ConvertTo(value, typeof(InstanceDescriptor));

		// Assert:
		actualResult.Should().BeOfType<InstanceDescriptor>().Which.Invoke().Should().Be(value);
	}

	[Test]
	public void ConvertTo_WithValidDateAndInstanceDescriptorType_ReturnsExpected()
	{
		// Arrange:
		Object value = new Date(2000, 6, 15);

		// Act:
		Object? actualResult = _dateConverter.ConvertTo(value, typeof(InstanceDescriptor));

		// Assert:
		actualResult.Should().BeOfType<InstanceDescriptor>().Which.Invoke().Should().Be(value);
	}

	[Test]
	public void ConvertToInvariantString_WithEmptyDate_ReturnsEmptyString()
	{
		// Arrange:
		Object value = Date.Empty;

		// Act:
		String? actualResult = _dateConverter.ConvertToInvariantString(value);

		// Assert:
		actualResult.Should().BeEmpty();
	}

	[Test]
	public void ConvertToInvariantString_WithValidDate_ReturnsExpected()
	{
		// Arrange:
		Object value = new Date(2000, 6, 15);

		// Act:
		String? actualResult = _dateConverter.ConvertToInvariantString(value);

		// Assert:
		actualResult.Should().Be("2000-06-15");
	}

	[Test]
	public void ConvertToString_WithNullValue_ReturnsEmptyString()
	{
		// Arrange:
		Object? value = null;

		// Act:
		String? actualResult = _dateConverter.ConvertToString(value);

		// Assert:
		actualResult.Should().Be("");
	}

	[Test]
	[SetCulture("en-US")]
	public void ConvertToString_WithEmptyDate_ReturnsEmptyString()
	{
		// Arrange:
		Object value = Date.Empty;

		// Act:
		String? actualResult = _dateConverter.ConvertToString(value);

		// Assert:
		actualResult.Should().Be("");
	}

	[Test]
	[SetCulture("en-US")]
	public void ConvertToString_WithValidDate_ReturnsExpected()
	{
		// Arrange:
		Object value = new Date(2000, 6, 15);

		// Act:
		Object? actualResult = _dateConverter.ConvertToString(value);

		// Assert:
		actualResult.Should().Be("6/15/2000");
	}

	#endregion
}
