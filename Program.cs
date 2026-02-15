using System.Diagnostics;
using System.Numerics;

namespace Factorization
{
  internal class Program
  {
    const int MinDigitsCount = 15;
    const int MaxDigitsCount = 20;
    const int nrOfTestPasses = 10;

    private static readonly List<Tests> tests = [Tests.PollardPm1PowMod, Tests.PollardPm1Standard];

    private static void Main()
    {
      Random random = new(1234);
      TestResults testResults = [];
      for (var digitsCount = MinDigitsCount; digitsCount <= MaxDigitsCount; ++digitsCount)
      {
        for (var i = 0; i < nrOfTestPasses; ++i)
        {
          Console.WriteLine($"========= {i}");
          BigInt n = BigInt.GenerateSemiPrime(digitsCount, out var p, out var q, random.Next());
          Console.WriteLine($"n: {n}");
          var pM1Factors = BigInt.Factorize(p - 1);
          var qM1Factors = BigInt.Factorize(q - 1);
          Console.WriteLine($"p-1: {string.Join(", ", pM1Factors.Select(factor => factor.prime))}");
          Console.WriteLine($"q-1: {string.Join(", ", qM1Factors.Select(factor => factor.prime))}");
          PerformTestsForPQ(digitsCount, p, q, ref testResults);
        }
        foreach (var testResult in testResults)
        {
          Console.WriteLine(testResult.Key.ToString());
          Console.WriteLine(testResult.Value.testEntries.Select(item => item.seconds).Median());
        }
      }
      var a = 0;
    }

    private static void PerformTestsForPQ(int digitsCount, BigInt p, BigInt q, ref TestResults testResults)
    {
      BigInt n = p * q;
      foreach (var test in tests)
      {
        if (!testResults.TryGetValue(test, out var testResult))
        {
          testResult = new();
          testResults.Add(test, testResult);
        }
        var watch = new Stopwatch();
        Console.WriteLine($"--------- {test}");
        watch.Start();
        BigInt result = 0;
        BigInt iterations = 0;
        double seconds = 0.0;
        try
        {
          result = PerformTestForPQ(p, q, test, out iterations);
        }
        finally
        {
          seconds = watch.ElapsedMilliseconds / 1000.0;
        }
        if (0 < result)
        {
          Console.WriteLine($"{result} x {n / result}");
          Console.WriteLine($"Iterations: {iterations}");
          Console.WriteLine($"Duration: {seconds} s");
          testResult.testEntries.Add(new TestEntry(digitsCount, n, iterations, seconds));
        }
      }
    }

    static BigInt PerformTestForPQ(BigInt p, BigInt q, Tests test, out BigInt iterations)
    {
      BigInt n = p * q;
      iterations = 0;
      return test switch
      {
        Tests.PollardRho => PollardRho.Factorize(n),
        Tests.CFRAC => CFRAC.Factorize(n),
        Tests.PollardRhoChebyshev => PollardRhoChebyshev(n, out iterations),
        Tests.PollardPm1Standard => PollardPm1Standard(n, out iterations),
        Tests.PollardPm1SelfReferential => PollardPm1SelfReferential(n, out iterations),
        Tests.PollardPm1WithMultipleFoldings => PollardPm1WithMultipleFoldings(n, out iterations),
        Tests.PollardPm1WithOneFoldingAndLimits => PollardPm1WithOneFoldingAndLimits(n, out iterations),
        Tests.PollardPm1TopBottom => PollardPm1TopBottom(n, out iterations),
        Tests.PollardPm1TopBottomInverse => PollardPm1TopBottomInverse(n, out iterations),
        Tests.PollardPm1PowMod => PollardPm1PowMod(n, out iterations),
        Tests.PollardRhoPowMod => PollardRhoPowMod(n, out iterations),
        _ => 1
      };
    }

    static BigInteger PollardRhoChebyshev(BigInt n, out BigInt iterations)
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
      var resetAction = (BigInt.PowModState state) => { state.b = state.r; state.e = state.b; state.r = 1; };
      BigInt.PowModState stepA = new(3, 3, n, resetAction);
      BigInt.PowModState stepB = new(3, 3, n, resetAction);
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

    static BigInteger PollardPm1Standard(BigInt n, out BigInt iterations)
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

    static BigInteger PollardPm1SelfReferential(BigInt n, out BigInt iterations)
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

    static BigInteger PollardPm1PowMod(BigInt n, out BigInt iterations)
    {
      iterations = 0;
      BigInt a = 2;
      BigInt e = a;
      BigInt r = 1;
      BigInt t = 1;
      BigInt l = n * 10;
      BigInt limit = n.Root(3);
      while (iterations < limit)
      {
        if (!e.IsEven)
        {
          ++iterations;
          r = r.MulMod(a, n);
          t *= r;
          if (l < t)
          {
            BigInt d = BigInt.GreatestCommonDivisor(r - 1, n);
            if (d != 1)
            {
              return d == n ? n : d;
            }
            t = 1;
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

    static BigInteger PollardPm1WithMultipleFoldings(BigInt n, out BigInt iterations)
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

    static BigInteger PollardPm1WithOneFoldingAndLimits(BigInt n, out BigInt iterations)
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

    static BigInteger PollardPm1TopBottom(BigInt n, out BigInt iterations)
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

    static BigInteger PollardPm1TopBottomInverse(BigInt n, out BigInt iterations)
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
  }

  public class TestEntry(int digitsCount, BigInt n, BigInt iterations, double seconds)
  {
    public int digitsCount = digitsCount;
    public BigInt n = n;
    public BigInt iterations = iterations;
    public double seconds = seconds;
  }

  public class TestResult
  {
    public List<TestEntry> testEntries = [];
  }

  public class TestResults : Dictionary<Tests, TestResult>
  {
  }
}