using System.Numerics;
using System.Reflection;

namespace Factorization;

internal static class PollardRhoExtensions
{
  public static BigInteger PollardRho(this BigInt n, Sequence sequence, out BigInt iterations)
  {
    Console.WriteLine(MethodBase.GetCurrentMethod()?.Name);
    iterations = 0;
    BigInt limit = n.Power(2, 5);
    Sequence sequence1 = sequence.Clone;
    for (int i = 0; i < limit; ++i)
    {
      ++iterations;
      BigInt a = sequence.Next();
      BigInt b = sequence1.Next(1);
      BigInt c = BigInt.GreatestCommonDivisor(BigInt.Abs(a - b), n);
      if (c != 1)
      {
        return c == n ? n : c;
      }
    }
    return 1;
  }

  public static BigInt PollardRhoCombined(this BigInt n)
  {
    BigInt function1Limit = n.Power(1, 9);
    BigInt function2Limit = n.Power(2, 9);
    BigInt limit = n.Power(2, 5);
    Func<BigInt, BigInt, BigInt> function = static (a0, n0) => BigInt.Chebyshev(a0, a0 + 2, n0);
    Console.WriteLine("Using funktion 1");
    BigInt a = 2;
    BigInt b = 2;
    for (int count = 0; count < limit; ++count)
    {
      if (count == function1Limit)
      {
        Console.WriteLine("Using function 2");
        function = static (a0, n0) => a0.PowerMod(a0, n0);
      }
      else if (count == function2Limit)
      {
        Console.WriteLine("Using function 3");
        function = static (a0, n0) => a0.PowerMod(2, n0) + 1;
      }
      a = function(a, n);
      b = function(function(b, n), n);
      BigInt gcd = BigInt.GreatestCommonDivisor(a - b, n);
      if (gcd != 1)
      {
        return gcd == n ? n : gcd;
      }
    }
    return 1;
  }

  public static BigInteger PollardRhoPowerMod(this BigInt n, out BigInt iterations)
  {
    iterations = 0;
    Action<BigInt.PowModState> resetAction = static state =>
    {
      state.b = state.r;
      state.e = state.b;
      state.r = 1;
    };
    BigInt.PowModState stepA = new(60, 60, n, resetAction);
    BigInt.PowModState stepB = new(60, 60, n, resetAction);
    while (true)
    {
      ++iterations;
      stepA.Step();
      stepA.Step();
      stepB.Step();
      BigInt d = BigInt.GreatestCommonDivisor(stepA.r - stepB.r, n);
      if (d != 1 && d != n)
      {
        return d;
      }
    }
  }

  public static BigInteger PollardRhoChebyshev(this BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    BigInt b = 2;
    while (true)
    {
      ++iterations;
      a = BigInt.Chebyshev(a, a, n);
      a = BigInt.Chebyshev(a, a, n);
      b = BigInt.Chebyshev(b, b, n);
      BigInt d = BigInt.GreatestCommonDivisor(a - b, n);
      if (d != 1)
      {
        return d == n ? n : d;
      }
    }
  }
}