using System.Numerics;

namespace Factorization;

/// <summary>
///   Implements the "Continued Fraction Factorization".
/// </summary>
public class ContinuedFractionFactorization
{
  /// <summary>
  ///   Factorizes a composite number using the Continued Fraction Factorization algorithm.
  /// </summary>
  /// <param name="n">The number to factorize. Should be an odd composite number.</param>
  /// <returns>
  ///   A non-trivial factor of <paramref name="n" /> if found; otherwise, returns 1.
  ///   The returned factor satisfies 1 &lt; factor &lt; n.
  /// </returns>
  /// <remarks>
  ///   <para>
  ///     This method implements the Continued Fraction Factorization (CFRAC) algorithm, which finds factors
  ///     by computing the continued fraction expansion of sqrt(n) and using linear algebra over GF(2)
  ///     to construct a congruence of squares: x² ≡ y² (mod n).
  ///   </para>
  ///   <para>
  ///     The algorithm works as follows:
  ///     <list type="number">
  ///       <item>
  ///         <description>Generates a factor base of small primes satisfying the Legendre symbol condition.</description>
  ///       </item>
  ///       <item>
  ///         <description>Computes convergents of the continued fraction of sqrt(n).</description>
  ///       </item>
  ///       <item>
  ///         <description>For each convergent, factors the residue using only primes from the factor base.</description>
  ///       </item>
  ///       <item>
  ///         <description>Collects smooth relations (B-smooth numbers) until sufficient linear dependencies are found.</description>
  ///       </item>
  ///       <item>
  ///         <description>Solves the linear system over GF(2) to construct x and y such that x² ≡ y² (mod n).</description>
  ///       </item>
  ///       <item>
  ///         <description>Computes gcd(x + y, n) to extract a non-trivial factor.</description>
  ///       </item>
  ///     </list>
  ///   </para>
  ///   <para>
  ///     The algorithm prints diagnostic information to the console during execution, including the input number,
  ///     prime base size, progress of relation collection, and the final factor components.
  ///   </para>
  /// </remarks>
  public static BigInt Factorize(BigInt n)
  {
    Console.WriteLine($"n: {n}");
    BigInt primesLimit = (int) Math.Pow(Math.Log((double) n), 2);
    Console.WriteLine($"primes limit: {primesLimit}");
    List<BigInt> factorBase = Enumerable.Range(2, (int) primesLimit).Where(item => BigInt.IsProbablePrime(item) && (item == 2 || LegendreSymbol(n, item) == 1)).Select(static item => (BigInt) item).ToList();
    int factorBaseSize = factorBase.Count;
    int relationsRequired = factorBase.Count * 4 / 3;
    Console.WriteLine($"primes count in factor base: {factorBase.Count}");
    List<BigInt> pValues = [];
    List<int[]> relations = [];
    // foreach ((BigInt p0, BigInt q0) in SqrtContinuedFraction.ConvergentsOfSqrt(n, int.MaxValue))
    foreach ((BigInt p0, BigInt q0) in SquareRootConvergents(n, int.MaxValue))
    {
      BigInt p = p0 % n;
      BigInt q = q0 % n;
      // BigInt d = BigInt.Abs(p.Square() - n * q.Square()) % n;
      BigInt d = p.Square().SubMod(n * q.Square(), n);
      int[] relation = Enumerable.Repeat(0, factorBaseSize).ToArray();
      for (int primesIndex = 0; primesIndex < factorBaseSize; ++primesIndex)
      {
        BigInteger prime = factorBase[primesIndex];
        BigInt newDCandidate = BigInt.DivRem(d, prime, out BigInt remainder);
        while (remainder == 0)
        {
          ++relation[primesIndex];
          d = newDCandidate;
          newDCandidate = BigInt.DivRem(d, prime, out remainder);
        }
        if (d == 1)
        {
          break;
        }
      }
      if (d == 1)
      {
        pValues.Add(p % n);
        relations.Add(relation);
        if (relations.Count % 100 == 0)
        {
          // Console.WriteLine($"primes used limit: {primesUsed.Count * 5 / 4} / relations count: {relations.Count}");
          Console.WriteLine($"relations required: {relationsRequired} / relations count: {relations.Count}");
        }
      }
      // if (primesUsed.Count * 4 / 3 < relations.Count)
      if (relationsRequired < relations.Count)
      {
        BigInt y = BigInt.FindSquareRootFromRelations(relations, factorBase, n, out List<int> usedIndices);
        if (1 < y)
        {
          BigInt x = usedIndices.Aggregate<int, BigInt>(1, (current, t) => current.MulMod(pValues[t], n));
          BigInt xSquared = x.PowerMod(2, n);
          BigInt ySquared = y.PowerMod(2, n);
          BigInt gcd = BigInt.GreatestCommonDivisor(x + y, n);
          if (1 < gcd && gcd < n)
          {
            Console.WriteLine($"x: {x}\ny: {y}\nx^2: {xSquared}\ny^2: {ySquared}\ngcd: {gcd}");
            return gcd;
          }
          relations.Clear();
          // primesUsed.Clear();
          pValues.Clear();
        }
      }
    }
    return 1;
  }

  /// <summary>
  ///   Computes the Legendre symbol (a|p) for an integer <paramref name="a" /> modulo an odd prime <paramref name="p" />.
  /// </summary>
  /// <param name="a">The integer whose quadratic residuosity modulo <paramref name="p" /> is to be tested.</param>
  /// <param name="p">An odd prime modulus.</param>
  /// <returns>
  ///   1 if <paramref name="a" /> is a quadratic residue modulo <paramref name="p" /> and <paramref name="a" /> ≠ 0;
  ///   -1 if <paramref name="a" /> is a quadratic non-residue modulo <paramref name="p" />;
  ///   0 if <paramref name="a" /> ≡ 0 (mod <paramref name="p" />).
  /// </returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="p" /> is not an odd prime (p ≤ 2 or even).</exception>
  /// <remarks>
  ///   The implementation reduces <paramref name="a" /> modulo <paramref name="p" /> and applies Euler's criterion:
  ///   a^((p - 1) / 2) ≡ (a|p) (mod p). The modular exponentiation result of 1 means a quadratic residue, and
  ///   p - 1 indicates -1 (a non-residue). A result other than 1 or p - 1 should not occur for prime <paramref name="p" />.
  /// </remarks>
  public static int LegendreSymbol(BigInteger a, BigInteger p)
  {
    if (p <= 2 || !IsOdd(p))
    {
      throw new ArgumentException("p must be an odd prime.");
    }
    a %= p;
    if (a < 0)
    {
      a += p;
    }
    if (a == 0)
    {
      return 0;
    }
    BigInteger exponent = (p - 1) / 2;
    BigInteger result = BigInteger.ModPow(a, exponent, p);
    if (result == 1)
    {
      return 1;
    }
    if (result == p - 1)
    {
      return -1;
    }
    return 0; // Sollte bei primem p nicht auftreten.
  }

  private static bool IsAcceptablePrime(int value, BigInteger n) => BigInt.IsProbablePrime(value) && (value == 2 || LegendreSymbol(n, value) == 1);

  /// <summary>
  ///   Determines whether the specified integer is odd.
  /// </summary>
  /// <param name="n">The integer to test.</param>
  /// <returns><c>true</c> if <paramref name="n" /> is odd; otherwise, <c>false</c>.</returns>
  /// <remarks>
  ///   Uses a bitwise AND to inspect the least significant bit of <paramref name="n" />.
  /// </remarks>
  private static bool IsOdd(BigInteger n) => (n & 1) == 1;

  // Liefert Konvergenten p_k / q_k
  private static IEnumerable<(BigInt p, BigInt q)> SquareRootConvergents(BigInt n, int maxIterations)
  {
    BigInt p0 = 0, p1 = 1;
    BigInt q0 = 1, q1 = 0;
    int i = 0;
    foreach (BigInt a in SquareRootPartialQuotients(n, int.MaxValue))
    {
      BigInt p = (a * p1 + p0) % n;
      BigInt q = (a * q1 + q0) % n;
      yield return (p, q);
      p0 = p1;
      p1 = p;
      q0 = q1;
      q1 = q;
      if (++i >= maxIterations)
      {
        yield break;
      }
    }
  }

  /// <summary>
  ///   Computes the partial quotients of the continued fraction expansion of sqrt(n).
  /// </summary>
  /// <param name="n">The number for which to compute the square root's continued fraction.</param>
  /// <param name="count">The maximum number of partial quotients to generate.</param>
  /// <returns>
  ///   An enumerable sequence of partial quotients a₀, a₁, a₂, ... of the continued fraction [a₀; a₁, a₂, ...].
  /// </returns>
  /// <remarks>
  ///   <para>
  ///     The continued fraction expansion of an irrational quadratic surd sqrt(n) is periodic.
  ///     This method uses the standard algorithm to compute the sequence of partial quotients by maintaining
  ///     the recurrence relations:
  ///     <list type="bullet">
  ///       <item>
  ///         <description>m_{i+1} = d_i * a_i - m_i</description>
  ///       </item>
  ///       <item>
  ///         <description>d_{i+1} = (n - m_{i+1}²) / d_i</description>
  ///       </item>
  ///       <item>
  ///         <description>a_{i+1} = floor((a₀ + m_{i+1}) / d_{i+1})</description>
  ///       </item>
  ///     </list>
  ///   </para>
  ///   <para>
  ///     Starting with a₀ = floor(sqrt(n)), m₀ = 0, and d₀ = 1.
  ///   </para>
  /// </remarks>
  private static IEnumerable<BigInt> SquareRootPartialQuotients(BigInt n, long count)
  {
    BigInt a0 = n.SquareRoot();
    yield return a0;
    BigInt oldM = 0;
    BigInt oldD = 1;
    BigInt oldA = a0;
    for (long i = 1; i < count; ++i)
    {
      BigInt m = oldD * oldA - oldM;
      BigInt d = (n - m.Square()) / oldD;
      BigInt a = (a0 + m) / d;
      yield return a;
      oldM = m;
      oldD = d;
      oldA = a;
    }
  }
}