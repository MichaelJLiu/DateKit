using System;
using System.Collections.Generic;
using System.Diagnostics;
#if NET7_0_OR_GREATER // for StringSyntaxAttribute
using System.Diagnostics.CodeAnalysis;
#endif
using System.Globalization;
using System.Linq;
using System.Text;

namespace DateKit;

/// <summary>
/// Represents a culture-specific date format pattern.
/// </summary>
/// <threadsafety static="true" instance="true" />
public class DatePattern
{
	// yyyy-MM-dd
	internal static readonly DatePattern Iso8601 = new(
		FourDigitYearToken.Instance, LiteralToken.Hyphen, TwoDigitMonthToken.Instance, LiteralToken.Hyphen,
		TwoDigitDayToken.Instance);

	// ddd, dd MMM yyyy
	internal static readonly DatePattern Rfc1123 = new(
		DayNameToken.InvariantAbbreviated, new LiteralToken(", "), TwoDigitDayToken.Instance, LiteralToken.Space,
		MonthNameToken.InvariantAbbreviated, LiteralToken.Space, FourDigitYearToken.Instance);

	private readonly Token[] _tokens;
	private readonly Int32 _maxLength;

	/// <summary>
	/// Creates a <see cref="DatePattern" /> using a specified format string and culture-specific format information.
	/// </summary>
	/// <param name="format">
	/// A date format string,
	/// or either <see langword="null" /> or the empty string to use the standard format specifier "d".
	/// </param>
	/// <param name="provider">
	/// An <see cref="IFormatProvider" /> that supplies culture-specific format information,
	/// or <see langword="null" /> to use the current culture.
	/// </param>
	/// <returns>
	/// A <see cref="DatePattern" /> that represents the specified <paramref name="format" />.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// The <see cref="DateTimeFormatInfo.Calendar" /> of the specified <paramref name="provider" /> is not
	/// <see cref="GregorianCalendar" />.
	/// </exception>
	/// <exception cref="FormatException">
	/// <paramref name="format" /> is not a valid date format string.
	/// </exception>
	/// <remarks>
	/// <para>
	/// If <paramref name="format" /> is a single character, it is interpreted as a standard format specifier:
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Format specifier</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term><c>d</c></term>
	/// <description>Short date pattern defined by <see cref="DateTimeFormatInfo.ShortDatePattern" />.</description>
	/// </item>
	/// <item>
	/// <term><c>D</c></term>
	/// <description>Long date pattern defined by <see cref="DateTimeFormatInfo.LongDatePattern" />.</description>
	/// </item>
	/// <item>
	/// <term><c>M</c> -or- <c>m</c></term>
	/// <description>Month/day pattern defined by <see cref="DateTimeFormatInfo.MonthDayPattern" />.</description>
	/// </item>
	/// <item>
	/// <term><c>O</c> -or- <c>o</c></term>
	/// <description>Round-trip pattern ("yyyy-MM-dd") that complies with ISO 8601.</description>
	/// </item>
	/// <item>
	/// <term><c>R</c> -or- <c>r</c></term>
	/// <description>RFC 1123 pattern defined by <see cref="DateTimeFormatInfo.RFC1123Pattern" />.</description>
	/// </item>
	/// <item>
	/// <term><c>Y</c> -or- <c>y</c></term>
	/// <description>Year/month pattern defined by <see cref="DateTimeFormatInfo.YearMonthPattern" />.</description>
	/// </item>
	/// </list>
	/// <para>
	/// Any other single character results in a <see cref="FormatException" />.
	/// </para>
	/// <para>
	/// If <paramref name="format" /> consists of two or more characters, it is interpreted as a custom format string
	/// comprising one or more custom format specifiers:
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Format specifier</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term><c>d</c></term>
	/// <description>The day of the month, from 1 to 31.</description>
	/// </item>
	/// <item>
	/// <term><c>dd</c></term>
	/// <description>The day of the month, from 01 to 31.</description>
	/// </item>
	/// <item>
	/// <term><c>ddd</c></term>
	/// <description>The abbreviated name of the day of the week.</description>
	/// </item>
	/// <item>
	/// <term><c>dddd</c></term>
	/// <description>The full name of the day of the week.</description>
	/// </item>
	/// <item>
	/// <term><c>g</c> -or- <c>gg</c></term>
	/// <description>The name of the era.</description>
	/// </item>
	/// <item>
	/// <term><c>M</c></term>
	/// <description>The month, from 1 to 12.</description>
	/// </item>
	/// <item>
	/// <term><c>MM</c></term>
	/// <description>The month, from 01 to 12.</description>
	/// </item>
	/// <item>
	/// <term><c>MMM</c></term>
	/// <description>The abbreviated name of the month.</description>
	/// </item>
	/// <item>
	/// <term><c>MMMM</c></term>
	/// <description>The full name of the month.</description>
	/// </item>
	/// <item>
	/// <term><c>y</c></term>
	/// <description>The full year, from 1 to 9999.</description>
	/// </item>
	/// <item>
	/// <term><c>yy</c></term>
	/// <description>The last two digits of the year, from 00 to 99.</description>
	/// </item>
	/// <item>
	/// <term><c>yyyy</c></term>
	/// <description>The full year, from 0001 to 9999.</description>
	/// </item>
	/// <item>
	/// <term><c>/</c></term>
	/// <description>The date separator.</description>
	/// </item>
	/// <item>
	/// <term><c>'</c><var>string</var><c>'</c> or <c>"</c><var>string</var><c>"</c></term>
	/// <description>A literal string.</description>
	/// </item>
	/// <item>
	/// <term><c>\</c><var>character</var></term>
	/// <description>A literal character.</description>
	/// </item>
	/// <item>
	/// <term><c>%</c><var>character</var></term>
	/// <description>A single custom format specifier.</description>
	/// </item>
	/// <item>
	/// <term>Any other character besides an ASCII letter or percent sign</term>
	/// <description>A literal character.</description>
	/// </item>
	/// </list>
	/// <para>
	/// All literal ASCII letters and percent signs must be escaped or quoted.
	/// </para>
	/// <para>
	/// A single-character format string is interpreted as a standard format specifier. To use "d", "g", "M", or "y"
	/// as the only custom format specifier in the format string, prefix it with the percent ("%") format specifier,
	/// resulting in the format string "%d", "%g", "%M", or "%y". The percent format specifier cannot be used in any
	/// other format string.
	/// </para>
	/// </remarks>
	public static DatePattern Create(
#if NET7_0_OR_GREATER
		[StringSyntax(StringSyntaxAttribute.DateOnlyFormat)]
#endif
		String? format,
		IFormatProvider? provider = null)
	{
		if (String.IsNullOrEmpty(format))
			format = "d";
		else if (format.Length == 1)
		{
			switch (format[0] | 0x20) // lowercase
			{
				case 'o':
					return Iso8601;
				case 'r':
					return Rfc1123;
			}
		}

		DateTimeFormatInfo info = DateTimeFormatInfo.GetInstance(provider);
		if (info.Calendar is not GregorianCalendar)
			throw new ArgumentException($"{info.Calendar.GetType().Name} is not supported.", nameof(provider));

		if (format.Length == 1)
		{
			format =
				format switch
				{
					"d" => info.ShortDatePattern,
					"D" => info.LongDatePattern,
					"m" or "M" => info.MonthDayPattern,
					"y" or "Y" => info.YearMonthPattern,
					_ => throw new FormatException($"'{format}' is not a valid standard date format string."),
				};
		}

		List<Token> tokens = [];
		Boolean hasDayToken = false;
		DayNameToken? abbreviatedDayNameToken = null;
		DayNameToken? dayNameToken = null;
		MonthNameToken? abbreviatedMonthNameToken = null;
		MonthNameToken? monthNameToken = null;
		YearOfCenturyToken? yearOfCenturyToken = null;
		StringBuilder literalBuilder = new();
		Int32 charIndex = format.Length == 2 && format[0] == '%' ? 1 : 0;

		while ((UInt32)charIndex < (UInt32)format.Length)
		{
			Char c = format[charIndex];
			Int32 repeatCount;

			switch (c)
			{
				case 'd':
					AppendLiteral(tokens, literalBuilder);
					repeatCount = GetRepeatCount(format, charIndex, c, maxCount: 4);
					charIndex += repeatCount;

					switch (repeatCount)
					{
						case 1:
							hasDayToken = true;
							tokens.Add(DayToken.Instance);
							break;

						case 2:
							hasDayToken = true;
							tokens.Add(TwoDigitDayToken.Instance);
							break;

						case 3:
							tokens.Add(abbreviatedDayNameToken ??= new DayNameToken(info.AbbreviatedDayNames));
							break;

						case 4:
							tokens.Add(dayNameToken ??= new DayNameToken(info.DayNames));
							break;

						default:
							--charIndex;
							goto InvalidFormat; // too many d's
					}

					continue; // Don't increment charIndex again.

				case 'M':
					AppendLiteral(tokens, literalBuilder);
					repeatCount = GetRepeatCount(format, charIndex, c, maxCount: 4);
					charIndex += repeatCount;

					switch (repeatCount)
					{
						case 1:
							tokens.Add(MonthToken.Instance);
							break;

						case 2:
							tokens.Add(TwoDigitMonthToken.Instance);
							break;

						case 3:
							tokens.Add(abbreviatedMonthNameToken ??= new MonthNameToken());
							break;

						case 4:
							tokens.Add(monthNameToken ??= new MonthNameToken());
							break;

						default:
							--charIndex;
							goto InvalidFormat; // too many M's
					}

					continue; // Don't increment charIndex again.

				case 'y':
					AppendLiteral(tokens, literalBuilder);
					repeatCount = GetRepeatCount(format, charIndex, c, maxCount: 4);
					charIndex += repeatCount;

					switch (repeatCount)
					{
						case 1:
							tokens.Add(YearToken.Instance);
							break;

						case 2:
							tokens.Add(yearOfCenturyToken ??= new YearOfCenturyToken(info.Calendar.TwoDigitYearMax));
							break;

						case 4:
							tokens.Add(FourDigitYearToken.Instance);
							break;

						default:
							--charIndex;
							goto InvalidFormat; // three y's or too many y's
					}

					continue; // Don't increment charIndex again.

				case 'g':
					repeatCount = GetRepeatCount(format, charIndex, c, maxCount: 2);
					charIndex += repeatCount;

					if (repeatCount > 2)
					{
						--charIndex;
						goto InvalidFormat; // too many g's
					}

					literalBuilder.Append(info.GetEraName(GregorianCalendar.ADEra));
					continue; // Don't increment charIndex again.

				case '/':
					literalBuilder.Append(info.DateSeparator);
					break;

				case '\'':
				case '"':
				{
					Char quoteChar = c;

					while (true)
					{
						++charIndex;
						if ((UInt32)charIndex >= (UInt32)format.Length)
							goto InvalidFormat; // missing closing quote
						c = format[charIndex];
						if (c == quoteChar)
							break;

						if (c == '\\')
						{
							++charIndex;
							if ((UInt32)charIndex >= (UInt32)format.Length)
								goto InvalidFormat; // missing escaped character
							c = format[charIndex];
						}

						literalBuilder.Append(c);
					}

					break;
				}

				case '\\':
					++charIndex;
					if ((UInt32)charIndex >= (UInt32)format.Length)
						goto InvalidFormat; // missing escaped character
					literalBuilder.Append(format[charIndex]);
					break;

				default:
					if (unchecked((UInt32)((c | 0x20) - 'a')) <= 'z' - 'a' || c == '%')
						goto InvalidFormat; // invalid ASCII letter
					literalBuilder.Append(c);
					break;
			}

			++charIndex;
		}

		AppendLiteral(tokens, literalBuilder);

		if (abbreviatedMonthNameToken != null)
		{
			abbreviatedMonthNameToken.MonthNames = hasDayToken
				? info.AbbreviatedMonthGenitiveNames
				: info.AbbreviatedMonthNames;
		}

		if (monthNameToken != null)
		{
			monthNameToken.MonthNames = hasDayToken
				? info.MonthGenitiveNames
				: info.MonthNames;
		}

		return new DatePattern([..tokens]);

	InvalidFormat:
		throw new FormatException((UInt32)charIndex < (UInt32)format.Length
			? $"Unexpected character '{format[charIndex]}' at index {charIndex}."
			: "Unexpected end of string.");

		static void AppendLiteral(List<Token> tokens, StringBuilder literalBuilder)
		{
			if (literalBuilder.Length > 0)
			{
				tokens.Add(new LiteralToken(literalBuilder.ToString()));
				literalBuilder.Length = 0;
			}
		}

		static Int32 GetRepeatCount(String format, Int32 startIndex, Char c, Int32 maxCount)
		{
			Int32 charIndex = startIndex + 1;
			Int32 endIndex = Math.Min(charIndex + maxCount, format.Length);
			while (charIndex < endIndex && format[charIndex] == c)
				++charIndex;
			return charIndex - startIndex;
		}
	}

	private DatePattern(params Token[] tokens)
	{
		_tokens = tokens;
		_maxLength = tokens.Sum(token => token.MaxLength);
	}

	/// <summary>
	/// Converts a specified date to its equivalent string representation
	/// using the culture-specific format pattern represented by this instance.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> to format.
	/// </param>
	/// <returns>
	/// The string representation of <paramref name="date" />,
	/// or the empty string if <paramref name="date" /> is the empty <see cref="Date" />.
	/// </returns>
	public String Format(Date date)
	{
		return date != Date.Empty
			? FormatContext.Format(_tokens, _maxLength, date)
			: "";
	}

	/// <summary>
	/// Converts the string representation of a date to its equivalent <see cref="Date" />
	/// using the culture-specific format pattern represented by this instance.
	/// </summary>
	/// <param name="s">
	/// A string that contains the date to convert.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> that is equivalent to <paramref name="s" />.
	/// </returns>
	/// <exception cref="FormatException">
	/// <paramref name="s" /> does not contain a valid string representation of a date.
	/// </exception>
	public Date ParseExact(String s)
	{
		if (s == null)
			throw new ArgumentNullException(nameof(s));
		else if (ParseContext.TryParse(_tokens, s.AsSpan(), out Date date))
			return date;
		else
			throw new FormatException($"'{s}' is not a valid date string.");
	}

	/// <summary>
	/// Converts the string representation of a date to its equivalent <see cref="Date" />
	/// using the culture-specific format pattern represented by this instance.
	/// </summary>
	/// <param name="s">
	/// A string that contains the date to convert.
	/// </param>
	/// <param name="date">
	/// When this method returns, contains the <see cref="Date" /> equivalent to <paramref name="s" />
	/// if the conversion succeeded, or <see cref="Date.Empty" /> if the conversion failed.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="s" /> was converted successfully;
	/// otherwise, <see langword="false" />.
	/// </returns>
	public Boolean TryParseExact(String s, out Date date)
	{
		return ParseContext.TryParse(_tokens, s.AsSpan(), out date);
	}

	#region Tokens

	private abstract class Token
	{
		protected Token()
		{
		}

		protected Token(Int32 minLength, Int32 maxLength)
		{
			Debug.Assert(minLength >= 0);
			Debug.Assert(minLength <= maxLength);

			this.MinLength = minLength;
			this.MaxLength = maxLength;
		}

		public Int32 MinLength { get; protected set; }

		public Int32 MaxLength { get; protected set; }

		public abstract void Format(ref FormatContext context);

		public abstract Boolean TryParse(ref ParseContext context);

		protected static Dictionary<String, (String, Int32)[]> GetNamesByPrefix(String[] names, Int32 prefixLength)
		{
			return names
				.Where(name => name != "") // Ignore the 13th month.
				.Select((name, index) => (name, index))
				.ToLookup(
					tuple => tuple.name.Substring(0, prefixLength),
					tuple => (suffix: tuple.name.Substring(prefixLength), tuple.index + 1))
				.ToDictionary(
					grouping => grouping.Key,
					grouping => grouping.OrderByDescending(tuple => tuple.suffix.Length).ToArray());
		}
	}

	private class DayToken : Token
	{
		public static readonly DayToken Instance = new();

		private DayToken()
			: base(minLength: 1, maxLength: 2)
		{
		}

		public override void Format(ref FormatContext context)
		{
			context.FormatOneOrTwoDigitNumber(context.Date.Day);
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			return context.TryParseOneOrTwoDigitNumber(ref context.Day);
		}
	}

	private class TwoDigitDayToken : Token
	{
		public static readonly TwoDigitDayToken Instance = new();

		private TwoDigitDayToken()
			: base(minLength: 2, maxLength: 2)
		{
		}

		public override void Format(ref FormatContext context)
		{
			context.FormatTwoDigitNumber(context.Date.Day);
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			return context.TryParseTwoDigitNumber(ref context.Day);
		}
	}

	private class DayNameToken : Token
	{
		public static readonly DayNameToken InvariantAbbreviated =
			new(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames);

		private readonly String[] _dayNames;
		private Dictionary<String, (String, Int32)[]>? _dayNamesByPrefix;

		public DayNameToken(String[] dayNames)
		{
			_dayNames = dayNames;

			Int32 minLength = dayNames[0].Length;
			Int32 maxLength = minLength;

			for (Int32 index = 1; index < dayNames.Length; ++index)
			{
				String dayName = dayNames[index];
				if (dayName.Length < minLength)
					minLength = dayName.Length;
				else if (dayName.Length > maxLength)
					maxLength = dayName.Length;
			}

			this.MinLength = minLength;
			this.MaxLength = maxLength;
		}

		public override void Format(ref FormatContext context)
		{
			context.FormatString(_dayNames[(Int32)context.Date.DayOfWeek].AsSpan());
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			Int32 prefixLength = this.MinLength;
			return context.TryParseName(
				_dayNamesByPrefix ??= GetNamesByPrefix(_dayNames, prefixLength),
				prefixLength,
				ref context.DayOfWeek);
		}
	}

	private class MonthToken : Token
	{
		public static readonly MonthToken Instance = new();

		private MonthToken()
			: base(minLength: 1, maxLength: 2)
		{
		}

		public override void Format(ref FormatContext context)
		{
			context.FormatOneOrTwoDigitNumber(context.Date.Month);
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			return context.TryParseOneOrTwoDigitNumber(ref context.Month);
		}
	}

	private class TwoDigitMonthToken : Token
	{
		public static readonly TwoDigitMonthToken Instance = new();

		private TwoDigitMonthToken()
			: base(minLength: 2, maxLength: 2)
		{
		}

		public override void Format(ref FormatContext context)
		{
			context.FormatTwoDigitNumber(context.Date.Month);
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			return context.TryParseTwoDigitNumber(ref context.Month);
		}
	}

	private class MonthNameToken : Token
	{
		public static readonly MonthNameToken InvariantAbbreviated =
			new() { MonthNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames };

		private String[]? _monthNames;
		private Dictionary<String, (String, Int32)[]>? _monthNamesByPrefix;

		public String[] MonthNames
		{
			get => _monthNames!;
			set
			{
				Debug.Assert(value.Length == 13);

				_monthNames = value;

				Int32 minLength = value[0].Length;
				Int32 maxLength = minLength;

				for (Int32 index = 1; index < value.Length - 1; ++index) // Ignore the 13th month.
				{
					String monthName = value[index];
					if (monthName.Length < minLength)
						minLength = monthName.Length;
					else if (monthName.Length > maxLength)
						maxLength = monthName.Length;
				}

				this.MinLength = minLength;
				this.MaxLength = maxLength;
			}
		}

		public override void Format(ref FormatContext context)
		{
			context.FormatString(this.MonthNames[context.Date.Month - 1].AsSpan());
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			Int32 prefixLength = this.MinLength;
			return context.TryParseName(
				_monthNamesByPrefix ??= GetNamesByPrefix(_monthNames!, prefixLength),
				prefixLength,
				ref context.Month);
		}
	}

	private class YearToken : Token
	{
		public static readonly YearToken Instance = new();

		private YearToken()
			: base(minLength: 1, maxLength: 4)
		{
		}

		public override void Format(ref FormatContext context)
		{
			Int32 year = context.Date.Year;

			if (year >= 100)
			{
				Int32 century = Date.GetCentury(year);
				Int32 yearOfCentury = Date.GetYearOfCentury(year);
				context.FormatOneOrTwoDigitNumber(century);
				context.FormatTwoDigitNumber(yearOfCentury);
			}
			else
				context.FormatOneOrTwoDigitNumber(year);
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			return context.TryParseNumber(minDigits: 1, maxDigits: 4, ref context.Year);
		}
	}

	private class FourDigitYearToken : Token
	{
		public static readonly FourDigitYearToken Instance = new();

		private FourDigitYearToken()
			: base(minLength: 4, maxLength: 4)
		{
		}

		public override void Format(ref FormatContext context)
		{
			Int32 year = context.Date.Year;

			if (year >= 100)
			{
				Int32 century = Date.GetCentury(year);
				Int32 yearOfCentury = Date.GetYearOfCentury(year);
				context.FormatTwoDigitNumber(century);
				context.FormatTwoDigitNumber(yearOfCentury);
			}
			else
			{
				context.FormatString(['0', '0']);
				context.FormatTwoDigitNumber(year);
			}
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			return context.TryParseNumber(minDigits: 4, maxDigits: 4, ref context.Year);
		}
	}

	private class YearOfCenturyToken : Token
	{
		private readonly Int32 _maxCentury100;
		private readonly Int32 _maxYearOfCentury;

		public YearOfCenturyToken(Int32 twoDigitYearMax)
			: base(minLength: 2, maxLength: 2)
		{
			_maxCentury100 = Date.GetCentury(twoDigitYearMax) * 100;
			_maxYearOfCentury = Date.GetYearOfCentury(twoDigitYearMax);
		}

		public override void Format(ref FormatContext context)
		{
			context.FormatTwoDigitNumber(Date.GetYearOfCentury(context.Date.Year));
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			context.YearOfCenturyToken = this;
			return context.TryParseTwoDigitNumber(ref context.YearOfCentury);
		}

		public Int32 GetFullYear(Int32 yearOfCentury)
		{
			Int32 year = _maxCentury100 + yearOfCentury;
			if (yearOfCentury > _maxYearOfCentury)
				year -= 100;
			return year;
		}
	}

	private class LiteralToken : Token
	{
		public static readonly LiteralToken Hyphen = new("-");
		public static readonly LiteralToken Space = new(" ");

		private readonly String _literal;

		public LiteralToken(String literal)
			: base(minLength: literal.Length, maxLength: literal.Length)
		{
			_literal = literal;
		}

		public override void Format(ref FormatContext context)
		{
			context.FormatString(_literal.AsSpan());
		}

		public override Boolean TryParse(ref ParseContext context)
		{
			return context.TryParseLiteral(_literal.AsSpan());
		}
	}

	#endregion Tokens

	private ref struct FormatContext
	{
		// Input:
		public readonly Date Date;

		// Output:
		private Span<Char> _chars;

		public static String Format(Token[] tokens, Int32 maxLength, Date date)
		{
			Span<Char> buffer = stackalloc Char[maxLength];
			FormatContext context = new(date, buffer);
			foreach (Token token in tokens)
				token.Format(ref context);
			return buffer.Slice(0, buffer.Length - context._chars.Length).ToString();
		}

		private FormatContext(Date date, Span<Char> chars)
		{
			this.Date = date;
			_chars = chars;
		}

		public void FormatString(ReadOnlySpan<Char> value)
		{
			Span<Char> chars = _chars;
			value.CopyTo(chars);
			_chars = chars.Slice(value.Length);
		}

		public void FormatOneOrTwoDigitNumber(Int32 number)
		{
			if (number >= 10)
				this.FormatTwoDigitNumber(number);
			else
			{
				Debug.Assert(number >= 0);

				Span<Char> chars = _chars;
				chars[0] = (Char)(number + '0');
				_chars = chars.Slice(1);
			}
		}

		public void FormatTwoDigitNumber(Int32 number)
		{
			Debug.Assert(number >= 0);
			Debug.Assert(number < 100);

			const Int32 divisor = 10;
			// Unoptimized:
			//   Int32 tens = number / divisor;
			//   Int32 ones = number % divisor;
			// Optimized (valid for number in [0, 16383]; 10 is minimum shift count that encompasses [0, 99]):
			const Int32 shift = 16;
			const Int32 multiplier = (1 << shift) / divisor + 1;
			Int32 tens = number * multiplier >>> shift;
			Int32 ones = (Int32)((UInt32)number * multiplier % (1 << shift) * divisor >>> shift);

			Span<Char> chars = _chars;
			chars[1] = (Char)(ones + '0'); // Access index 1 first to elide bounds check on index 0.
			chars[0] = (Char)(tens + '0');
			_chars = chars.Slice(2);
		}
	}

	private ref struct ParseContext
	{
		// Input:
		private ReadOnlySpan<Char> _chars;

		// Output:
		public Int32 Year;
		public Int32 YearOfCentury;
		public Int32 Month;
		public Int32 Day;
		public Int32 DayOfWeek; // 1 = Sunday

		public YearOfCenturyToken? YearOfCenturyToken;

		public static Boolean TryParse(Token[] tokens, ReadOnlySpan<Char> chars, out Date date)
		{
			ParseContext context = new(chars);
			foreach (Token token in tokens)
				if (!token.TryParse(ref context))
					goto InvalidDate;

			Int32 year = context.Year;
			Int32 yearOfCentury = context.YearOfCentury;
			if (year >= 0)
			{
				// Unoptimized:
				//   if (year < Date.MinYear || year > Date.MaxYear)
				// Optimized (assuming Date.MinYear == 1 and Date.MaxYear == 9999):
				if (year == 0)
					goto InvalidDate;

				if (yearOfCentury >= 0 && yearOfCentury != Date.GetYearOfCentury(year))
					goto InvalidDate;
			}
			else if (yearOfCentury >= 0)
			{
				year = context.YearOfCenturyToken!.GetFullYear(yearOfCentury);
				if (year == 0)
					goto InvalidDate;
			}
			else
				year = 1;

			Int32 month = context.Month;
			if (month >= 0)
			{
				// Unoptimized:
				//   if (month < Date.January || month > Date.December)
				// Optimized:
				if (unchecked((UInt32)(month - Date.January)) > Date.December - Date.January)
					goto InvalidDate;
			}
			else
				month = 1;

			Int32 day = context.Day;
			if (day >= 0)
			{
				// Unoptimized:
				//   if (day < 1 || day > UnsafeDaysInMonth(year, month))
				// Optimized:
				if (unchecked((UInt32)(day - 1)) >= Date.UnsafeDaysInMonth(year, month))
					goto InvalidDate;
			}
			else
				day = 1;

			Int32 dayOfWeek = context.DayOfWeek;
			if (dayOfWeek > 0 && dayOfWeek != (Int32)Date.UnsafeDayOfWeek(year, month, day) + 1)
				goto InvalidDate;

			date = Date.UnsafeCreate(year, month, day);
			return true;

		InvalidDate:
			date = Date.Empty;
			return false;
		}

		private ParseContext(ReadOnlySpan<Char> chars)
		{
			_chars = chars;

			this.Year = -1;
			this.YearOfCentury = -1;
			this.Month = -1;
			this.Day = -1;
			this.DayOfWeek = -1;
		}

		public Boolean TryParseLiteral(ReadOnlySpan<Char> literal)
		{
			ReadOnlySpan<Char> chars = _chars;

			if ((UInt32)chars.Length >= (UInt32)literal.Length && chars.StartsWith(literal))
			{
				_chars = chars.Slice(literal.Length);
				return true;
			}
			else
				return false;
		}

		public Boolean TryParseName(
			Dictionary<String, (String, Int32)[]> namesByPrefix, Int32 prefixLength, ref Int32 fieldValue)
		{
			Debug.Assert(prefixLength >= 0);

			ReadOnlySpan<Char> chars = _chars;

			if ((UInt32)chars.Length >= (UInt32)prefixLength &&
				namesByPrefix.TryGetValue(chars.Slice(0, prefixLength).ToString(), out (String, Int32)[]? suffixes))
			{
				// TODO: In .NET 9, use GetAlternateLookup instead of ReadOnlySpan<Char>.ToString() above.
				chars = chars.Slice(prefixLength);

				foreach ((String suffix, Int32 value) in suffixes)
				{
					if ((UInt32)chars.Length >= (UInt32)suffix.Length && chars.StartsWith(suffix.AsSpan()))
					{
						_chars = chars.Slice(suffix.Length);
						return SetField(ref fieldValue, value);
					}
				}
			}

			return false;
		}

		public Boolean TryParseNumber(Int32 minDigits, Int32 maxDigits, ref Int32 fieldValue)
		{
			Debug.Assert(minDigits > 0);
			Debug.Assert(minDigits <= maxDigits);

			ReadOnlySpan<Char> chars = _chars;
			Int32 charIndex = 0;
			Int32 endIndex = Math.Min(maxDigits, chars.Length);
			Int32 number = 0;

			while (charIndex < endIndex)
			{
				Int32 digit = chars[charIndex] - '0';

				if (unchecked((UInt32)digit) <= 9)
				{
					number = number * 10 + digit;
					++charIndex;
				}
				else
					break;
			}

			if (charIndex >= minDigits)
			{
				_chars = chars.Slice(charIndex);
				return SetField(ref fieldValue, number);
			}
			else
				return false;
		}

		public Boolean TryParseOneOrTwoDigitNumber(ref Int32 fieldValue)
		{
			ReadOnlySpan<Char> chars = _chars;

			if (chars.Length >= 1 && chars[0] - '0' is Int32 digit0 && unchecked((UInt32)digit0) <= 9)
			{
				Int32 number = digit0;

				if (chars.Length >= 2 && chars[1] - '0' is Int32 digit1 && unchecked((UInt32)digit1) <= 9)
				{
					number = number * 10 + digit1;
					_chars = chars.Slice(2);
				}
				else
					_chars = chars.Slice(1);

				return SetField(ref fieldValue, number);
			}

			return false;
		}

		public Boolean TryParseTwoDigitNumber(ref Int32 fieldValue)
		{
			ReadOnlySpan<Char> chars = _chars;

			if (chars.Length >= 2 &&
				chars[0] - '0' is Int32 digit0 && unchecked((UInt32)digit0) <= 9 &&
				chars[1] - '0' is Int32 digit1 && unchecked((UInt32)digit1) <= 9)
			{
				_chars = chars.Slice(2);
				return SetField(ref fieldValue, digit0 * 10 + digit1);
			}

			return false;
		}

		private static Boolean SetField(ref Int32 fieldValue, Int32 newValue)
		{
			if (fieldValue < 0)
				fieldValue = newValue;
			else if (newValue != fieldValue)
				return false;
			return true;
		}
	}
}
