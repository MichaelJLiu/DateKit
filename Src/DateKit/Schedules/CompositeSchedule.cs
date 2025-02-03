using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DateKit.Schedules;

/// <summary>
/// Represents the combination (set union) of a collection of other schedules.
/// </summary>
/// <remarks>
/// A <see cref="CompositeSchedule" /> contains every date that is in at least one of its <see cref="BaseSchedules" />.
/// </remarks>
/// <threadsafety static="true" instance="true" />
public sealed class CompositeSchedule : Schedule
{
	private static readonly Comparison<Date>
		s_dateComparison = Date.Compare,
		s_reverseDateComparison = Date.ReverseCompare;

	private static readonly Func<Schedule, Date, IEnumerable<Date>>
		s_enumerateBackwardFrom = CreateEnumerateFromDelegate(nameof(Schedule.EnumerateBackwardFrom)),
		s_enumerateForwardFrom = CreateEnumerateFromDelegate(nameof(Schedule.EnumerateForwardFrom));

	private static Func<Schedule, Date, IEnumerable<Date>> CreateEnumerateFromDelegate(String methodName)
	{
		return (Func<Schedule, Date, IEnumerable<Date>>)Delegate.CreateDelegate(
			typeof(Func<Schedule, Date, IEnumerable<Date>>),
			null,
			typeof(Schedule).GetMethod(methodName)!);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CompositeSchedule" /> class.
	/// </summary>
	/// <param name="baseSchedules">
	/// The collection of schedules to combine.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="baseSchedules" /> is uninitialized.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="baseSchedules" /> contains a <see langword="null" /> schedule.
	/// </exception>
	public CompositeSchedule(params ImmutableArray<Schedule> baseSchedules)
	{
		if (baseSchedules.IsDefault)
			ThrowHelper.ThrowArgumentNullException(nameof(baseSchedules));

		if (baseSchedules.Contains(null!))
			throw new ArgumentException("The collection contains a null schedule.", nameof(baseSchedules));

		this.BaseSchedules = baseSchedules;
	}

	/// <summary>
	/// Gets the collection of base schedules.
	/// </summary>
	/// <value>
	/// The collection of schedules to combine.
	/// </value>
	public ImmutableArray<Schedule> BaseSchedules { get; }

	/// <inheritdoc />
	public override Boolean Contains(Date date)
	{
		foreach (Schedule baseSchedule in this.BaseSchedules)
			if (baseSchedule.Contains(date))
				return true;

		return false;
	}

	/// <inheritdoc />
	public override IEnumerable<Date> EnumerateBackwardFrom(Date date)
	{
		ThrowHelper.ThrowIfDateArgumentIsEmpty(date, ExceptionArgument.date);
		return this.EnumerateIterator(s_enumerateBackwardFrom, date, s_reverseDateComparison);
	}

	/// <inheritdoc />
	public override IEnumerable<Date> EnumerateForwardFrom(Date date)
	{
		ThrowHelper.ThrowIfDateArgumentIsEmpty(date, ExceptionArgument.date);
		return this.EnumerateIterator(s_enumerateForwardFrom, date, s_dateComparison);
	}

	private IEnumerable<Date> EnumerateIterator(
		Func<Schedule, Date, IEnumerable<Date>> enumerateFrom, Date date, Comparison<Date> dateComparison)
	{
		Heap<IEnumerator<Date>, Date> heap = new(dateComparison, this.BaseSchedules.Length);

		foreach (Schedule baseSchedule in this.BaseSchedules)
		{
			IEnumerator<Date> enumerator = enumerateFrom(baseSchedule, date).GetEnumerator();
			if (enumerator.MoveNext())
				heap.Append(enumerator, enumerator.Current);
			else
				enumerator.Dispose();
		}

		heap.Heapify();

		Date previousDate = Date.Empty;

		while (heap.Count > 0)
		{
			IEnumerator<Date> enumerator = heap.GetRoot();
			date = enumerator.Current;

			if (date != previousDate)
			{
				yield return date;
				previousDate = date;
			}

			if (enumerator.MoveNext())
			{
				heap.ReplaceRoot(enumerator, enumerator.Current);
			}
			else
			{
				enumerator.Dispose();
				heap.RemoveRoot();
			}
		}
	}
}
