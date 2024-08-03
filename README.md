# DateKit

DateKit offers a simple and efficient date-only type named `Date` for .NET Standard 2.0, .NET Framework 4.5, and .NET 8.

## Quick Reference

The DateKit `Date` type provides the following members for working with `Date` instances:

```csharp
using DateKit;

// Constructor and deconstructor:
Date date = new Date(1999, 12, 31); // construct from a year, month, and day
(int year, int month, int day) = date; // C# 7.0 and later

// Properties:
int year = date.Year; // 1 to 9999
int month = date.Month; // 1 to 12
int day = date.Day; // 1 to 31
int dayNumber = date.DayNumber; // 0 to 3652058
DayOfWeek dayOfWeek = date.DayOfWeek; // Sunday to Saturday
int dayOfYear = date.DayOfYear; // 1 to 366

// Conversion methods:
Date date = Date.FromDateOnly(dateOnly); // convert from a DateOnly (.NET 6 and later)
DateOnly dateOnly = date.ToDateOnly(); // convert to a DateOnly (.NET 6 and later)
Date date = Date.FromDateTime(dateTime); // convert from the date component of a DateTime
DateTime dateTime = date.ToDateTime(); // convert to a DateTime with a time component of midnight
Date date = Date.FromDayNumber(730118); // convert from a number of days since January 1, 0001

// Comparison methods and operators:
bool areEqual = date1 == date2; // or Date.Equals(date1, date2)
bool areUnequal = date1 != date2;
bool isLessThan = date1 < date2; // or Date.Compare(date1, date2) < 0
bool isGreaterThan = date1 > date2; // or Date.Compare(date1, date2) > 0
// ... and similarly for <= and >=

// Arithmetic methods and operators:
date = date.AddYears(1);
date = date.AddMonths(2);
date = date.AddDays(3);
--date; // subtract one day
++date; // add one day
int daysBetween = endDate - startDate; // or Date.Subtract(endDate, startDate)

// Formatting and parsing methods:
string s = date.ToIsoString(); // format using "yyyy-MM-dd"
string s = date.ToString(); // format using "d" and the current culture
string s = date.ToString(format, provider);
Date date = Date.ParseIsoString(s); // parse using "yyyy-MM-dd"

DatePattern pattern = DatePattern.Create(format, provider); // create a reusable format pattern
string s = pattern.Format(date); // format more efficiently than ToString(format, provider)
Date date = pattern.ParseExact(s);
bool succeeded = pattern.TryParseExact(s, out Date date);
```

In addition, the `Date` type provides the following constants and static methods pertaining to the Gregorian calendar:

```csharp
const int MonthsPerYear = 12;
const int DaysPerYear = 365; // days in a common (non-leap) year
const int DaysPerWeek = 7;

static int DaysInMonth(int year, int month); // same as DateTime.DaysInMonth
static bool IsLeapYear(int year); // same as DateTime.IsLeapYear
```

## Empty Date

The `default(Date)` instance represents the “empty date” instead of an actual date, and can be used as a sentinel value to indicate the absence of a date. The empty date behaves as follows:

* The `Year`, `Month`, and `Day` properties return 0. (These properties never return 0 for an actual date.)
* The empty date compares equal to itself, greater than `null`, and less than `Date.MinValue`. (The last behavior differs from that of `DateTime`, whose default instance equals `DateTime.MinValue`, or January 1, 0001.)
* The `ToIsoString` and `ToString` methods return the empty string.
* Except for `GetHashCode`, all other properties and methods throw
`InvalidOperationException`.

## Performance

The .NET `DateOnly` and `DateTime` types internally store a number of days or ticks since the `MinValue` epoch of January 1, 0001, so their `Year`, `Month`, and `Day` properties must perform some complex calculations every time they are accessed. In contrast, the DateKit `Date` type internally stores the `Year`, `Month`, and `Day` as separate fields, making these properties cheap to access. The tradeoff is that calculations involving intervals of days are slower.

The following table compares the performance of `Date` versus `DateOnly` and `DateTime`:

| Member | Is `Date` faster or slower than `DateOnly`/`DateTime`? |
| ------------------------------ | ------ |
| Constructor (year, month, day) | Faster |
| FromDayNumber                  | Slower |
| Year, Month, Day               | Faster |
| DayNumber                      | Slower |
| DayOfWeek                      | Slower |
| DayOfYear                      | Faster |
| Compare                        | Tie    |
| Equals                         | Tie    |
| AddYears                       | Faster |
| AddMonths                      | Faster |
| AddDays                        | Slower |
| Subtract                       | Slower |

## Limitations

`Date` supports the Gregorian calendar only.

`Date` currently does not provide a non-exact `Parse` method (due to the complexity of parsing free-form dates). As an alternative, call `DateOnly.Parse` or `DateTime.Parse`, and pass the result to `Date.FromDateOnly` or `Date.FromDateTime`.

`DatePattern.TryParseExact` requires that the input string be an exact ordinal, case-sensitive match with the output string of `DatePattern.Format`. (Unfortunately, the .NET `DateTimeFormatInfo` class currently does not publicly expose its `CompareInfo` instance, which is needed to perform culture-sensitive, case-insensitive string comparisons.)

## License

DateKit is licensed under the [MIT](LICENSE.md) license.
