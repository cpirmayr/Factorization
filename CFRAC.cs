using System.Collections;
using System.Collections.Concurrent;
using System.Numerics;

namespace Factorization;

/// <summary>
///   IMPLEMENTIERUNG DER KETTENBRUCHMETHODE (CFRAC)
///   MATHEMATISCHES PRINZIP:
///   Das Ziel ist das Finden einer Kongruenz der Form $x^2 \equiv y^2 \pmod{n}$.
///   Wenn diese Bedingung erfüllt ist, lässt sich ein Teiler von n durch den
///   größten gemeinsamen Teiler von $(x - y)$ und $n$ berechnen.
///   DIE ROLLE DER KETTENBRÜCHE:
///   Die Kettenbruchentwicklung von $\sqrt{n}$ liefert Näherungsbrüche $A/B$.
///   Für diese Brüche gilt die Identität: $A_i^2 - n \cdot B_i^2 = Q_i$.
///   Daraus folgt: $A_i^2 \equiv Q_i \pmod{n}$.
///   Da der Betrag von $Q_i$ kleiner als $(2 \cdot \sqrt{n})$ ist, ist die
///   Wahrscheinlichkeit hoch, dass $Q$ glatt über einer Faktorbasis ist.
/// </summary>
public class CFRAC
{
  public CFRAC(BigInteger targetNumber, int baseSize = 400)
  {
    targetCompositeNumber = targetNumber;
    factorBaseSize = baseSize;
    smoothRelationsBag = new ConcurrentBag<SmoothRelation>();
  }

  public List<int> factorBasePrimes = []; // Primzahlen p, für die (n/p) = 1 gilt
  private readonly int factorBaseSize; // Anzahl der Primzahlen in der Basis

  private readonly ConcurrentBag<SmoothRelation> smoothRelationsBag; // Speicher für gefundene Relationen

  // --- Klassenfelder (Zustand des Algorithmus) ---
  private readonly BigInteger targetCompositeNumber; // Die zu faktorisierende Zahl n
  private List<SmoothRelation> sortedSmoothRelations = []; // Sortierte Liste für Matrixaufbau

  /// <summary> Statische Einstiegsmethode zur Faktorisierung. </summary>
  public static BigInteger Factorize(BigInteger n)
  {
    CFRAC factorizer = new(n, CalculateOptimalFactorBaseSize(n) / 10);
    return factorizer.RunFactorization();
  }

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

  private static int CalculateOptimalFactorBaseSize(BigInteger n)
  {
    double l = BigInteger.Log(n);
    return (int) Math.Max(Math.Exp(0.4 * Math.Sqrt(l * Math.Log(l))), 200);
  }

  private static bool IsPerfectSquare(BigInteger n, out BigInteger root)
  {
    root = CalculateIntegerSquareRoot(n);
    return root * root == n;
  }

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

  /// <summary> Löst das Gleichungssystem über GF(2) mittels Gauß-Elimination. </summary>
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
        int currentPivot = pivotRow; // Lokale Kopie für thread-safe Capture
        int currentCol = col; // Lokale Kopie für thread-safe Capture
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

  /// <summary> Erzeugt Kandidaten mittels Kettenbruch und prüft diese parallel. </summary>
  private void FindSmoothRelationsParallel()
  {
    // --- Variablen der Kettenbruchentwicklung ---
    BigInteger rootN = CalculateIntegerSquareRoot(targetCompositeNumber); // floor(Wurzel(n))
    BigInteger pValue = 0; // Rekursionswert P (Teil des Nenners)
    BigInteger qValue = 1; // Rekursionswert Q (Rest-Kandidat)
    BigInteger expansionCoefficient = rootN; // Koeffizient q (ganzzahliger Anteil)
    BigInteger aPrevious = 1; // Vorletzter Zähler des Näherungsbruchs
    BigInteger aCurrent = expansionCoefficient; // Aktueller Zähler des Näherungsbruchs
    int targetCount = factorBaseSize + 20; // Benötigte Anzahl an Relationen
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

        // Rekursion nach Morrison und Brillhart
        pValue = expansionCoefficient * qValue - pValue;
        qValue = (targetCompositeNumber - pValue * pValue) / qValue;
        expansionCoefficient = (rootN + pValue) / qValue;
        BigInteger aNext = (expansionCoefficient * aCurrent + aPrevious) % targetCompositeNumber;
        aPrevious = aCurrent;
        aCurrent = aNext;
      }

      // Parallele Prüfung der Kandidaten auf Glattheit
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
        .Where(IsSmallPrime) // Methodengruppe
        .Where(p => BigInteger.ModPow(targetCompositeNumber, (p - 1) / 2, p) == 1)
        .Take(size - primes.Count)
        .ToList();
      primes.AddRange(found);
      start += chunk * 2;
    }
    return primes.Take(size).ToList();
  }

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

  private struct SmoothRelation
  {
    public BigInteger congruenceQ; // Der glatte Rest Q
    public BigInteger congruenceX; // Der Zähler A
    public BitArray exponentParityVector; // Exponenten-Parität modulo 2
  }
}