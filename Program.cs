using System.Numerics;
using ConsoleApp2;

const int digitsCount = 35;
int factorizationMethod = 1;
if (args.Length == 1 && int.TryParse(args[0], out int method))
{
  factorizationMethod = method;
}
BigInteger n = BigIntegerHelpers.GenerateSemiPrime(digitsCount);
Console.WriteLine($"{n} =");
BigInteger result = 0;
switch (factorizationMethod)
{
  case 0:
    result = PollardRho.Factorize(n);
    break;
  case 1:
    result = ContinuedFractionFactorizer2.Factorize(n);
    break;
}
Console.WriteLine($"{result} x {n / result}");