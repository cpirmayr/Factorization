using System.Numerics;

namespace Factorization;

internal static class PollardPm1Extensions
{
  public static BigInteger PollardPm1(this BigInt n, Sequence sequence, out BigInt iterations)
  {
    iterations = 0;
    BigInt limit = n.Power(2, 5);
    sequence.Next();
    sequence.Next();
    sequence.Next();
    BigInt a = sequence.Next();
    for (int i = 0; i < limit; ++i)
    {
      ++iterations;
      a = a.PowerMod(sequence.Next(), n);
      BigInt d = BigInt.GreatestCommonDivisor(a - 1, n);
      if (d != 1)
      {
        return d == n ? n : d;
      }
    }
    return 1;
  }

  public static BigInteger PollardPm1PowMod(this BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    BigInt e = a;
    BigInt r = 1;
    BigInt limit = n.Power(2, 5);
    while (iterations < limit)
    {
      if (!e.IsEven)
      {
        ++iterations;
        r = r.MulMod(a, n);
        BigInt d = BigInt.GreatestCommonDivisor(r - 1, n);
        if (d != 1)
        {
          return d == n ? n : d;
        }
      }
      e >>= 1;
      if (e.IsZero)
      {
        a = r;
        e = a;
        r = 1;
      }
      a = a.SquareMod(n);
    }
    return 0;
  }

  public static BigInt PollardPm1Reference(this BigInt n)
  {
    PollardPMinus1 pollard = new(n.Power(2, 5));
    PollardPMinus1.ComputeBound(n);
    BigInteger result = pollard.Factor(n);
    return result;
  }

  public static BigInteger PollardPm1Rho(this BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    BigInt b = 2;
    while (true)
    {
      ++iterations;
      a = a.PowerMod(a, n);
      a = a.PowerMod(a, n);
      b = b.PowerMod(a, n);
      BigInt d = BigInt.GreatestCommonDivisor(a - 1, n);
      if (d != 1)
      {
        return d == n ? n : d;
      }
      BigInt e = BigInt.GreatestCommonDivisor(BigInt.Abs(a - b), n);
      if (e != 1)
      {
        return e == n ? n : e;
      }
    }
  }

  public static BigInteger PollardPm1SelfReferential(this BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    while (true)
    {
      ++iterations;
      a = a.PowerMod(a, n);
      BigInt d = BigInt.GreatestCommonDivisor(a - 1, n);
      if (d != 1)
      {
        return d;
      }
      if (d == n)
      {
        return n;
      }
    }
  }

  public static BigInteger PollardPm1Standard(this BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    BigInt b = 2;
    BigInt limit = n.Root(3);
    while (iterations < limit)
    {
      ++iterations;
      a = a.PowerMod(b, n);
      BigInt d = BigInt.GreatestCommonDivisor(a - 1, n);
      if (d != 1)
      {
        return d == n ? n : d;
      }
      b += 1;
    }
    return 0;
  }

  public static BigInteger PollardPm1TopBottom(this BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt limit = n.Root(3);
    BigInt a = 2;
    BigInt b = n;
    for (int i = 0; i < limit; ++i)
    {
      ++iterations;
      a = a.PowerMod(b, n);
      BigInt d = BigInt.GreatestCommonDivisor(a - 1, n);
      if (d != 1)
      {
        return d;
      }
      if (d == n)
      {
        return n;
      }
      --b;
    }
    return 1;
  }

  public static BigInteger PollardPm1WithMultipleFoldings(this BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    BigInt b = n.SquareRoot().Even;
    BigInt.InitializeDifferences(b, 2, out List<BigInt> differences);
    while (true)
    {
      ++iterations;
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

  public static BigInteger PollardPm1WithOneFoldingAndLimits(this BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt limit = n.Root(3);
    BigInt b = 2;
    BigInt u = n.PowRoot(1, 3);
    BigInt l = n.PowRoot(1, 5);
    Console.WriteLine($"l: {l}");
    Console.WriteLine($"u: {u}");
    BigInt e = l * u;
    BigInt d = u - l - 1;
    for (int i = 0; i < limit; ++i)
    {
      ++iterations;
      b = b.PowerMod(e, n);
      BigInt r = BigInt.GreatestCommonDivisor(b - 1, n);
      if (r != 1)
      {
        return r == n ? n : r;
      }
      e += d;
      d -= 2;
    }
    return 1;
  }
}