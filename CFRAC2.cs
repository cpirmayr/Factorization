using System.Collections;
using System.Collections.Concurrent;
using System.Numerics;

// Für thread-safe Collections

namespace ConsoleApp2;

public class ContinuedFractionFactorizer2
{
  public ContinuedFractionFactorizer2(BigInteger targetNumber, int baseSize = 400)
  {
    targetCompositeNumber = targetNumber;
    factorBaseSize = baseSize;
    smoothRelationsBag = new ConcurrentBag<SmoothRelation>();
  }

  public List<int> factorBasePrimes = [];

  private readonly int factorBaseSize;

  // Thread-safe Collection für die Relationen während der parallelen Suche
  private readonly ConcurrentBag<SmoothRelation> smoothRelationsBag;
  private readonly BigInteger targetCompositeNumber;

  // Wir speichern das Ergebnis später hier sortiert für die Matrix
  private List<SmoothRelation> sortedSmoothRelations = [];

  public static BigInteger Factorize(BigInteger n)
  {
    ContinuedFractionFactorizer2 factorizer = new(n, CalculateOptimalFactorBaseSize(n));
    return factorizer.RunFactorization();
  }

  public BigInteger RunFactorization()
  {
    if (targetCompositeNumber < 2) return targetCompositeNumber;
    if (targetCompositeNumber.IsEven) return 2;
    if (IsPerfectSquare(targetCompositeNumber, out BigInteger root)) return root;

    // 1. Parallelisierte Faktorbasis-Generierung
    factorBasePrimes = GenerateFactorBaseParallel(factorBaseSize);

    // 2. Parallelisierte Suche nach Relationen (Batch-Processing)
    FindSmoothRelationsParallel();

    // Übertragen in Liste für deterministische Ordnung (wichtig für Matrix-Indizes)
    sortedSmoothRelations = smoothRelationsBag.ToList();

    // 3. Parallelisierte Gauß-Elimination
    return FindDependencyAndGcdParallel();
  }

  // --- Hilfsmethoden (unverändert oder minimal angepasst) ---

  private static BigInteger CalculateIntegerSquareRoot(BigInteger n)
  {
    if (n < 0) throw new ArgumentException("Keine Wurzel aus negativen Zahlen.");
    if (n == 0) return 0;
    BigInteger x = BigInteger.Pow(10, n.ToString().Length / 2 + 1);
    while (true)
    {
      BigInteger y = (x + n / x) >> 1;
      if (y >= x) return x;
      x = y;
    }
  }

  private static int CalculateOptimalFactorBaseSize(BigInteger n)
  {
    // 1. Natürlicher Logarithmus von n (ln n)
    double logN = BigInteger.Log(n);

    // 2. Logarithmus vom Logarithmus (ln ln n)
    double logLogN = Math.Log(logN);

    // 3. Die Hauptwurzel
    double root = Math.Sqrt(logN * logLogN);

    // 4. Skalierungsfaktor (Empirisch für Gauß-Elimination)
    // 0.5 wäre theoretisches Optimum für Block-Lanczos (viel schnellerer Solver).
    // 0.38 bis 0.42 ist ideal für normale Gauß-Elimination.
    const double factor = 0.4;

    // Ergebnis berechnen: e^(factor * root)
    double size = Math.Exp(factor * root);

    // Casting und Sicherheitsgrenzen
    int optimalSize = (int) size;

    // Safety Bounds (damit es bei sehr kleinen Zahlen nicht 0 wird)
    if (optimalSize < 200) return 200;

    // Optional: Obergrenze, falls RAM knapp ist (z.B. max 100.000)
    // if (optimalSize > 100000) return 100000;
    return optimalSize;
  }

  private static bool IsPerfectSquare(BigInteger n, out BigInteger root)
  {
    root = CalculateIntegerSquareRoot(n);
    return root * root == n;
  }

  private static bool IsSmallPrime(int n)
  {
    if (n < 2) return false;
    if (n == 2 || n == 3) return true;
    if (n % 2 == 0 || n % 3 == 0) return false;
    for (int i = 5; i * i <= n; i += 6)
      if (n % i == 0 || n % (i + 2) == 0)
        return false;
    return true;
  }

  /// <summary>
  ///   Parallelisierte Gauß-Elimination.
  /// </summary>
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

        // Parallelisierung der Matrix-Reduktion (XOR über alle anderen Zeilen)
        // Da jede Iteration r eine andere Zeile bearbeitet, ist das thread-safe ohne Locks.
        Parallel.For(0, rowCount, r =>
        {
          if (r != pivotRow && matrix[r][col])
          {
            matrix[r].Xor(matrix[pivotRow]);
            history[r].Xor(history[pivotRow]);
          }
        });
        pivotRow++;
      }
    }

    // Lösungssuche (bleibt sequenziell, da sehr schnell im Vergleich zur Elimination)
    for (int r = 0; r < rowCount; r++)
    {
      bool isRowAllZero = true;
      // Schnellerer Check als Schleife bei BitArray? Cast zu int[] möglich, aber hier simple Logik:
      for (int c = 0; c < colCount; c++)
      {
        if (matrix[r][c])
        {
          isRowAllZero = false;
          break;
        }
      }
      if (isRowAllZero)
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

  // --- Parallelisierte Kernlogik ---

  private void FindSmoothRelationsParallel()
  {
    BigInteger rootN = CalculateIntegerSquareRoot(targetCompositeNumber);
    BigInteger remainderTerm = 0;
    BigInteger denominatorTerm = 1;
    BigInteger expansionCoefficient = rootN;
    BigInteger prevNumerator = 1;
    BigInteger currentNumerator = expansionCoefficient;

    // Batch-Größe: Wie viele Kettenbruch-Schritte berechnen wir, 
    // bevor wir sie parallel prüfen? 
    // Ein höherer Wert reduziert Overhead, verbraucht aber mehr RAM.
    int batchSize = 2000;

    // Wir brauchen etwas mehr Relationen als die Basisgröße (z.B. +5% oder +20 Stück)
    int targetCount = factorBaseSize + 20;
    while (smoothRelationsBag.Count < targetCount)
    {
      // 1. Sequenziell: Kandidaten generieren (Mathematik ist rekursiv)
      List<(BigInteger Q, BigInteger X)> candidates = new(batchSize);
      for (int i = 0; i < batchSize; i++)
      {
        BigInteger quadraticResidue = currentNumerator * currentNumerator % targetCompositeNumber;

        // Optimierung: Negative Reste nutzen
        if (quadraticResidue > targetCompositeNumber / 2)
          quadraticResidue -= targetCompositeNumber;
        candidates.Add((quadraticResidue, currentNumerator));

        // --- Kettenbruch-Schritt (Sequenziell) ---
        remainderTerm = denominatorTerm * expansionCoefficient - remainderTerm;
        denominatorTerm = (targetCompositeNumber - remainderTerm * remainderTerm) / denominatorTerm;
        expansionCoefficient = (rootN + remainderTerm) / denominatorTerm;
        BigInteger nextNumerator = (expansionCoefficient * currentNumerator + prevNumerator) % targetCompositeNumber;
        prevNumerator = currentNumerator;
        currentNumerator = nextNumerator;
      }

      // 2. Parallel: Kandidaten prüfen (Teuerster Teil)
      Parallel.ForEach(candidates, candidate =>
      {
        // Wenn wir schon genug haben, brechen wir die teure Prüfung ab
        if (smoothRelationsBag.Count >= targetCount) return;
        if (IsNumberSmoothOverFactorBase(candidate.Q, out BitArray parity))
        {
          smoothRelationsBag.Add(new SmoothRelation
          {
            congruenceX = candidate.X,
            congruenceQ = candidate.Q,
            exponentParityVector = parity
          });
        }
      });
    }
  }

  /// <summary>
  ///   Nutzt PLINQ (Parallel LINQ) um die Basis schneller zu finden.
  /// </summary>
  private List<int> GenerateFactorBaseParallel(int size)
  {
    List<int> primes = [-1, 2];

    // Wir suchen parallel nach Kandidaten. 
    // Wir schätzen grob ab, wie weit wir suchen müssen (n log n).
    // Hier einfach in Chunks.
    int searchStart = 3;
    int chunkSize = size * 10;
    while (primes.Count < size)
    {
      List<int> foundPrimes = Enumerable.Range(0, chunkSize)
        .Select(i => searchStart + i * 2) // Nur ungerade Zahlen
        .AsParallel() // Parallelisierung hier!
        .AsOrdered() // Reihenfolge beibehalten ist wichtig für deterministisches Verhalten
        .Where(n => IsSmallPrime(n))
        .Where(p => BigInteger.ModPow(targetCompositeNumber, (p - 1) / 2, p) == 1)
        .Take(size - primes.Count) // Nicht mehr nehmen als nötig
        .ToList();
      primes.AddRange(foundPrimes);
      searchStart += chunkSize * 2;
    }
    return primes.Take(size).ToList();
  }

  // Diese Methode wird jetzt von vielen Threads gleichzeitig aufgerufen.
  // Da factorBasePrimes nur gelesen wird, ist das thread-safe.
  private bool IsNumberSmoothOverFactorBase(BigInteger residue, out BitArray exponentParity)
  {
    exponentParity = new BitArray(factorBasePrimes.Count);
    BigInteger tempResidue = BigInteger.Abs(residue);
    if (residue < 0) exponentParity[0] = true;
    for (int i = 1; i < factorBasePrimes.Count; i++)
    {
      int prime = factorBasePrimes[i];

      // Kleine Optimierung: Schneller Modulo Check bevor Division
      while (tempResidue > 0)
      {
        BigInteger remainder;
        BigInteger quotient = BigInteger.DivRem(tempResidue, prime, out remainder);
        if (remainder == 0)
        {
          exponentParity[i] = !exponentParity[i];
          tempResidue = quotient;
        }
        else
        {
          break;
        }
      }
      if (tempResidue == 1) return true;
    }
    return tempResidue == 1;
  }

  private struct SmoothRelation
  {
    public BigInteger congruenceQ;
    public BigInteger congruenceX;
    public BitArray exponentParityVector;
  }
}