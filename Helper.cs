using System.Numerics;
using System.Security.Cryptography;

namespace ConsoleApp2;

public static class BigIntegerHelpers
{
  private static readonly int[] SmallPrimes =
  [
    3, 5, 7, 11, 13, 17, 19, 23, 29, 31,
    37, 41, 43, 47, 53, 59, 61, 67
  ];

  /// <summary>
  ///   Computes the k-th Chebyshev polynomial Tk(x) modulo n using a binary ladder algorithm.
  /// </summary>
  public static BigInteger CalculateChebyshev(BigInteger x, BigInteger k, BigInteger n)
  {
    if (k.IsZero) return BigInteger.One;
    if (k.IsOne) return Mod(x, n);
    BigInteger a = BigInteger.One;
    BigInteger b = Mod(x, n);
    int bitLength = (int) k.GetBitLength();
    for (int i = bitLength - 1; i >= 0; i--)
    {
      bool bitSet = !((k >> i) & BigInteger.One).IsZero;
      if (bitSet)
      {
        a = Mod(2 * a * b - x, n);
        b = Mod(2 * b * b - BigInteger.One, n);
      }
      else
      {
        b = Mod(2 * a * b - x, n);
        a = Mod(2 * a * a - BigInteger.One, n);
      }
    }
    return a;
  }

  /// <summary>
  ///   Computes the k-th Chebyshev polynomial Tk(x) modulo n in a near constant-time manner.
  /// </summary>
  public static BigInteger CalculateChebyshevConstantTime(BigInteger x, BigInteger k, BigInteger n)
  {
    BigInteger a = BigInteger.One;
    BigInteger b = Mod(x, n);
    int bitLength = (int) k.GetBitLength();
    for (int i = bitLength - 1; i >= 0; i--)
    {
      BigInteger t0A = Mod(2 * a * a - BigInteger.One, n);
      BigInteger t0B = Mod(2 * a * b - x, n);
      BigInteger t1B = Mod(2 * b * b - BigInteger.One, n);
      bool bit = !((k >> i) & BigInteger.One).IsZero;
      a = bit ? t0B : t0A; // t1a ist gleich t0b
      b = bit ? t1B : t0B;
    }
    return a;
  }

  /// <summary>
  ///   Computes the convergents of the continued fraction expansion of a real number.
  /// </summary>
  public static IEnumerable<(long Numerator, long Denominator)> Convergents(double x, int maxIterations = 20)
  {
    long p0 = 0, p1 = 1;
    long q0 = 1, q1 = 0;
    double value = x;
    for (int i = 0; i < maxIterations; i++)
    {
      long a = (long) Math.Floor(value);
      long p = a * p1 + p0;
      long q = a * q1 + q0;
      yield return (p, q);
      double remainder = value - a;
      if (Math.Abs(remainder) < 1e-15) yield break;
      value = 1.0 / remainder;
      p0 = p1;
      p1 = p;
      q0 = q1;
      q1 = q;
    }
  }

  /// <summary>
  ///   Generates a semiprime number with the specified number of decimal digits.
  /// </summary>
  public static BigInteger GenerateSemiPrime(int decimalPlaces, int seed = -1)
  {
    if (decimalPlaces < 2)
      throw new ArgumentException("Decimal places must be at least 2.");
    int d1 = decimalPlaces / 2;
    int d2 = decimalPlaces - d1;
    BigInteger min1 = BigInteger.Pow(10, d1 - 1);
    BigInteger max1 = BigInteger.Pow(10, d1) - 1;
    BigInteger min2 = BigInteger.Pow(10, d2 - 1);
    BigInteger max2 = BigInteger.Pow(10, d2) - 1;
    Random rng = seed >= 0 ? new Random(seed) : Random.Shared;
    BigInteger p = GeneratePrimeInRange(min1, max1, rng);
    BigInteger q;
    do
    {
      q = GeneratePrimeInRange(min2, max2, rng);
    } while (q == p);
    return p * q;
  }

  public static bool IsSquare(BigInteger n)
  {
    if (n < 0) return false;
    if (n.IsZero || n.IsOne) return true;
    BigInteger r = Sqrt(n);
    return r * r == n;
  }

  public static BigInteger Sqrt(BigInteger n)
  {
    if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
    if (n < 2) return n;
    int bitLength = (int) n.GetBitLength();
    BigInteger x = BigInteger.One << ((bitLength + 1) / 2);
    while (true)
    {
      BigInteger y = (x + n / x) >> 1;
      if (y >= x) return x;
      x = y;
    }
  }

  /// <summary>
  ///   Berechnet die Näherungsbrüche (Convergents) der Quadratwurzel einer großen Zahl n.
  ///   Nutzt den exakten Algorithmus für quadratische Irrationale, um Fließkommafehler zu vermeiden.
  /// </summary>
  public static IEnumerable<(BigInteger Numerator, BigInteger Denominator)> SqrtConvergents(BigInteger n, int maxIterations = 100)
  {
    if (n < 0) throw new ArgumentException("S must be non-negative.");
    BigInteger m0 = 0;
    BigInteger d0 = 1;
    BigInteger a0 = Sqrt(n); // Nutzt deine vorhandene Sqrt-Methode

    // Prüfen, ob S ein perfektes Quadrat ist
    if (a0 * a0 == n)
    {
      yield return (a0, 1);
      yield break;
    }
    BigInteger pMinus2 = 0, pMinus1 = 1;
    BigInteger qMinus2 = 1, qMinus1 = 0;
    BigInteger m = m0;
    BigInteger d = d0;
    BigInteger a = a0;
    for (int i = 0; i < maxIterations; i++)
    {
      // Berechne den aktuellen Näherungsbruch (Pn / Qn)
      BigInteger p = a * pMinus1 + pMinus2;
      BigInteger q = a * qMinus1 + qMinus2;
      yield return (p, q);

      // Update der Koeffizienten für den nächsten Schritt (rekursive Definition)
      m = d * a - m;
      d = (n - m * m) / d;
      a = (a0 + m) / d;

      // Verschiebe die Werte für den nächsten Iterationsschritt
      pMinus2 = pMinus1;
      pMinus1 = p;
      qMinus2 = qMinus1;
      qMinus1 = q;
    }
  }

  private static BigInteger GeneratePrimeInRange(BigInteger min, BigInteger max, Random rng)
  {
    while (true)
    {
      BigInteger p = RandomBigInteger(min, max, rng);
      if (IsProbablePrimeCrypto(p)) return p;
    }
  }

  private static bool IsProbablePrimeCrypto(BigInteger n, int rounds = 64)
  {
    if (n <= 1) return false;
    if (n == 2 || n == 3) return true;
    if (n.IsEven) return false;
    if (!PassesSmallPrimeTest(n)) return false;
    BigInteger d = n - 1;
    int r = 0;
    while (d.IsEven)
    {
      d >>= 1;
      r++;
    }
    for (int i = 0; i < rounds; i++)
    {
      BigInteger a = RandomBigIntegerCrypto(2, n - 2);
      BigInteger x = BigInteger.ModPow(a, d, n);
      if (x == 1 || x == n - 1) continue;
      bool composite = true;
      for (int j = 0; j < r - 1; j++)
      {
        x = BigInteger.ModPow(x, 2, n);
        if (x == n - 1)
        {
          composite = false;
          break;
        }
      }
      if (composite) return false;
    }
    return true;
  }

  private static BigInteger Mod(BigInteger value, BigInteger modulus)
  {
    BigInteger res = value % modulus;
    return res.Sign < 0 ? res + modulus : res;
  }

  private static bool PassesSmallPrimeTest(BigInteger n)
  {
    foreach (int p in SmallPrimes)
    {
      if (n == p) return true;
      if (n % p == 0) return false;
    }
    return true;
  }

  private static BigInteger RandomBigInteger(BigInteger min, BigInteger max, Random rng)
  {
    BigInteger range = max - min;
    byte[] bytes = range.ToByteArray();
    BigInteger value;
    do
    {
      rng.NextBytes(bytes);
      bytes[^1] &= 0x7F; // Sicherstellen, dass die Zahl positiv bleibt
      value = new BigInteger(bytes);
    } while (value > range);
    return min + value;
  }

  private static BigInteger RandomBigIntegerCrypto(BigInteger min, BigInteger max)
  {
    BigInteger range = max - min;
    byte[] bytes = range.ToByteArray();
    BigInteger value;
    do
    {
      RandomNumberGenerator.Fill(bytes);
      bytes[^1] &= 0x7F;
      value = new BigInteger(bytes);
    } while (value > range);
    return min + value;
  }
}