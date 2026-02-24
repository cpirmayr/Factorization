namespace Factorization;

internal abstract class Sequence
{
  public Sequence Clone => (Sequence) MemberwiseClone();

  public abstract BigInt Next();

  public BigInt Next(int skip)
  {
    for (int i = 0; i < skip; ++i)
    {
      Next();
    }
    return Next();
  }

  public Sequence Skip(int skip)
  {
    for (int i = 0; i < skip; ++i)
    {
      Next();
    }
    return this;
  }
}

internal class PowModSelfReferential(BigInt a, BigInt n) : Sequence
{
  private BigInt a = a;

  public override BigInt Next()
  {
    BigInt result = a;
    a = a.PowerMod(a, n);
    return result;
  }
}

internal class SquarePlusC(BigInt a, BigInt c, BigInt n) : Sequence
{
  private BigInt a = a;

  public override BigInt Next()
  {
    BigInt result = a;
    a = a.Square().AddMod(c, n);
    return result;
  }
}

internal class LucasU(BigInt p, BigInt q, BigInt n) : Sequence
{
  private BigInt u0 = 0;
  private BigInt u1 = 1;
  private int state = 2;

  public override BigInt Next()
  {
    switch (state)
    {
      case 2:
        --state;
        return u0;
      case 1:
        --state;
        return u1;
    }
    BigInt result = (p * u1 - q * u0) % n;
    u0 = u1;
    u1 = result;
    return result;
  }
}

internal class LucasV(BigInt p, BigInt q, BigInt n) : Sequence
{
  private readonly BigInt p = p;
  private BigInt v0 = 2;
  private BigInt v1 = p;
  private int state = 2;

  public override BigInt Next()
  {
    switch (state)
    {
      case 2:
        --state;
        return v0;
      case 1:
        --state;
        return v1;
    }
    BigInt result = (p * v1 - q * v0) % n;
    v0 = v1;
    v1 = result;
    return result;
  }
}