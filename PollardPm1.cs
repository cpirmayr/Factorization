using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Factorization
{

  /// <summary>
  /// Pollard p-1 Faktorisierungsalgorithmus (optimierte Version).
  /// Findet einen nicht-trivialen Faktor von n, wenn p-1 B-smooth ist.
  /// </summary>
  public class PollardPMinus1
  {
    // ── Eigenschaften ────────────────────────────────────────────────────────

    /// <summary>
    /// Smoothness-Schranke B.
    /// 0 = heuristische Berechnung via ComputeBound(n).
    /// </summary>
    public BigInteger Bound { get; set; }

    /// <summary>Alle wieviel Primzahlschritte der ggT geprüft wird.</summary>
    public int GcdInterval { get; set; }

    /// <summary>Basis für die Modularpotenzierung.</summary>
    public BigInteger Base { get; set; }

    /// <summary>Laufzeit der letzten Factor()-Ausführung.</summary>
    public TimeSpan LastRunTime { get; private set; }

    /// <summary>Verwendete Schranke der letzten Factor()-Ausführung.</summary>
    public BigInteger LastBoundUsed { get; private set; }

    // ── Konstruktor ──────────────────────────────────────────────────────────

    /// <param name="bound">
    ///   Smoothness-Schranke B als BigInteger.
    ///   Wenn 0 (default), wird sie heuristisch aus n berechnet:
    ///   B ≈ exp(√(ln n · ln ln n) / √2).
    /// </param>
    /// <param name="gcdInterval">ggT-Prüfintervall (default: 20).</param>
    /// <param name="base_">Basis (default: 2).</param>
    public PollardPMinus1(BigInteger bound = default, int gcdInterval = 20, BigInteger base_ = default)
    {
      Bound = bound == default ? BigInteger.Zero : bound;
      GcdInterval = gcdInterval;
      Base = base_ == default ? 2 : base_;
    }

    // ── Öffentliche API ──────────────────────────────────────────────────────

    /// <summary>
    /// Versucht, einen nicht-trivialen Faktor von n zu finden.
    /// </summary>
    /// <param name="n">Die zu faktorisierende Zahl (zusammengesetzt, ungerade, > 4).</param>
    /// <returns>
    ///   Faktor von n (1 &lt; f &lt; n), oder
    ///   -1 wenn keiner gefunden wurde (Schranke zu klein),
    ///   -2 bei ungültigem Input.
    /// </returns>
    public BigInteger Factor(BigInteger n)
    {
      if (n < 4 || n % 2 == 0)
        return -2;

      BigInteger bound = Bound > 0 ? Bound : ComputeBound(n);
      LastBoundUsed = bound;

      var sw = Stopwatch.StartNew();
      BigInteger result = FactorInternal(n, bound);
      sw.Stop();
      LastRunTime = sw.Elapsed;

      return result;
    }

    /// <summary>
    /// Berechnet die heuristische Schranke für n.
    /// B ≈ exp(√(ln n · ln ln n) / √2)
    /// Nützlich um vorab zu sehen, welchen Wert die Automatik wählt.
    /// </summary>
    public static BigInteger ComputeBound(BigInteger n)
    {
      double lnn = (double) BigInteger.Log(n);
      double lnlnn = Math.Log(lnn);

      double exponent = Math.Sqrt(lnn * lnlnn) / Math.Sqrt(2.0);
      double b = Math.Exp(exponent);

      b = Math.Clamp(b, 1_000, 1e15);

      return (BigInteger) b;
    }

    // ── Implementierung ──────────────────────────────────────────────────────

    private BigInteger FactorInternal(BigInteger n, BigInteger bound)
    {
      var primes = SieveOfEratosthenes(bound);
      BigInteger a = Base;
      int i = 0;

      foreach (long p in primes)
      {
        int exp = (int) (BigInteger.Log(bound) / Math.Log(p));
        a = BigInteger.ModPow(a, BigInteger.Pow(p, exp), n);

        if (++i % GcdInterval == 0)
        {
          BigInteger g = BigInteger.GreatestCommonDivisor(a - 1, n);
          if (g > 1 && g < n) return g;
          if (g == n) return -1;
        }
      }

      BigInteger gcd = BigInteger.GreatestCommonDivisor(a - 1, n);
      return gcd > 1 && gcd < n ? gcd : -1;
    }

    private static List<long> SieveOfEratosthenes(BigInteger limit)
    {
      if (limit > long.MaxValue)
        throw new ArgumentOutOfRangeException(nameof(limit),
            "Schranke zu groß für Sieb (max. long.MaxValue).");

      long lim = (long) limit;
      var isComposite = new bool[lim + 1];
      var primes = new List<long>();

      for (long i = 2; i <= lim; i++)
      {
        if (isComposite[i]) continue;
        primes.Add(i);
        for (long j = i * i; j <= lim; j += i)
          isComposite[j] = true;
      }
      return primes;
    }
  }
}