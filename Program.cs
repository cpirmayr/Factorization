using System.Numerics;
using Factorization;

const int digitsCount = 25;
int factorizationMethod = 5;
if (args.Length == 1 && int.TryParse(args[0], out int method))
{
  factorizationMethod = method;
}

Factorize(digitsCount, factorizationMethod);

static BigInteger FactorizePollardChebyshev(BigInt n)
{
  var d = n.Root(4);
  for (var i = 0; i < 100; ++i)
  {
    Console.WriteLine(i);
    var powers = new Dictionary<BigInt, BigInt>();
    var c = 1;
    BigInt a = 2 + i * 4;
    while (c < d)
    {
      ++c;
      BigInt b = BigIntegerHelpers.CalculateChebyshev(a, a + 1, n);
      if (powers.TryGetValue(b, out BigInt value))
      {
        Console.WriteLine(c);
        return BigInt.GreatestCommonDivisor(value - b, n);
      }
      powers.Add(b, a);
      a = b;
    }
  }
  return 1;
}

static BigInteger FactorizePollardPM1(BigInteger n)
{
  BigInteger a = 2;
  BigInteger b = 2;
  BigInteger c = 2;
  while (true)
  {
    a = BigInteger.ModPow(a, b, n);
    BigInteger d = BigInteger.GreatestCommonDivisor(a - 1, n);
    if (d != 1)
    {
      return d;
    }
    /*
    b *= c;
    ++c;
    */
    ++b;
  }
}

static BigInteger FactorizePollardPM1Ex(BigInt n)
{
  BigInt a = 2;
  BigInt b = n.SquareRoot().Even;

  InitializeDifferences(b, 2, out var differences);

  while (true)
  {
    a = a.PowerMod(differences[0], n);
    BigInt d = BigInt.GreatestCommonDivisor(a - 1, n);
    if (d != 1)
    {
      return d;
    }
    if (d == n)
    {
      return 1;
    }
    for (int i = 0; i < differences.Count - 1; ++i)
    {
      differences[i] += differences[i + 1];
    }
  }
}

static BigInteger FactorizePollardPM1Ex1(BigInt n)
{
  BigInt limit = n.Root(3);
  BigInt b = 3;
  // BigInt u = n.PowRoot(4, 2 * 5);
  // BigInt u = n.PowRoot(1, 2);
  BigInt u = n.PowRoot(1, 3);
  // BigInt l = n.PowRoot(1, 1 * 2);
  BigInt l = n.PowRoot(1, 4);
  BigInt e = l * u;
  BigInt d = u - l - 1;

  for (int i = 0; i < limit; ++i)
  {
    b = b.PowerMod(e, n);
    BigInt r = BigInt.GreatestCommonDivisor(b - 1, n);
    if (r != 1)
    {
      return r;
    }
    else if (r == n)
    {
      return n;
    }
    e += d;
    d -= 2;
  }
  return 1;
}

/// <summary>
/// Initialisiert die "Eimerkette" (Differenzenliste) für eine gefaltete Folge.
/// </summary>
/// <param name="N">Die Länge der Folge (N).</param>
/// <param name="foldings">Die Anzahl der Faltungen (m).</param>
/// <param name="differences">Ausgabe: Liste der Startwerte [Wert, Diff1, Diff2, ..., DiffKonstant].</param>
static void InitializeDifferences(BigInteger N, BigInteger foldings, out List<BigInteger> differences)
{
  // Konvertierung in int für Schleifen (physikalisch limitiert durch RAM/CPU, daher sicher)
  // int exp = (int) exponent;
  int m = (int) foldings;

  // 1. Bestimme N und den Grad des Polynoms
  // BigInteger N = BigInteger.Pow(2, exp);

  // Der Grad entspricht 2^m (z.B. bei 2 Faltungen = Grad 4)
  int degree = 1 << m;

  // 2. Berechnung der Stützstellen (a_1 bis a_{degree+1})
  // Um die k-te Differenz zu finden, benötigen wir k+1 Werte.
  List<BigInteger> sequenceValues = new List<BigInteger>();

  // Wir berechnen nur die absolut notwendigen ersten Glieder der Folge direkt.
  // Das ist sehr effizient, da wir nicht die ganze Fakultät berechnen müssen.
  for (int n = 1; n <= degree + 1; n++)
  {
    sequenceValues.Add(CalculateFoldedValue(n, N, m));
  }

  // 3. Aufbau der Eimerkette (Differenzen-Schema)
  // Wir benötigen jeweils das erste Element jeder Differenzen-Ebene.
  differences = new List<BigInteger>();

  // Ebene 0 (Die Werte selbst)
  List<BigInteger> currentLevel = sequenceValues;
  differences.Add(currentLevel[0]); // Startwert a_1

  // Berechnung der Differenzen-Ebenen (Newton-Schema)
  for (int i = 0; i < degree; i++)
  {
    List<BigInteger> nextLevel = new List<BigInteger>();

    for (int j = 0; j < currentLevel.Count - 1; j++)
    {
      // Differenz = Nachfolger - Vorgänger
      nextLevel.Add(currentLevel[j + 1] - currentLevel[j]);
    }

    if (nextLevel.Count > 0)
    {
      differences.Add(nextLevel[0]); // Das erste Element der neuen Ebene merken
    }

    currentLevel = nextLevel;
  }

  // Am Ende enthält 'differences' genau [a_1, d_1, d_2, ..., d_Konstant]
  // Damit kann die iterative Addition ("Eimerkette") gestartet werden.
}

/// <summary>
/// Hilfsmethode: Berechnet direkt den Wert a_n für eine gegebene Faltung,
/// indem die Indizes rückwärts "entfaltet" werden.
/// </summary>
/// <param name="n">Index des Gliedes.</param>
/// <param name="N">Länge der Folge.</param>
/// <param name="foldings">Anzahl der Faltungen.</param>
/// <returns>Das berechnete Glied a_n.</returns>
static BigInteger CalculateFoldedValue(int n, BigInteger N, int foldings)
{
  // Liste der Faktoren, die dieses Glied bilden. Wir starten mit Index n.
  List<BigInteger> factors = new List<BigInteger>();
  factors.Add(n);

  // Aktuelle Länge der Reihe in der gefalteten Ebene
  // Startet bei N / 2^foldings
  BigInteger currentLen = N >> foldings;

  // Rückwärts entfalten (von Faltung m runter zu 0)
  for (int i = 0; i < foldings; i++)
  {
    // Beim Entfalten verdoppelt sich die Länge
    currentLen *= 2;

    // Jeder Index x in der gefalteten Ebene entsteht aus zwei Indizes in der Ebene darunter:
    // x und (Länge + 1 - x). Wir fügen den Partner hinzu.
    int count = factors.Count;
    for (int j = 0; j < count; j++)
    {
      BigInteger partner = currentLen + 1 - factors[j];
      factors.Add(partner);
    }
  }

  // Das Produkt aller gesammelten Faktoren ist a_n
  BigInteger result = 1;
  foreach (var f in factors)
  {
    result *= f;
  }
  return result;
}

static void Factorize(int digitsCount, int factorizationMethod)
{
  BigInteger n = BigIntegerHelpers.GenerateSemiPrime(digitsCount);
  Console.WriteLine($"{n} =");
  BigInteger result = factorizationMethod switch
  {
    0 => PollardRho.Factorize(n),
    1 => CFRAC.Factorize(n),
    2 => FactorizePollardChebyshev(n),
    3 => FactorizePollardPM1(n),
    4 => FactorizePollardPM1Ex(n),
    5 => FactorizePollardPM1Ex1(n),
    _ => 1
  };
  Console.WriteLine($"{result} x {n / result}");
}