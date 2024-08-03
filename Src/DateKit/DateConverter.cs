using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace DateKit;

/// <summary>
/// Provides a type converter to convert <see cref="Date" /> objects to and from various other representations.
/// </summary>
public class DateConverter : TypeConverter
{
	private static readonly PropertyInfo s_emptyProperty =
		typeof(Date).GetProperty(nameof(Date.Empty), BindingFlags.Public | BindingFlags.Static)!;
	private static readonly ConstructorInfo s_constructor =
		typeof(Date).GetConstructor([typeof(Int32), typeof(Int32), typeof(Int32)])!;

	/// <inheritdoc />
	public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		return sourceType == typeof(String) || base.CanConvertFrom(context, sourceType);
	}

	/// <inheritdoc />
	public override Boolean CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
	{
		return base.CanConvertTo(context, destinationType) || destinationType == typeof(InstanceDescriptor);
	}

	/// <inheritdoc />
	public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
	{
		if (value is String s)
		{
			return s == ""
				? Date.Empty
				// ReSharper disable once PossibleUnintendedReferenceComparison
				: culture == CultureInfo.InvariantCulture
					? Date.ParseIsoString(s)
					: DatePattern.Create(format: null, culture).ParseExact(s);
		}

		return base.ConvertFrom(context, culture, value);
	}

	/// <inheritdoc />
	public override Object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, Object? value, Type destinationType)
	{
		if (value is Date date)
		{
			if (destinationType == typeof(String))
			{
				// ReSharper disable once PossibleUnintendedReferenceComparison
				return culture == CultureInfo.InvariantCulture
					? date.ToIsoString()
					: date.ToString(format: null, culture);
			}
			else if (destinationType == typeof(InstanceDescriptor))
			{
				return date != Date.Empty
					? new InstanceDescriptor(s_constructor, new Object[] { date.Year, date.Month, date.Day })
					: new InstanceDescriptor(s_emptyProperty, null);
			}
		}

		return base.ConvertTo(context, culture, value, destinationType);
	}
}
