using System.Collections;
using System.Numerics;

namespace ConsoleApp2;

/// <summary>
///   Implementierung der Kettenbruch-Faktorisierung (Continued Fraction Factorization).
///   Dieser Algorithmus versucht, eine große zusammengesetzte Zahl in zwei Faktoren zu zerlegen.
///   Er basiert auf der Suche nach Kongruenzen der Form x^2 ≡ y^2 (mod n).
/// </summary>
public class ContinuedFractionFactorizer
{
  public ContinuedFractionFactorizer(BigInteger targetNumber, int baseSize = 400)
  {
    targetCompositeNumber = targetNumber;
    factorBaseSize = baseSize;
    smoothRelations = [];
  }

  // Die Faktorbasis: Eine Liste kleiner Primzahlen (-1, 2, 3, 5, ...),
  // die als Teiler für die Reste (q) verwendet werden.
  public List<int> factorBasePrimes = [];

  // Die Größe der Faktorbasis (Anzahl der verwendeten kleinen Primzahlen).
  // Eine größere Basis erhöht die Wahrscheinlichkeit, "glatte" Zahlen zu finden,
  // vergrößert aber auch die Matrix.
  private readonly int factorBaseSize;

  // Liste der gefundenen Relationen (x, q, Exponentenvektor).
  // Eine Relation ist gefunden, wenn q in x^2 ≡ q (mod n) "B-glatt" ist.
  private readonly List<SmoothRelation> smoothRelations;

  // Die Zahl, die wir faktorisieren wollen (das 'n').
  private readonly BigInteger targetCompositeNumber;

  /// <summary>
  ///   Statische Einstiegsmethode zur Bequemlichkeit.
  /// </summary>
  public static BigInteger Factorize(BigInteger n)
  {
    ContinuedFractionFactorizer factorizer = new(n);
    return factorizer.RunFactorization();
  }

  /// <summary>
  ///   Führt den Hauptalgorithmus aus.
  /// </summary>
  public BigInteger RunFactorization()
  {
    // 1. Triviale Fälle abfangen
    if (targetCompositeNumber < 2) return targetCompositeNumber;
    if (targetCompositeNumber.IsEven) return 2;
    if (IsPerfectSquare(targetCompositeNumber, out BigInteger root)) return root;

    // 2. Faktorbasis generieren
    // Wir suchen Primzahlen p, für die n ein quadratischer Rest ist (Legendre-Symbol = 1).
    factorBasePrimes = GenerateFactorBase(factorBaseSize);

    // 3. Relationen sammeln ("Sieben" / Suche nach glatten Zahlen)
    // Wir nutzen die Kettenbruch-Entwicklung von sqrt(n), um Kandidaten zu finden.
    FindSmoothRelations();

    // 4. Lineare Abhängigkeiten in der Matrix finden (Gauß-Elimination)
    // und daraus den ggT berechnen.
    return FindDependencyAndGcd();
  }

  /// <summary>
  ///   Berechnet die ganzzahlige Quadratwurzel mit dem Newton-Verfahren.
  /// </summary>
  private static BigInteger CalculateIntegerSquareRoot(BigInteger n)
  {
    if (n < 0) throw new ArgumentException("Keine Wurzel aus negativen Zahlen.");
    if (n == 0) return 0;

    // Gute Startschätzung basierend auf der Länge der Zahl
    BigInteger x = BigInteger.Pow(10, n.ToString().Length / 2 + 1);
    while (true)
    {
      BigInteger y = (x + n / x) >> 1; // (x + n/x) / 2
      if (y >= x) return x;
      x = y;
    }
  }

  // --- Basisfunktionen / Hilfsmathematik ---

  private static bool IsPerfectSquare(BigInteger n, out BigInteger root)
  {
    root = CalculateIntegerSquareRoot(n);
    return root * root == n;
  }

  /// <summary>
  ///   Einfacher Primzahltest (Trial Division) für die Erstellung der Faktorbasis.
  ///   Ausreichend für kleine p.
  /// </summary>
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
  ///   Löst das lineare Gleichungssystem über GF(2) (Modulo 2).
  ///   Ziel: Eine Kombination von Zeilen finden, deren Summe der Exponentenvektoren 0 (mod 2) ergibt.
  ///   Das bedeutet, das Produkt der zugehörigen q-Werte ist eine Quadratzahl.
  /// </summary>
  private BigInteger FindDependencyAndGcd()
  {
    int rowCount = smoothRelations.Count;
    int colCount = factorBasePrimes.Count;

    // Matrix aufbauen: Jede Zeile ist der Paritätsvektor einer Relation
    BitArray[] matrix = smoothRelations.Select(r => new BitArray(r.exponentParityVector)).ToArray();

    // History speichert, welche Original-Zeilen kombiniert wurden, um die aktuelle Zeile zu erzeugen.
    // Initial ist history[i] einfach nur die i-te Zeile selbst.
    BitArray[] history = new BitArray[rowCount];
    for (int i = 0; i < rowCount; i++)
    {
      history[i] = new BitArray(rowCount);
      history[i].Set(i, true);
    }

    // --- Gauß-Elimination ---
    int pivotRow = 0;
    for (int col = 0; col < colCount && pivotRow < rowCount; col++)
    {
      // Suche eine Zeile, die in der aktuellen Spalte eine 1 hat
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
        // Tausche die gefundene Zeile mit der aktuellen Pivot-Zeile (falls nötig)
        (matrix[selectedRow], matrix[pivotRow]) = (matrix[pivotRow], matrix[selectedRow]);
        (history[selectedRow], history[pivotRow]) = (history[pivotRow], history[selectedRow]);

        // Eliminiere die 1 in dieser Spalte für alle anderen Zeilen durch XOR
        for (int r = 0; r < rowCount; r++)
        {
          if (r != pivotRow && matrix[r][col])
          {
            matrix[r].Xor(matrix[pivotRow]); // Matrix-Reduktion
            history[r].Xor(history[pivotRow]); // Historie mitführen
          }
        }
        pivotRow++;
      }
    }

    // --- Lösungssuche ---
    // Wir suchen jetzt Null-Zeilen in der Matrix. Eine Null-Zeile bedeutet,
    // wir haben eine Kombination von Relationen gefunden, deren Produkt ein Quadrat ist.
    for (int r = 0; r < rowCount; r++)
    {
      bool isRowAllZero = true;
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
        // Wir haben eine Lösung! Jetzt bauen wir x und y zusammen.
        BigInteger x = 1;
        BigInteger ySquared = 1;

        // Durchlaufe die Historie, um zu sehen, welche Relationen beteiligt waren
        for (int i = 0; i < rowCount; i++)
        {
          if (history[r][i]) // Wenn Relation i Teil der Lösung ist
          {
            x = x * smoothRelations[i].congruenceX % targetCompositeNumber;
            ySquared *= smoothRelations[i].congruenceQ;
          }
        }

        // y ist die Wurzel aus dem Produkt der q-Werte
        BigInteger y = CalculateIntegerSquareRoot(ySquared);

        // Die entscheidende Prüfung: GCD(x - y, n)
        // Wenn x^2 ≡ y^2 (mod n), aber x !≡ ±y (mod n), dann liefert der ggT einen Faktor.
        BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(x - y), targetCompositeNumber);
        if (gcd > 1 && gcd < targetCompositeNumber)
        {
          return gcd; // Faktor gefunden!
        }
      }
    }
    return -1; // Keine Faktoren gefunden
  }

  // --- Mathematische Kernlogik ---

  /// <summary>
  ///   Generiert die Kettenbruch-Entwicklung von sqrt(n) und sucht nach "glatten" Resten.
  ///   Dies ist der rechenintensivste Teil (Schritt: Datensammlung).
  /// </summary>
  private void FindSmoothRelations()
  {
    // Initialisierung der Kettenbruch-Variablen (Standard-Notation P, Q, a in der Literatur)
    // Hier ausführlicher benannt für Verständnis.

    // Startwerte für die Wurzel-Annäherung
    BigInteger rootN = CalculateIntegerSquareRoot(targetCompositeNumber);
    BigInteger remainderTerm = 0; // Entspricht 'm' oder 'P' in Formeln
    BigInteger denominatorTerm = 1; // Entspricht 'd' oder 'Q' in Formeln
    BigInteger expansionCoefficient = rootN; // Entspricht 'a' (aktueller Koeffizient)

    // Variablen für die Berechnung der Näherungsbrüche (Konvergenten) p/q
    // Wir brauchen nur den Zähler 'p' (hier 'numerator'), da x = p mod n ist.
    BigInteger prevNumerator = 1;
    BigInteger currentNumerator = expansionCoefficient;

    // Wir sammeln etwas mehr Relationen als die Basisgröße, um sicherzustellen,
    // dass das Gleichungssystem lösbar ist (Überbestimmtheit).
    while (smoothRelations.Count < factorBaseSize + 20)
    {
      // Berechne den quadratischen Rest q:
      // x^2 ≡ q (mod n)  =>  Hier ist x = currentNumerator
      BigInteger quadraticResidue = currentNumerator * currentNumerator % targetCompositeNumber;

      // Optimierung: Wenn q > n/2, nutzen wir den negativen Rest (q - n).
      // Beispiel: 8 mod 10 ist 8, aber äquivalent zu -2. Kleine Zahlen lassen sich leichter faktorisieren.
      if (quadraticResidue > targetCompositeNumber / 2)
      {
        quadraticResidue -= targetCompositeNumber;
      }

      // Prüfen, ob q "glatt" über unserer Faktorbasis ist.
      // Das heißt: Lässt sich q komplett durch die Primzahlen in unserer Basis teilen?
      if (IsNumberSmoothOverFactorBase(quadraticResidue, out BitArray exponentParityVector))
      {
        smoothRelations.Add(new SmoothRelation
        {
          congruenceX = currentNumerator,
          exponentParityVector = exponentParityVector,
          congruenceQ = quadraticResidue
        });
      }

      // --- Nächster Schritt im Kettenbruch (Rekursionsformeln) ---

      // 1. Neuen Restterm berechnen: m_{k+1} = d_k * a_k - m_k
      remainderTerm = denominatorTerm * expansionCoefficient - remainderTerm;

      // 2. Neuen Nennerterm berechnen: d_{k+1} = (n - m_{k+1}^2) / d_k
      denominatorTerm = (targetCompositeNumber - remainderTerm * remainderTerm) / denominatorTerm;

      // 3. Neuen Koeffizienten berechnen: a_{k+1} = (a0 + m_{k+1}) / d_{k+1}
      expansionCoefficient = (rootN + remainderTerm) / denominatorTerm;

      // 4. Neuen Zähler des Näherungsbruchs berechnen (modulo n)
      BigInteger nextNumerator = (expansionCoefficient * currentNumerator + prevNumerator) % targetCompositeNumber;

      // Verschieben für nächste Iteration
      prevNumerator = currentNumerator;
      currentNumerator = nextNumerator;
    }
  }

  /// <summary>
  ///   Erstellt die Faktorbasis.
  ///   Wählt kleine Primzahlen p, für die das Legendre-Symbol (n/p) = 1 ist.
  ///   Das bedeutet, die Gleichung x^2 ≡ n (mod p) hat eine Lösung.
  ///   Solche Primzahlen tauchen wahrscheinlicher in der Faktorisierung von x^2 - n auf.
  /// </summary>
  private List<int> GenerateFactorBase(int size)
  {
    List<int> primes = [-1, 2]; // -1 für das Vorzeichen, 2 als erste Primzahl
    int candidate = 3;
    while (primes.Count < size)
    {
      if (IsSmallPrime(candidate))
      {
        // Prüfe Legendre-Symbol: n^((p-1)/2) mod p == 1
        if (BigInteger.ModPow(targetCompositeNumber, (candidate - 1) / 2, candidate) == 1)
        {
          primes.Add(candidate);
        }
      }
      candidate += 2;
    }
    return primes;
  }

  /// <summary>
  ///   Versucht, 'q' vollständig durch die Zahlen in der Faktorbasis zu teilen (Trial Division).
  /// </summary>
  /// <param name="residue">Der zu prüfende Rest (q).</param>
  /// <param name="exponentParity">
  ///   Ausgabe: Ein BitArray, das speichert, ob der Exponent eines Primfaktors ungerade (true)
  ///   oder gerade (false) ist.
  /// </param>
  /// <returns>True, wenn die Zahl vollständig zerlegt werden konnte ("B-Smooth"), sonst False.</returns>
  private bool IsNumberSmoothOverFactorBase(BigInteger residue, out BitArray exponentParity)
  {
    exponentParity = new BitArray(factorBasePrimes.Count);
    BigInteger tempResidue = BigInteger.Abs(residue);

    // Sonderfall: Vorzeichen (behandelt wie Faktor -1 am Index 0)
    if (residue < 0)
    {
      exponentParity[0] = true;
    }

    // Probedivision durch die Faktorbasis
    for (int i = 1; i < factorBasePrimes.Count; i++)
    {
      int prime = factorBasePrimes[i];
      while (tempResidue > 0 && tempResidue % prime == 0)
      {
        // Wir brauchen nur die Parität (gerade/ungerade) des Exponenten.
        // Umschalten (Toggle) ist wie Addition modulo 2.
        exponentParity[i] = !exponentParity[i];
        tempResidue /= prime;
      }
    }

    // Wenn tempResidue am Ende 1 ist, konnten wir alle Faktoren finden.
    return tempResidue == 1;
  }

  /// <summary>
  ///   Interne Struktur für eine gefundene Relation.
  ///   Speichert die Daten für die Gleichung: x^2 ≡ q (mod n)
  /// </summary>
  private struct SmoothRelation
  {
    public BigInteger congruenceQ; // Der "rechte" Teil der Gleichung (Residuum)
    public BigInteger congruenceX; // Der "linke" Teil der Gleichung
    public BitArray exponentParityVector; // Exponenten mod 2 (für Gauß)
  }
}