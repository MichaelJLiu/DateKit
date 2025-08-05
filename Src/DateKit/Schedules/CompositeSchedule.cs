using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DateKit.Schedules;

using EnumerateFromFunc = Func<Schedule, Date, IEnumerable<Date>>;

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

	private static readonly EnumerateFromFunc
		s_enumerateBackwardFromCore = CreateEnumerateFromFunc(nameof(Schedule.EnumerateBackwardFromCore)),
		s_enumerateForwardFromCore = CreateEnumerateFromFunc(nameof(Schedule.EnumerateForwardFromCore));

	private static EnumerateFromFunc CreateEnumerateFromFunc(String methodName)
	{
		return (EnumerateFromFunc)Delegate.CreateDelegate(
			typeof(EnumerateFromFunc),
			firstArgument: null,
			typeof(Schedule).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!);
	}

	/// <summary>
	/// Creates a <see cref="CompositeSchedule" /> using a specified collection of base schedules.
	/// </summary>
	/// <param name="baseSchedules">
	/// An <see cref="ImmutableArray{T}" /> that contains the schedules to combine.
	/// </param>
	/// <returns>
	/// A <see cref="CompositeSchedule" /> that combines <paramref name="baseSchedules" />.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="baseSchedules" /> is uninitialized.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="baseSchedules" /> contains a <see langword="null" /> schedule.
	/// </exception>
	public static CompositeSchedule Create(params ImmutableArray<Schedule> baseSchedules)
	{
		return new CompositeSchedule(baseSchedules);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CompositeSchedule" /> class.
	/// </summary>
	/// <param name="baseSchedules">
	/// An <see cref="ImmutableArray{T}" /> that contains the schedules to combine.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="baseSchedules" /> is uninitialized.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="baseSchedules" /> contains a <see langword="null" /> schedule.
	/// </exception>
	private CompositeSchedule(ImmutableArray<Schedule> baseSchedules)
	{
		if (baseSchedules.IsDefault)
			ThrowHelper.ThrowArgumentNullException(nameof(baseSchedules));
		if (baseSchedules.Contains(null!))
			throw new ArgumentException("The collection cannot contain null references.", nameof(baseSchedules));

		this.BaseSchedules = baseSchedules;
	}

	/// <summary>
	/// Gets the collection of base schedules.
	/// </summary>
	/// <value>
	/// An <see cref="ImmutableArray{T}" /> that contains the schedules to combine.
	/// </value>
	public ImmutableArray<Schedule> BaseSchedules { get; }

	/// <inheritdoc />
	protected internal override Boolean ContainsCore(Date date)
	{
		foreach (Schedule baseSchedule in this.BaseSchedules)
			if (baseSchedule.ContainsCore(date))
				return true;

		return false;
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateBackwardFromCore(Date date)
	{
		return this.EnumerateIterator(s_enumerateBackwardFromCore, date, s_reverseDateComparison);
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateForwardFromCore(Date date)
	{
		return this.EnumerateIterator(s_enumerateForwardFromCore, date, s_dateComparison);
	}

	private IEnumerable<Date> EnumerateIterator(
		EnumerateFromFunc enumerateFrom, Date date, Comparison<Date> dateComparison)
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
