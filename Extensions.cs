using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Factorization
{
  public static class Extensions
  {
    public static T Median<T>(this IEnumerable<T> source) where T : INumber<T>
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var sortedList = source.Order().ToList();

        if (sortedList.Count == 0)
            throw new InvalidOperationException("Die Sequenz enthält keine Elemente.");

        int count = sortedList.Count;
        int mid = count / 2;

        if (count % 2 == 0)
        {
            // Durchschnitt der beiden mittleren Werte
            return (sortedList[mid - 1] + sortedList[mid]) / T.CreateChecked(2);
        }
        else
        {
            return sortedList[mid];
        }
    }

    public static string ToPlain(this double value) => value.ToString("0.###############################", CultureInfo.InvariantCulture);  }
}
