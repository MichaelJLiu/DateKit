using System;
using System.Collections.Generic;

namespace DateKit.Schedules;

internal class MockAnnualRecurrence : AnnualRecurrence
{
	public MockAnnualRecurrence(AnnualRecurrenceOptions options)
		: base(options)
	{
	}

	public override void GetDayOfYearRange(out Int32 minDayOfYear, out Int32 maxDayOfYear)
	{
		throw new NotImplementedException();
	}

	protected override Boolean ContainsCore(Date date)
	{
		throw new NotImplementedException();
	}

	protected override IEnumerable<Date> EnumerateBackwardFromCore(Date date)
	{
		throw new NotImplementedException();
	}

	protected override IEnumerable<Date> EnumerateForwardFromCore(Date date)
	{
		throw new NotImplementedException();
	}

	protected override Date GetOccurrenceCore(Int32 year)
	{
		throw new NotImplementedException();
	}
}
