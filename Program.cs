using System.Numerics;
using ConsoleApp2;

const int digitsCount = 45;
// BigInteger n = BigInteger.Parse("56772286057224175134407894536228864081"); // BigIntegerHelpers.GenerateSemiPrime(digitsCount);
BigInteger n = BigIntegerHelpers.GenerateSemiPrime(digitsCount);
Console.WriteLine($"{n} = ");
/*
Console.WriteLine($"{n}");
PollardRho.Factorize(n);
*/

BigInteger result = ContinuedFractionFactorizer2.Factorize(n);
Console.WriteLine($"{result} x {n / result}");
/*
BigInteger result = MpqsSolver.Factor(n);
Console.WriteLine($"{result} x {n / result}");
*/
/*
foreach ((BigInteger Numerator, BigInteger Denominator) convergent in BigIntegerHelpers.SqrtConvergents(n, 1000000))
{
  // Console.WriteLine($"{convergent.Numerator} / {convergent.Denominator}");
  BigInteger k = convergent.Numerator * convergent.Numerator - n * convergent.Denominator * convergent.Denominator;
  // if (BigIntegerHelpers.IsSquare(k))
  {
    Console.WriteLine($"{k} {BigIntegerHelpers.IsSquare(k)}");
  }
}
*/