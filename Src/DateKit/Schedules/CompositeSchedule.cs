using System;
using System.Collections.Generic;
using System.Linq;

namespace DateKit.Schedules;

/// <summary>
/// Represents the combination (set union) of a collection of other schedules.
/// </summary>
/// <remarks>
/// A <see cref="CompositeSchedule" /> contains every date that is in at least one of its <see cref="BaseSchedules" />.
/// </remarks>
/// <threadsafety static="true" instance="true" />
public class CompositeSchedule : Schedule
{
	private readonly Schedule[] _baseSchedules;

	/// <summary>
	/// Initializes a new instance of the <see cref="CompositeSchedule" /> class.
	/// </summary>
	/// <param name="baseSchedules">
	/// The collection of schedules to combine.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="baseSchedules" /> is <see langword="null" />.
	/// </exception>
	public CompositeSchedule(params Schedule[] baseSchedules)
		: this((IEnumerable<Schedule>)baseSchedules)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CompositeSchedule" /> class.
	/// </summary>
	/// <param name="baseSchedules">
	/// The collection of schedules to combine.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="baseSchedules" /> is <see langword="null" />.
	/// </exception>
	public CompositeSchedule(IEnumerable<Schedule> baseSchedules)
	{
		ThrowHelper.ThrowIfArgumentIsNull(baseSchedules);
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		_baseSchedules = baseSchedules.Where(baseSchedule => baseSchedule != null).ToArray();
	}

	/// <summary>
	/// Gets the collection of base schedules.
	/// </summary>
	/// <value>
	/// The collection of schedules to combine.
	/// </value>
	public IReadOnlyCollection<Schedule> BaseSchedules => Array.AsReadOnly(_baseSchedules);

	/// <inheritdoc />
	public override Boolean Contains(Date date)
	{
		return _baseSchedules.Any(baseSchedule => baseSchedule.Contains(date));
	}

	/// <inheritdoc />
	public override IEnumerable<Date> EnumerateBackwardFrom(Date date)
	{
		ThrowHelper.ThrowIfDateArgumentIsEmpty(date, ExceptionArgument.date);
		return this.EnumerateIterator(baseSchedule => baseSchedule.EnumerateBackwardFrom(date), Date.ReverseCompare);
	}

	/// <inheritdoc />
	public override IEnumerable<Date> EnumerateForwardFrom(Date date)
	{
		ThrowHelper.ThrowIfDateArgumentIsEmpty(date, ExceptionArgument.date);
		return this.EnumerateIterator(baseSchedule => baseSchedule.EnumerateForwardFrom(date), Date.Compare);
	}

	private IEnumerable<Date> EnumerateIterator(
		Func<Schedule, IEnumerable<Date>> enumerate, Comparison<Date> dateComparison)
	{
		Heap<IEnumerator<Date>, Date> heap = new(dateComparison, _baseSchedules.Length);

		foreach (Schedule baseSchedule in _baseSchedules)
		{
			IEnumerator<Date> enumerator = enumerate(baseSchedule).GetEnumerator();
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
			Date date = enumerator.Current;

			if (date != previousDate)
			{
				yield return date;
				previousDate = date;
			}

			if (enumerator.MoveNext())
				heap.ReplaceRoot(enumerator, enumerator.Current);
			else
			{
				enumerator.Dispose();
				heap.RemoveRoot();
			}
		}
	}
}
