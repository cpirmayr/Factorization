using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorization
{
  public static class Extensions
  {
    public static double Median(this IEnumerable<double> source)
    {
      if (source == null || !source.Any())
        throw new InvalidOperationException("Die Sequenz enthält keine Elemente.");

      // 1. Daten sortieren
      var sortedList = source.Order().ToList();
      int count = sortedList.Count;
      int mid = count / 2;

      // 2. Median berechnen
      if (count % 2 == 0)
      {
        // Bei gerader Anzahl: Durchschnitt der beiden mittleren Werte
        return (sortedList[mid - 1] + sortedList[mid]) / 2.0;
      }
      else
      {
        // Bei ungerader Anzahl: Der exakte mittlere Wert
        return sortedList[mid];
      }
    }
  }
}
