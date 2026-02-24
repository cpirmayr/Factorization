using System.Globalization;
using System.Numerics;

namespace Factorization;

internal static class Extensions
{
  public static T Median<T>(this IEnumerable<T> source) where T : INumber<T>
  {
    ArgumentNullException.ThrowIfNull(source);
    List<T> sortedList = source.Order().ToList();
    if (sortedList.Count == 0)
    {
      throw new InvalidOperationException("Die Sequenz enthält keine Elemente.");
    }
    int count = sortedList.Count;
    int mid = count / 2;
    if (count % 2 == 0) // Durchschnitt der beiden mittleren Werte ...
    {
      return (sortedList[mid - 1] + sortedList[mid]) / T.CreateChecked(2);
    }
    return sortedList[mid];
  }

  public static string ToPlain(this double value)
  {
    return value.ToString("0.###############################", CultureInfo.InvariantCulture);
  }
}