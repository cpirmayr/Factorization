using System.Collections;
using System.Collections.Concurrent;
using System.Numerics;

namespace Factorization;

/// <summary>
///   Implementierung der Kettenbruchmethode (Continued Fraction Factorization Method, CFRAC).
///   Mathematisches Prinzip:
///   Das Ziel ist das Finden einer Kongruenz der Form $x^2 \equiv y^2 \pmod{n}$.
///   Wenn diese Bedingung erfüllt ist, lässt sich ein Teiler von n durch den
///   größten gemeinsamen Teiler von $(x - y)$ und $n$ berechnen.
/// </summary>
public class CFRAC
{
  /// <summary>
  ///   Initialisiert eine neue Instanz der <see cref="CFRAC" />-Klasse.
  /// </summary>
  /// <param name="targetNumber">Die zusammengesetzte Zahl, die faktorisiert werden soll.</param>
  /// <param name="baseSize">Die gewünschte Größe der Primzahl-Faktorbasis.</param>
  public CFRAC(BigInteger targetNumber, int baseSize = 400)
  {
    targetCompositeNumber = targetNumber;
    factorBaseSize = baseSize;
    smoothRelationsBag = new ConcurrentBag<SmoothRelation>();
  }

  /// <summary>
  ///   Die Liste der Primzahlen p, für die das Legendre-Symbol (n/p) = 1 gilt.
  ///   Diese bilden die Grundlage für die Prüfung auf Glattheit.
  /// </summary>
  public List<int> factorBasePrimes = [];

  /// <summary>
  ///   Die Anzahl der Primzahlen, die in der Faktorbasis enthalten sein sollen.
  /// </summary>
  private readonly int factorBaseSize;

  /// <summary>
  ///   Ein thread-sicherer Speicher für alle gefundenen glatten Relationen während der Siebphase.
  /// </summary>
  private readonly ConcurrentBag<SmoothRelation> smoothRelationsBag;

  /// <summary>
  ///   Die zu faktorisierende Zahl n.
  /// </summary>
  private readonly BigInteger targetCompositeNumber;

  /// <summary>
  ///   Eine sortierte Liste der gefundenen Relationen für den Aufbau der Matrix in der linearen Algebra-Phase.
  /// </summary>
  private List<SmoothRelation> sortedSmoothRelations = [];

  /// <summary>
  ///   Statische Einstiegsmethode zur Faktorisierung einer Zahl.
  ///   Berechnet automatisch eine sinnvolle Basisgröße basierend auf der Größe von n.
  /// </summary>
  /// <param name="n">Die zu faktorisierende Zahl.</param>
  /// <returns>Einen nicht-trivialen Faktor von n oder -1, falls kein Faktor gefunden wurde.</returns>
  public static BigInteger Factorize(BigInteger n)
  {
    CFRAC factorizer = new(n, CalculateOptimalFactorBaseSize(n) / 10);
    return factorizer.RunFactorization();
  }

  /// <summary>
  ///   Führt den vollständigen CFRAC-Algorithmus aus: Vorprüfung, Basisgenerierung, Relationssuche und Matrixlösung.
  /// </summary>
  /// <returns>Einen gefundenen Faktor von n.</returns>
  public BigInteger RunFactorization()
  {
    if (targetCompositeNumber < 2)
    {
      return targetCompositeNumber;
    }
    if (targetCompositeNumber.IsEven)
    {
      return 2;
    }
    if (IsPerfectSquare(targetCompositeNumber, out BigInteger root))
    {
      return root;
    }

    // 1. Schritt: Faktorbasis bestimmen
    factorBasePrimes = GenerateFactorBaseParallel(factorBaseSize);

    // 2. Schritt: Glatte Relationen sammeln
    FindSmoothRelationsParallel();
    sortedSmoothRelations = smoothRelationsBag.ToList();

    // 3. Schritt: Lineare Abhängigkeit lösen
    return FindDependencyAndGcdParallel();
  }

  /// <summary>
  ///   Berechnet die ganzzahlige Quadratwurzel einer BigInteger-Zahl mittels Newton-Verfahren.
  /// </summary>
  /// <param name="n">Die Zahl, deren Wurzel berechnet werden soll.</param>
  /// <returns>Die abgerundete ganzzahlige Quadratwurzel.</returns>
  /// <exception cref="ArgumentException">Wird geworfen, wenn n negativ ist.</exception>
  private static BigInteger CalculateIntegerSquareRoot(BigInteger n)
  {
    if (n < 0)
    {
      throw new ArgumentException("Negative Wurzel.");
    }
    if (n == 0)
    {
      return 0;
    }
    BigInteger x = BigInteger.Pow(10, n.ToString().Length / 2 + 1);
    while (true)
    {
      BigInteger y = (x + n / x) >> 1;
      if (y >= x)
      {
        return x;
      }
      x = y;
    }
  }

  /// <summary>
  ///   Schätzt die optimale Größe der Faktorbasis basierend auf der Komplexitätsfunktion L[n].
  /// </summary>
  /// <param name="n">Die zu faktorisierende Zahl.</param>
  /// <returns>Die empfohlene Anzahl an Primzahlen für die Basis.</returns>
  private static int CalculateOptimalFactorBaseSize(BigInteger n)
  {
    double l = BigInteger.Log(n);
    return (int) Math.Max(Math.Exp(0.4 * Math.Sqrt(l * Math.Log(l))), 200);
  }

  /// <summary>
  ///   Prüft, ob eine Zahl eine Quadratzahl ist.
  /// </summary>
  /// <param name="n">Die zu prüfende Zahl.</param>
  /// <param name="root">Die berechnete Wurzel, falls n eine Quadratzahl ist.</param>
  /// <returns>True, wenn n eine perfekte Quadratzahl ist, andernfalls False.</returns>
  private static bool IsPerfectSquare(BigInteger n, out BigInteger root)
  {
    root = CalculateIntegerSquareRoot(n);
    return root * root == n;
  }

  /// <summary>
  ///   Einfacher Primalitätstest für kleine Integer-Zahlen.
  /// </summary>
  /// <param name="n">Die zu prüfende Zahl.</param>
  /// <returns>True, wenn die Zahl prim ist.</returns>
  private static bool IsSmallPrime(int n)
  {
    if (n < 2)
    {
      return false;
    }
    for (int i = 2; i * i <= n; i++)
    {
      if (n % i == 0)
      {
        return false;
      }
    }
    return true;
  }

  /// <summary>
  ///   Prüft, ob eine Zeile in einer BitMatrix nur aus Nullen besteht.
  /// </summary>
  /// <param name="row">Das Bit-Array der Zeile.</param>
  /// <param name="len">Die zu prüfende Länge (Anzahl der Spalten).</param>
  /// <returns>True, wenn alle Bits bis zur Länge len Null sind.</returns>
  private static bool IsZeroRow(BitArray row, int len)
  {
    for (int i = 0; i < len; i++)
    {
      if (row[i])
      {
        return false;
      }
    }
    return true;
  }

  /// <summary>
  ///   Löst das Gleichungssystem über dem Körper GF(2) mittels Gauß-Elimination.
  ///   Findet eine lineare Abhängigkeit der Exponentenvektoren, um ein Quadrat modulo n zu erzeugen.
  /// </summary>
  /// <returns>Einen nicht-trivialen Teiler von n oder -1.</returns>
  private BigInteger FindDependencyAndGcdParallel()
  {
    int rowCount = sortedSmoothRelations.Count;
    int colCount = factorBasePrimes.Count;
    BitArray[] matrix = sortedSmoothRelations.Select(r => new BitArray(r.exponentParityVector)).ToArray();
    BitArray[] history = new BitArray[rowCount];
    for (int i = 0; i < rowCount; i++)
    {
      history[i] = new BitArray(rowCount);
      history[i].Set(i, true);
    }
    int pivotRow = 0;
    for (int col = 0; col < colCount && pivotRow < rowCount; col++)
    {
      int selectedRow = -1;
      for (int r = pivotRow; r < rowCount; r++)
      {
        if (matrix[r][col])
        {
          selectedRow = r;
          break;
        }
      }
      if (selectedRow != -1)
      {
        (matrix[selectedRow], matrix[pivotRow]) = (matrix[pivotRow], matrix[selectedRow]);
        (history[selectedRow], history[pivotRow]) = (history[pivotRow], history[selectedRow]);
        int currentPivot = pivotRow;
        int currentCol = col;
        Parallel.For(0, rowCount, r =>
        {
          if (r != currentPivot && matrix[r][currentCol])
          {
            matrix[r].Xor(matrix[currentPivot]);
            history[r].Xor(history[currentPivot]);
          }
        });
        pivotRow++;
      }
    }
    for (int r = 0; r < rowCount; r++)
    {
      if (IsZeroRow(matrix[r], colCount))
      {
        BigInteger x = 1;
        BigInteger ySquared = 1;
        for (int i = 0; i < rowCount; i++)
        {
          if (history[r][i])
          {
            x = x * sortedSmoothRelations[i].congruenceX % targetCompositeNumber;
            ySquared *= sortedSmoothRelations[i].congruenceQ;
          }
        }
        BigInteger y = CalculateIntegerSquareRoot(ySquared);
        BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(x - y), targetCompositeNumber);
        if (gcd > 1 && gcd < targetCompositeNumber)
        {
          return gcd;
        }
      }
    }
    return -1;
  }

  /// <summary>
  ///   Erzeugt Kandidaten für die Kongruenz mittels Kettenbruchentwicklung von Wurzel(n).
  ///   Prüft diese Kandidaten parallel auf ihre "Glattheit" bezüglich der Faktorbasis.
  /// </summary>
  private void FindSmoothRelationsParallel()
  {
    BigInteger rootN = CalculateIntegerSquareRoot(targetCompositeNumber);
    BigInteger pValue = 0;
    BigInteger qValue = 1;
    BigInteger expansionCoefficient = rootN;
    BigInteger aPrevious = 1;
    BigInteger aCurrent = expansionCoefficient;
    int targetCount = factorBaseSize + 20;
    while (smoothRelationsBag.Count < targetCount)
    {
      List<(BigInteger Residue, BigInteger Numerator)> candidates = new(2000);
      for (int i = 0; i < 2000; i++)
      {
        BigInteger residue = aCurrent * aCurrent % targetCompositeNumber;
        if (residue > targetCompositeNumber / 2)
        {
          residue -= targetCompositeNumber;
        }
        candidates.Add((residue, aCurrent));
        pValue = expansionCoefficient * qValue - pValue;
        qValue = (targetCompositeNumber - pValue * pValue) / qValue;
        expansionCoefficient = (rootN + pValue) / qValue;
        BigInteger aNext = (expansionCoefficient * aCurrent + aPrevious) % targetCompositeNumber;
        aPrevious = aCurrent;
        aCurrent = aNext;
      }
      Parallel.ForEach(candidates, c =>
      {
        if (smoothRelationsBag.Count >= targetCount)
        {
          return;
        }
        if (IsNumberSmoothOverFactorBase(c.Residue, out BitArray parity))
        {
          smoothRelationsBag.Add(new SmoothRelation
          {
            congruenceX = c.Numerator,
            congruenceQ = c.Residue,
            exponentParityVector = parity
          });
        }
      });
    }
  }

  /// <summary>
  ///   Generiert die Faktorbasis bestehend aus -1, 2 und weiteren kleinen Primzahlen,
  ///   für die n ein quadratischer Rest ist (Legendre-Symbol = 1).
  /// </summary>
  /// <param name="size">Die Anzahl der zu findenden Primzahlen.</param>
  /// <returns>Eine Liste von Primzahlen, die die Faktorbasis bilden.</returns>
  private List<int> GenerateFactorBaseParallel(int size)
  {
    List<int> primes = [-1, 2];
    int start = 3;
    int chunk = size * 10;
    while (primes.Count < size)
    {
      List<int> found = Enumerable.Range(0, chunk)
        .Select(i => start + i * 2)
        .AsParallel()
        .Where(IsSmallPrime)
        .Where(p => BigInteger.ModPow(targetCompositeNumber, (p - 1) / 2, p) == 1)
        .Take(size - primes.Count)
        .ToList();
      primes.AddRange(found);
      start += chunk * 2;
    }
    return primes.Take(size).ToList();
  }

  /// <summary>
  ///   Prüft, ob eine Zahl (Residue) vollständig in Primfaktoren aus der Faktorbasis zerlegbar ist.
  /// </summary>
  /// <param name="residue">Der zu prüfende Rest Q.</param>
  /// <param name="exponentParity">
  ///   Ein Bit-Vektor, der angibt, ob die Exponenten der Primfaktoren gerade (0) oder ungerade
  ///   (1) sind.
  /// </param>
  /// <returns>True, wenn die Zahl über der Faktorbasis glatt ist.</returns>
  private bool IsNumberSmoothOverFactorBase(BigInteger residue, out BitArray exponentParity)
  {
    exponentParity = new BitArray(factorBasePrimes.Count);
    BigInteger temp = BigInteger.Abs(residue);
    if (residue < 0)
    {
      exponentParity[0] = true;
    }
    for (int i = 1; i < factorBasePrimes.Count; i++)
    {
      int p = factorBasePrimes[i];
      while (temp > 0)
      {
        BigInteger quotient = BigInteger.DivRem(temp, p, out BigInteger rem);
        if (rem == 0)
        {
          exponentParity[i] = !exponentParity[i];
          temp = quotient;
        }
        else
        {
          break;
        }
      }
      if (temp == 1)
      {
        return true;
      }
    }
    return temp == 1;
  }

  /// <summary>
  ///   Repräsentiert eine "glatte Relation".
  ///   Speichert den Zähler A des Näherungsbruchs und den zugehörigen glatten Rest Q sowie dessen Exponenten-Parität.
  /// </summary>
  private struct SmoothRelation
  {
    /// <summary>
    ///   Der glatte Rest Q, der aus der Kettenbruchentwicklung resultiert.
    /// </summary>
    public BigInteger congruenceQ;

    /// <summary>
    ///   Der Zähler A des Näherungsbruchs (x), für den gilt: x² ≡ Q (mod n).
    /// </summary>
    public BigInteger congruenceX;

    /// <summary>
    ///   Ein Bit-Vektor, der die Parität der Exponenten der Primfaktorzerlegung von Q speichert.
    /// </summary>
    public BitArray exponentParityVector;
  }
}