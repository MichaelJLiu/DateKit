using System;
using System.Runtime.CompilerServices;

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Formatting;

namespace DateKit;

public static class AssertionExtensions
{
	public static DateAssertions Should(this Date date)
	{
		return new DateAssertions(date);
	}
}

public class DateAssertions
{
	public DateAssertions(Date date)
	{
		this.Subject = date;
	}

	public Date Subject { get; }

	/// <summary>
	/// Asserts that the current <see cref="Date" /> is equal to a specified <see cref="Date" />
	/// using the <see cref="Date.Equals(Date, Date)" /> method.
	/// </summary>
	[CustomAssertion]
	public AndConstraint<DateAssertions> Be(Date expected)
	{
		Date subject = this.Subject;
		Execute.Assertion
			.ForCondition(Date.Equals(subject, expected))
			.FailWith("Expected {context:date} to be {0}, but found {1}.", expected, subject);
		return new AndConstraint<DateAssertions>(this);
	}

	/// <summary>
	/// Asserts that the current <see cref="Date" /> has the specified year, month, and day components.
	/// </summary>
	[CustomAssertion]
	public AndConstraint<DateAssertions> HaveComponents(Int32 year, Int32 month, Int32 day)
	{
		Date subject = this.Subject;
		Execute.Assertion
			.ForCondition(subject.Year == year && subject.Month == month && subject.Day == day)
			.FailWith("Expected {context:date} to be {0}, but found {1}.", new Date(year, month, day), subject);
		return new AndConstraint<DateAssertions>(this);
	}

	/// <summary>
	/// Asserts that the current <see cref="Date" /> has the same year, month, and day components
	/// as a specified <see cref="DateTime" />.
	/// </summary>
	[CustomAssertion]
	public AndConstraint<DateAssertions> HaveSameComponentsAs(DateTime expected)
	{
		return this.HaveComponents(expected.Year, expected.Month, expected.Day);
	}
}

internal class DateValueFormatter : IValueFormatter
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		Formatter.AddFormatter(new DateValueFormatter());
	}

	public Boolean CanHandle(Object value)
	{
		return value is Date;
	}

	public void Format(
		Object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
	{
		Date date = (Date)value;
		formattedGraph.AddFragment(date.Year != 0 ? $"<{date.Year:D4}-{date.Month:D2}-{date.Day:D2}>" : "<empty>");
	}
}
