using System.Numerics;

namespace Factorization;

internal static class FactorizationExtensions
{
  public static BigInt ShanksSqufof(this BigInt n)
  {
    return Squfof.Factor(n);
  }

  public static BigInteger WilliamsPPlus1(this BigInt n, int maxAttempts = 20)
  {
    BigInt bound = n.Power(2, 5);
    for (BigInt startP = 3; startP < 3 + maxAttempts; startP++)
    {
      BigInt v = startP % n;
      for (long k = 2; k <= bound; k++) // Montgomery-Leiter: v = V_k(startP, 1) mod n ...
      {
        BigInt vm = v, vm1 = (v * v - 2) % n;
        int highBit = 0;
        while (1L << (highBit + 1) <= k)
        {
          highBit++;
        }
        for (int i = highBit - 1; i >= 0; i--)
        {
          if (((k >> i) & 1) == 0)
          {
            vm1 = (vm * vm1 - v + n) % n;
            vm = (vm * vm - 2 + n) % n;
          }
          else
          {
            vm = (vm * vm1 - v + n) % n;
            vm1 = (vm1 * vm1 - 2 + n) % n;
          }
        }
        v = vm;
        BigInt g = BigInt.GreatestCommonDivisor((v - 2 + n) % n, n);
        if (g > 1 && g < n) return g;
        if (g == n) break;
      }
    }
    return BigInteger.One;
  }
}