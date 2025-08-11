using System;
using System.Collections.Generic;
using System.Linq;

namespace DateKit.Schedules;

internal class MockAnnualRecurrence : AnnualRecurrence
{
	private readonly Dictionary<Int32, Date> _getOccurrence;

	public MockAnnualRecurrence(
		AnnualRecurrenceOptions options,
		IEnumerable<(Int32, Date)>? getOccurrence = null)
		: base(options)
	{
		_getOccurrence = getOccurrence != null
			? getOccurrence.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2)
			: [];
	}

	protected override Date GetOccurrenceCore(Int32 year)
	{
		return _getOccurrence[year];
	}
}
