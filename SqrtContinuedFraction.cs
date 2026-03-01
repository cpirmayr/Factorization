namespace Factorization;

internal static class SqrtContinuedFraction
{
  // Liefert die a_k (Teilquotienten)
  public static IEnumerable<BigInt> ContinuedFractionSqrt(BigInt n)
  {
    BigInt a0 = IntegerSqrt(n);
    if (a0 * a0 == n)
      yield break; // perfektes Quadrat
    BigInt m = 0;
    BigInt d = 1;
    BigInt a = a0;
    yield return a;
    while (true)
    {
      m = d * a - m;
      d = (n - m * m) / d;
      a = (a0 + m) / d;
      yield return a;
    }
  }

  // Liefert Konvergenten p_k / q_k
  public static IEnumerable<(BigInt p, BigInt q)> ConvergentsOfSqrt(
    BigInt n,
    int maxIterations)
  {
    BigInt p0 = 0, p1 = 1;
    BigInt q0 = 1, q1 = 0;
    int i = 0;
    foreach (BigInt a in ContinuedFractionSqrt(n))
    {
      BigInt p = (a * p1 + p0) % n;
      BigInt q = (a * q1 + q0) % n;
      yield return (p, q);
      p0 = p1;
      p1 = p;
      q0 = q1;
      q1 = q;
      if (++i >= maxIterations)
        yield break;
    }
  }

  // Ganzzahliges sqrt via Newton
  public static BigInt IntegerSqrt(BigInt n)
  {
    if (n < 0) throw new ArgumentException("n must be nonnegative");
    if (n == 0) return 0;
    BigInt x = n;
    BigInt y = (x + 1) >> 1;
    while (y < x)
    {
      x = y;
      y = (x + n / x) >> 1;
    }
    return x;
  }
}