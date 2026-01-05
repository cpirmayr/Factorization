using System.Numerics;

namespace ConsoleApp2;

public static class PollardRho
{
  public static BigInteger Factorize(BigInteger n)
  {
    int function1Limit = (int) Math.Pow(10, Math.Round(GetDecimalPlaces(n) / 5.0));
    int function2Limit = 2 * function1Limit;
    BigInteger gcd;
    int count = 0;
    Func<BigInteger, BigInteger, BigInteger> function = NextElement1;
    Console.WriteLine("Using funktion 1");
    BigInteger a = 2;
    BigInteger b = 2;
    do
    {
      ++count;
      if (count == function1Limit)
      {
        function = NextElement2;
        Console.WriteLine("Using function 2");
      }
      else if (count == function2Limit)
      {
        function = NextElement3;
        Console.WriteLine("Using function 3");
      }
      a = function(a, n);
      b = function(function(b, n), n);
      gcd = BigInteger.GreatestCommonDivisor(a - b, n);
      if (gcd != BigInteger.One && gcd != n)
      {
        break;
      }
    } while (true);
    return gcd;
  }

  private static int GetDecimalPlaces(BigInteger value)
  {
    return value.IsZero ? 1 : (int) Math.Floor(BigInteger.Log10(BigInteger.Abs(value)) + 1);
  }

  private static BigInteger NextElement1(BigInteger a, BigInteger n)
  {
    return BigIntegerHelpers.CalculateChebyshev(a, a + 2, n);
  }

  private static BigInteger NextElement2(BigInteger a, BigInteger n)
  {
    return BigInteger.ModPow(a, a, n);
  }

  private static BigInteger NextElement3(BigInteger a, BigInteger n)
  {
    return BigInteger.ModPow(a, 2, n) + 1;
  }
}