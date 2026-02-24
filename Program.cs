using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Factorization;

internal class Program
{
  public class TestEntry(int digitsCount, BigInt n, Tests test, BigInt iterations, long ticks)
  {
    public int digitsCount = digitsCount;
    public BigInt iterations = iterations;
    public BigInt n = n;
    public Tests test = test;
    public long ticks = ticks;
  }

  public class TestResults : List<TestEntry>;

  private const int DigitsPasses = 1;
  private const int MinDigitsCount = 20;
  private const int NrOfTestPasses = 1;

  private static readonly List<Tests> Tests = [Factorization.Tests.CFRAC, Factorization.Tests.PollardPm1PowerMod];

  private static void FactorBaseExperiments()
  {
    Random random = new();
    BigInt n = BigInt.GenerateSemiPrime(MinDigitsCount, out BigInt _, out BigInt _, random.Next());
    Console.WriteLine($"n: {n}");
    BigInt primesLimit = n.Root(8);
    List<BigInt> primes = Enumerable.Range(2, (int) primesLimit).Where(static item => BigInt.IsProbablePrime(item)).Select(static item => (BigInt) item).ToList();
    List<int[]> relations = [];
    HashSet<BigInt> usedPrimes = [];
    foreach ((BigInt p, BigInt q) in SqrtContinuedFraction.ConvergentsOfSqrt(n, int.MaxValue))
    {
      BigInt d = BigInt.Abs(p * p - n * q * q); // 2744102684715;
      int[] relation = Enumerable.Repeat(0, primes.Count).ToArray();
      List<BigInteger> factors = [];
      // foreach (int prime in primes)
      for (int primesIndex = 0; primesIndex < primes.Count; ++primesIndex)
      {
        BigInteger prime = primes[primesIndex];
        bool factorUsed = false;
        BigInt newD = BigInt.DivRem(d, prime, out BigInt remainder);
        while (remainder == 0)
        {
          relation[primesIndex]++;
          usedPrimes.Add(prime);
          d = newD;
          newD = BigInt.DivRem(d, prime, out remainder);
          factorUsed = true;
        }
        if (factorUsed)
        {
          factors.Add(prime);
        }
      }
      if (d == 1)
      {
        relations.Add(relation);
      }
      if (1 < relations.Count && usedPrimes.Count < relations.Count && relations.Count % 20 == 0)
      {
        BigInt square = BigInt.FindRootOfSquareFromFactorBase(relations, primes, out List<int> usedIndices, n);
        if (1 < square)
        {
          Console.WriteLine($"Square: {square}");
          return;
        }
      }
    }
  }

  private static void Main()
  {
    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    if (PerformFactorizations)
    {
      Random random = new();
      TestResults? testResults = PerformTests ? [] : null;
      const int maxDigitsCount = MinDigitsCount + DigitsPasses - 1;
      for (int digitsCount = MinDigitsCount; digitsCount <= maxDigitsCount; ++digitsCount)
      {
        for (int i = 0; i < NrOfTestPasses; ++i)
        {
          Console.WriteLine($"========= Pass {i}");
          BigInt n = BigInt.GenerateSemiPrime(digitsCount, out BigInt p, out BigInt q, random.Next());
          Console.WriteLine($"n: {n}");
          List<(BigInt prime, int exponent)> pM1Factors = BigInt.Factorize(p - 1);
          List<(BigInt prime, int exponent)> qM1Factors = BigInt.Factorize(q - 1);
          Console.WriteLine($"p-1: {string.Join(", ", pM1Factors.Select(factor => factor.prime))}");
          Console.WriteLine($"q-1: {string.Join(", ", qM1Factors.Select(factor => factor.prime))}");
          PerformPQTests(digitsCount, p, q, ref testResults);
        }
      }
      if (PerformTests && testResults != null)
      {
        foreach (Tests test in Tests)
        {
          TestEntry[] testEntries = testResults.Where(item => item.test == test).ToArray();
          List<string> lines = [];
          for (int j = MinDigitsCount; j <= maxDigitsCount; ++j)
          {
            TestEntry[] digitsEntries = testEntries.Where(item => item.digitsCount == j).ToArray();
            BigInteger[] numbers = digitsEntries.Select(item => (BigInteger) item.n).ToArray();
            long[] ticks = digitsEntries.Select(item => item.ticks).ToArray();
            BigInteger medianNumber = numbers.Median();
            long medianTick = ticks.Median();
            lines.Add($"{medianNumber};{((double) medianTick / Stopwatch.Frequency).ToPlain()}");
          }
          File.WriteAllLines(@"C:\Usr\" + test + ".txt", lines);
        }
      }
    }
    else
    {
      FactorBaseExperiments();
    }
  }

  private static void PerformPQTests(int digitsCount, BigInt p, BigInt q, ref TestResults? testResults)
  {
    BigInt n = p * q;
    foreach (Tests test in Tests)
    {
      Stopwatch watch = new();
      Console.WriteLine($"--------- {test}");
      watch.Start();
      BigInt result;
      BigInt iterations;
      long ticks;
      try
      {
        result = PerformTestForPQ(p, q, test, out iterations);
      }
      finally
      {
        ticks = watch.ElapsedTicks;
      }
      if (1 < result && result < n)
      {
        Console.WriteLine($"{result} x {n / result}");
        Console.WriteLine($"Iterations: {iterations}");
        Console.WriteLine($"Duration: {Math.Round((double) ticks / Stopwatch.Frequency, 3)} s");
        testResults?.Add(new TestEntry(digitsCount, n, test, iterations, ticks));
      }
    }
  }

  private static BigInt PerformTestForPQ(BigInt p, BigInt q, Tests test, out BigInt iterations)
  {
    BigInt n = p * q;
    iterations = 0;
    return test switch
    {
      Factorization.Tests.CFRAC => CFRAC.Factorize(n),
      Factorization.Tests.PollardPm1PowerMod => n.PollardPm1PowMod(out iterations),
      Factorization.Tests.PollardPm1Reference => n.PollardPm1Reference(),
      Factorization.Tests.PollardPm1Rho => n.PollardPm1Rho(out iterations),
      Factorization.Tests.PollardPm1SelfReferential => n.PollardPm1SelfReferential(out iterations),
      Factorization.Tests.PollardPm1Standard => n.PollardPm1Standard(out iterations),
      Factorization.Tests.PollardPm1TopBottom => n.PollardPm1TopBottom(out iterations),
      Factorization.Tests.PollardPm1WithMultipleFoldings => n.PollardPm1WithMultipleFoldings(out iterations),
      Factorization.Tests.PollardPm1WithOneFoldingAndLimits => n.PollardPm1WithOneFoldingAndLimits(out iterations),
      Factorization.Tests.PollardRho => n.PollardRho(new SquarePlusC(2, 1, n), out iterations),
      Factorization.Tests.PollardRhoChebyshev => n.PollardRhoChebyshev(out iterations),
      Factorization.Tests.PollardRhoCombined => n.PollardRhoCombined(),
      Factorization.Tests.PollardRhoPowerMod => n.PollardRhoPowerMod(out iterations),
      Factorization.Tests.ShanksSqufof => n.ShanksSqufof(),
      Factorization.Tests.WilliamsPp1 => n.WilliamsPPlus1(),
      _ => 1
    };
  }

  private static bool PerformFactorizations => false;

  private static bool PerformTests => false;
}