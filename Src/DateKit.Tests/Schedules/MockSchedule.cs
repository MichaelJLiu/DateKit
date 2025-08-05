using System;
using System.Collections.Generic;
using System.Linq;

namespace DateKit.Schedules;

internal class MockSchedule : Schedule
{
	private readonly Dictionary<Date, Boolean> _contains;
	private readonly Dictionary<Date, IEnumerable<Date>> _enumerateBackwardFrom;
	private readonly Dictionary<Date, IEnumerable<Date>> _enumerateForwardFrom;

	public MockSchedule(
		IEnumerable<(Date, Boolean)>? contains = null,
		IEnumerable<(Date, IEnumerable<Date>)>? enumerateBackwardFrom = null,
		IEnumerable<(Date, IEnumerable<Date>)>? enumerateForwardFrom = null)
	{
		_contains = contains != null
			? contains.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2)
			: [];
		_enumerateBackwardFrom = enumerateBackwardFrom != null
			? enumerateBackwardFrom.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2)
			: [];
		_enumerateForwardFrom = enumerateForwardFrom != null
			? enumerateForwardFrom.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2)
			: [];
	}

	protected override Boolean ContainsCore(Date date)
	{
		return _contains[date];
	}

	protected override IEnumerable<Date> EnumerateBackwardFromCore(Date date)
	{
		return _enumerateBackwardFrom[date];
	}

	protected override IEnumerable<Date> EnumerateForwardFromCore(Date date)
	{
		return _enumerateForwardFrom[date];
	}
}
