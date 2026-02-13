using Factorization;
using System.Diagnostics;
using System.Numerics;

internal class Program
{
  private static List<Test> tests = [/*Test.PollardRhoTopBottom,*/ Test.PollardRhoStandard, Test.PollardRhoPowMod, Test.PollardRhoWithOneFoldingAndLimits];

  private static void Main(string[] args)
  {
    const int digitsCount = 20;

    BigInt n = BigInt.GenerateSemiPrime(digitsCount, out var p, out var q);
    Console.WriteLine($"n: {n}");
    var pM1Factors = BigInt.Factorize(p - 1);
    var qM1Factors = BigInt.Factorize(q - 1);
    Console.WriteLine($"p-1: {string.Join(", ", pM1Factors.Select(factor => factor.prime))}");
    Console.WriteLine($"q-1: {string.Join(", ", qM1Factors.Select(factor => factor.prime))}");
    foreach (var test in tests)
    {
      var watch = new Stopwatch();
      Console.WriteLine($"###### {test} ###########################");
      watch.Start();
      BigInt result = 0;
      BigInt iterations = 0;
      try
      {
        result = PerformTest(p, q, test, out iterations);
      }
      finally
      {
        Console.WriteLine($"Duration: {watch.ElapsedMilliseconds / 1000.0} s");
      }
      if (0 < result)
      {
        Console.WriteLine($"{result} x {n / result}");
        Console.WriteLine($"Iterations: {iterations}");
      }
    }
  }

  static BigInt PerformTest(BigInt p, BigInt q, Test test, out BigInt iterations)
  {
    BigInt n = p * q;
    iterations = 0;
    return test switch
    {
      Test.PollardRho => PollardRho.Factorize(n),
      Test.CFRAC => CFRAC.Factorize(n),
      Test.PollardRhoChebyshev => PollardRhoChebyshev(n, out iterations),
      Test.PollardRhoStandard => PollardRhoStandard(n, out iterations),
      Test.PollardRhoSelfReferential => PollardRhoSelfReferential(n, out iterations),
      Test.PollardRhoWithMultipleFoldings => PollardRhoWithMultipleFoldings(n, out iterations),
      Test.PollardRhoWithOneFoldingAndLimits => PollardRhoWithOneFoldingAndLimits(n, out iterations),
      Test.PollardRhoTopBottom => PollardRhoTopBottom(n, out iterations),
      Test.PollardRhoTopBottomInverse => PollardRhoTopBottomInverse(n, out iterations),
      Test.PollardRhoPowMod => PollardRhoPowMod(n, out iterations),
      Test.PowerModBenchMarks => PowerModBenchmarks(),
      Test.ExperimentPplusQNplusOne => ExperimentPplusQNplusOne(p, q),
      _ => 1
    };
  }

  static BigInteger PollardRhoChebyshev(BigInt n, out BigInt iterations)
  {
    iterations = 0;
    var d = n.Root(4);
    for (var i = 0; i < 100; ++i)
    {
      Console.WriteLine(i);
      var powers = new Dictionary<BigInt, BigInt>();
      var c = 1;
      BigInt a = 2 + i * 4;
      while (c < d)
      {
        ++iterations;
        ++c;
        BigInt b = BigInt.CalculateChebyshev(a, a + 1, n);
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

  static BigInteger PollardRhoStandard(BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    BigInt b = 2;
    while (true)
    {
      ++iterations;
      a = a.PowerMod(b, n);
      BigInt d = BigInt.GreatestCommonDivisor(a - 1, n);
      if (d != 1)
      {
        return d;
      }
      else if (d == n)
      {
        return n;
      }
      b += 1;
    }
  }

  static BigInteger PollardRhoSelfReferential(BigInt n, out BigInt iterations)
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
      else if (d == n)
      {
        return n;
      }
    }
  }

  static BigInteger PollardRhoPowMod(BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    BigInt e = a;
    while (true)
    {
      ++iterations;
      BigInt r = 1;
      BigInt p = a;
      while (true)
      {
        if (!e.IsEven)
        {
          r = r.MulMod(p, n);
          BigInt d = BigInt.GreatestCommonDivisor(r - 1, n);
          if (d != 1)
          {
            return d;
          }
          else if (d == n)
          {
            return n;
          }
        }
        e >>= 1;
        if (e.IsZero)
        {
          break;
        }
        p = p.SquareMod(n);
      }
      a = r;
      e = a;
    }
  }

  static BigInteger PollardRhoWithMultipleFoldings(BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    BigInt b = n.SquareRoot().Even;
    BigInt.InitializeDifferences(b, 2, out var differences);
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

  static BigInteger PollardRhoWithOneFoldingAndLimits(BigInt n, out BigInt iterations)
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

  static BigInteger PollardRhoTopBottom(BigInt n, out BigInt iterations)
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
      else if (d == n)
      {
        return n;
      }
      --b;
    }
    return 1;
  }

  static BigInteger PollardRhoTopBottomInverse(BigInt n, out BigInt iterations)
  {
    iterations = 0;
    BigInt a = 2;
    BigInt b = 2;
    while (true)
    {
      ++iterations;
      a = a.PowerMod(n, n).MulMod(a.PowerMod(b, n).InvMod(n)!.Value, n);
      BigInt e = BigInt.GreatestCommonDivisor(a - 1, n);
      if (e != 1)
      {
        return e;
      }
      else if (e == n)
      {
        return n;
      }
      ++b;
    }
  }

  static BigInt PowModMontgomery(BigInt b, BigInt e, BigInt n)
  {
    Montgomery m = new(n, e);
    BigInteger r = m.ToMontgomery(BigInteger.One);
    BigInteger p = m.ToMontgomery(b);
    for (int i = 0; i < m.ExponentBitLength; ++i)
    {
      if (m.IsBitSet(i))
      {
        r = m.Multiply(r, p);
      }
      p = m.Square(p);
    }
    return m.FromMontgomery(r);
  }

  static BigInt PowModStandard(BigInt b, BigInt e, BigInt n)
  {
    BigInt r = 1;
    BigInt p = b;
    while (true)
    {
      if (!e.IsEven)
      {
        r = r.MulMod(p, n);
      }
      e >>= 1;
      if (e.IsZero)
      {
        return r;
      }
      p = p.SquareMod(n);
    }
  }

  static BigInteger PowerModBenchmarks()
  {
    BigInt b = 3;
    BigInt e = BigInt.Parse("1000000000000000000");
    BigInt n = 1000001;
    int l = 100000;
    Stopwatch watch = new Stopwatch();
    BigInt result = 0;
    watch.Restart();
    for (var i = 0; i < l; ++i)
    {
      result = PowModMontgomery(b, e, n);
    }
    Console.WriteLine($"{watch.ElapsedMilliseconds / 1000.0} {result}");
    watch.Restart();
    for (var i = 0; i < l; ++i)
    {
      result = BigInt.ModPowSlidingWindow(b, e, n);
    }
    Console.WriteLine($"{watch.ElapsedMilliseconds / 1000.0} {result}");
    watch.Restart();
    for (var i = 0; i < l; ++i)
    {
      result = PowModStandard(b, e, n);
    }
    Console.WriteLine($"{watch.ElapsedMilliseconds / 1000.0} {result}");
    watch.Restart();
    for (var i = 0; i < l; ++i)
    {
      result = b.PowerMod(e, n);
    }
    Console.WriteLine($"{watch.ElapsedMilliseconds / 1000.0} {result}");
    return 0;
  }

  static BigInteger ExperimentPplusQNplusOne(BigInt p, BigInt q)
  {
    BigInt n = p * q;
    BigInt aLimit = BigInt.Min(BigInt.Min(p, q), n.Root(3));
    BigInt gcdLimit = n.Root(6);
    BigInt biggestGCD = 2;
    BigInt biggestDistance = 2;
    BigInt biggestPower = 2;
    Console.WriteLine($"a-limit: {aLimit} gcd-limit: {gcdLimit}");
    for (BigInt a = 2; a < aLimit; ++a)
    {
      BigInt? aOrderP = a.Order(p);
      BigInt? aOrderQ = a.Order(q);
      if (aOrderP != null && aOrderQ != null)
      {
        BigInt distance = BigInt.LeastCommonMultiple(aOrderP.Value, aOrderQ.Value);
        BigInt power = a.PowerMod(n - 1, n);
        BigInt gcd = BigInt.GreatestCommonDivisor(distance, power);
        if (biggestGCD < gcd)
        {
          biggestGCD = gcd;
          biggestDistance = distance;
          biggestPower = power;
        }
      }
    }
    Console.WriteLine($"biggest gcd: {biggestGCD} gcd factors: {string.Join(" ", BigInt.Factorize(biggestGCD))}");
    Console.WriteLine($"biggest gcd-distance: {biggestDistance} biggest distance factors: {string.Join(" ", BigInt.Factorize(biggestDistance))}");
    Console.WriteLine($"biggest power: {biggestPower} biggest power factors: {string.Join(" ", BigInt.Factorize(biggestPower))}");
    return 0;
  }
}