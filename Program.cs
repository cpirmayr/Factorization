using System.Numerics;
using Factorization;

const int digitsCount = 30;
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
    result = CFRAC.Factorize(n);
    break;
}
Console.WriteLine($"{result} x {n / result}");

