using System.Numerics;

namespace Factorization;

/// <summary>
/// Implementierung von Shanks' Square Forms Factorization (SQUFOF) für BigInteger.
/// 
/// SQUFOF nutzt die Infrastruktur quadratischer Formen, um einen Faktor von N zu finden.
/// Die Idee: Man iteriert über die Kettenbruchentwicklung von sqrt(kN) und sucht nach
/// einer "fast quadratischen" Form – einem Moment, wo Q_i ein perfektes Quadrat ist.
/// Von dort geht man rückwärts und liest den Faktor ab.
/// 
/// Geeignet für Zahlen bis ca. 18-20 Dezimalstellen (danach werden Long-Arithmetik-
/// Überläufe kritisch; für echte BigIntegers empfiehlt sich MPQS oder NFS).
/// Diese Implementierung arbeitet intern mit BigInteger, verliert dadurch etwas
/// Geschwindigkeit, gewinnt aber Korrektheit für größere Eingaben.
/// </summary>
public static class Squfof
{
  // Multiplikatoren nach Shanks/Riesel – verbessern die Erfolgsrate für verschiedene N
  private static readonly int[] Multipliers =
  [
    1, 3, 5, 7, 11, 3*5, 3*7, 3*11, 5*7, 5*11, 7*11,
    3*5*7, 3*5*11, 3*7*11, 5*7*11, 3*5*7*11
  ];

  /// <summary>
  /// Versucht, einen nicht-trivialen Faktor von <paramref name="n"/> zu finden.
  /// Gibt 0 zurück, wenn keiner gefunden wurde (dann Primtest empfehlenswert).
  /// </summary>
  public static BigInteger Factor(BigInteger n)
  {
    if (n < 2) return 0;
    if (n % 2 == 0) return 2;
    if (IsPerfectSquare(n, out BigInteger sr)) return sr;

    // Probedivision durch kleine Primzahlen
    BigInteger small = TrialDivision(n);
    if (small != 0) return small;

    // SQUFOF mit verschiedenen Multiplikatoren versuchen
    foreach (int k in Multipliers)
    {
      BigInteger f = SqufofCore(n, k);
      if (f > 1 && f < n) return f;
    }

    return 0; // N ist vermutlich prim oder erfordert eine stärkere Methode
  }

  /// <summary>
  /// Vollständige Faktorisierung: gibt alle Primfaktoren zurück (mit Vielfachheit).
  /// </summary>
  public static List<BigInteger> Factorize(BigInteger n)
  {
    List<BigInteger> factors = [];
    if (n <= 1) return factors;

    Queue<BigInteger> queue = new Queue<BigInteger>();
    queue.Enqueue(n);

    while (queue.Count > 0)
    {
      BigInteger x = queue.Dequeue();
      if (x == 1) continue;

      if (IsProbablePrime(x))
      {
        factors.Add(x);
        continue;
      }

      BigInteger d = Factor(x);
      if (d == 0 || d == x)
      {
        // Fallback: als prim behandeln (sollte nicht vorkommen)
        factors.Add(x);
      }
      else
      {
        queue.Enqueue(d);
        queue.Enqueue(x / d);
      }
    }

    factors.Sort();
    return factors;
  }

  // -------------------------------------------------------------------------
  // Kern-Algorithmus
  // -------------------------------------------------------------------------

  private static BigInteger SqufofCore(BigInteger n, long k)
  {
    BigInteger kn = k * n;

    // Iterationsgrenze: ~sqrt(sqrt(kN)) * Konstante
    // Wir schätzen grob; für sehr große N muss man ggf. erhöhen.
    BigInteger sqrtKn = Isqrt(kn);
    long limit = (long) BigInteger.Min(1_000_000, Isqrt(sqrtKn) * 3 + 100);

    // Initialisierung der Kettenbruchiteration
    BigInteger P0 = sqrtKn;
    BigInteger Q0 = 1;
    BigInteger Q1 = kn - P0 * P0;   // = kN - floor(sqrt(kN))^2

    if (Q1 == 0)
    {
      // kN ist ein perfektes Quadrat
      BigInteger g = BigInteger.GreatestCommonDivisor(n, sqrtKn);
      return g > 1 && g < n ? g : 0;
    }

    BigInteger Pprev = P0, Qprev = Q0, Qcurr = Q1;
    BigInteger sqrtQ;

    // ---- Phase 1: Vorwärtsiteration, Suche nach perfekt-quadratischem Q ----
    for (long i = 1; i <= limit; i++)
    {
      BigInteger b = (sqrtKn + Pprev) / Qcurr;
      BigInteger Pnew = b * Qcurr - Pprev;
      BigInteger Qnew = Qprev + b * (Pprev - Pnew);

      Qprev = Qcurr;
      Pprev = Pnew;
      Qcurr = Qnew;

      // Nur bei ungeradem i testen (Shanks' Beobachtung)
      if ((i & 1) == 1 && IsPerfectSquare(Qcurr, out sqrtQ))
      {
        // ---- Phase 2: Rückwärtsiteration ab der Wurzel ----
        BigInteger b0 = (sqrtKn - Pprev) / sqrtQ;
        BigInteger Pinv = b0 * sqrtQ + Pprev;
        BigInteger Qinv_prev = sqrtQ;
        BigInteger Qinv_curr = (kn - Pinv * Pinv) / sqrtQ;

        for (long j = 0; j < limit; j++)
        {
          BigInteger b2 = (sqrtKn + Pinv) / Qinv_curr;
          BigInteger Pinv2 = b2 * Qinv_curr - Pinv;
          BigInteger Qinv2 = Qinv_prev + b2 * (Pinv - Pinv2);

          if (Pinv == Pinv2) // Periode gefunden
          {
            BigInteger factor = BigInteger.GreatestCommonDivisor(n, Pinv);
            if (factor > 1 && factor < n) return factor;
            break; // dieser Multiplikator hilft nicht
          }

          Qinv_prev = Qinv_curr;
          Pinv = Pinv2;
          Qinv_curr = Qinv2;
        }
      }
    }

    return 0;
  }

  // -------------------------------------------------------------------------
  // Hilfsmethoden
  // -------------------------------------------------------------------------

  /// <summary>Probedivision bis 1000.</summary>
  private static BigInteger TrialDivision(BigInteger n)
  {
    int[] smallPrimes =
    [
      2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47,
      53, 59, 61, 67, 71, 73, 79, 83, 89, 97,
      101,103,107,109,113,127,131,137,139,149,
      151,157,163,167,173,179,181,191,193,197,
      199,211,223,227,229,233,239,241,251,257,
      263,269,271,277,281,283,293,307,311,313,
      317,331,337,347,349,353,359,367,373,379,
      383,389,397,401,409,419,421,431,433,439,
      443,449,457,461,463,467,479,487,491,499,
      503,509,521,523,541,547,557,563,569,571,
      577,587,593,599,601,607,613,617,619,631,
      641,643,647,653,659,661,673,677,683,691,
      701,709,719,727,733,739,743,751,757,761,
      769,773,787,797,809,811,821,823,827,829,
      839,853,857,859,863,877,881,883,887,
      907,911,919,929,937,941,947,953,967,971,
      977,983,991,997
    ];

    foreach (int p in smallPrimes)
      if (n % p == 0 && n != p) return p;

    return 0;
  }

  /// <summary>Ganzzahlige Quadratwurzel (floor).</summary>
  public static BigInteger Isqrt(BigInteger n)
  {
    if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
    if (n == 0) return 0;

    // Newton-Raphson-Iteration
    BigInteger x = BigInteger.One << ((int) (BigInteger.Log(n, 2) / 2) + 1);
    while (true)
    {
      BigInteger y = (x + n / x) >> 1;
      if (y >= x) return x;
      x = y;
    }
  }

  /// <summary>Prüft, ob n ein perfektes Quadrat ist; gibt ggf. die Wurzel zurück.</summary>
  public static bool IsPerfectSquare(BigInteger n, out BigInteger root)
  {
    if (n < 0) { root = 0; return false; }
    root = Isqrt(n);
    return root * root == n;
  }

  /// <summary>
  /// Miller-Rabin-Primtest (deterministisch für n &lt; 3,317,044,064,679,887,385,961,981;
  /// für größere N probabilistisch mit den gewählten Zeugen).
  /// </summary>
  public static bool IsProbablePrime(BigInteger n)
  {
    if (n < 2) return false;
    if (n == 2 || n == 3 || n == 5 || n == 7) return true;
    if (n % 2 == 0 || n % 3 == 0 || n % 5 == 0) return false;

    // Schreibe n-1 = 2^r * d
    BigInteger d = n - 1;
    int r = 0;
    while (d % 2 == 0) { d >>= 1; r++; }

    // Zeugen: deterministisch korrekt bis ca. 3.3 * 10^24
    BigInteger[] witnesses = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37];

    foreach (BigInteger a in witnesses)
    {
      if (a >= n) continue;
      BigInteger x = BigInteger.ModPow(a, d, n);
      if (x == 1 || x == n - 1) continue;

      bool composite = true;
      for (int i = 0; i < r - 1; i++)
      {
        x = BigInteger.ModPow(x, 2, n);
        if (x == n - 1) { composite = false; break; }
      }
      if (composite) return false;
    }
    return true;
  }
}