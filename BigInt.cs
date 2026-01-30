using System;
using System.Numerics;
using System.Globalization;

namespace Factorization;

/// <summary>
/// Wrapper for BigInteger that behaves like built-in numeric types
/// </summary>
public struct BigInt : IComparable, IComparable<BigInt>, IEquatable<BigInt>, IFormattable
{
  private readonly BigInteger _value;

  #region Constructors

  public BigInt(int value) => _value = new BigInteger(value);
  public BigInt(long value) => _value = new BigInteger(value);
  public BigInt(uint value) => _value = new BigInteger(value);
  public BigInt(ulong value) => _value = new BigInteger(value);
  public BigInt(double value) => _value = new BigInteger(value);
  public BigInt(decimal value) => _value = new BigInteger(value);
  public BigInt(byte[] value) => _value = new BigInteger(value);
  public BigInt(string value) => _value = BigInteger.Parse(value);
  public BigInt(BigInteger value) => _value = value;

  #endregion

  #region Static Properties

  public static BigInt Zero => new BigInt(BigInteger.Zero);
  public static BigInt One => new BigInt(BigInteger.One);
  public static BigInt MinusOne => new BigInt(BigInteger.MinusOne);

  #endregion

  #region Properties

  public bool IsZero => _value.IsZero;
  public bool IsOne => _value.IsOne;
  public bool IsEven => _value.IsEven;
  public bool IsPowerOfTwo => _value.IsPowerOfTwo;
  public int Sign => _value.Sign;

  #endregion

  #region Arithmetic Operators

  public static BigInt operator +(BigInt left, BigInt right) => new BigInt(left._value + right._value);
  public static BigInt operator -(BigInt left, BigInt right) => new BigInt(left._value - right._value);
  public static BigInt operator *(BigInt left, BigInt right) => new BigInt(left._value * right._value);
  public static BigInt operator /(BigInt left, BigInt right) => new BigInt(left._value / right._value);
  public static BigInt operator %(BigInt left, BigInt right) => new BigInt(left._value % right._value);
  public static BigInt operator -(BigInt value) => new BigInt(-value._value);
  public static BigInt operator +(BigInt value) => value;
  public static BigInt operator ++(BigInt value) => new BigInt(value._value + 1);
  public static BigInt operator --(BigInt value) => new BigInt(value._value - 1);

  #endregion

  #region Bitwise Operators

  public static BigInt operator &(BigInt left, BigInt right) => new BigInt(left._value & right._value);
  public static BigInt operator |(BigInt left, BigInt right) => new BigInt(left._value | right._value);
  public static BigInt operator ^(BigInt left, BigInt right) => new BigInt(left._value ^ right._value);
  public static BigInt operator ~(BigInt value) => new BigInt(~value._value);
  public static BigInt operator <<(BigInt value, int shift) => new BigInt(value._value << shift);
  public static BigInt operator >>(BigInt value, int shift) => new BigInt(value._value >> shift);

  #endregion

  #region Comparison Operators

  public static bool operator ==(BigInt left, BigInt right) => left._value == right._value;
  public static bool operator !=(BigInt left, BigInt right) => left._value != right._value;
  public static bool operator <(BigInt left, BigInt right) => left._value < right._value;
  public static bool operator >(BigInt left, BigInt right) => left._value > right._value;
  public static bool operator <=(BigInt left, BigInt right) => left._value <= right._value;
  public static bool operator >=(BigInt left, BigInt right) => left._value >= right._value;

  #endregion

  #region Implicit Conversions from built-in types

  public static implicit operator BigInt(byte value) => new BigInt(value);
  public static implicit operator BigInt(sbyte value) => new BigInt(value);
  public static implicit operator BigInt(short value) => new BigInt(value);
  public static implicit operator BigInt(ushort value) => new BigInt(value);
  public static implicit operator BigInt(int value) => new BigInt(value);
  public static implicit operator BigInt(uint value) => new BigInt(value);
  public static implicit operator BigInt(long value) => new BigInt(value);
  public static implicit operator BigInt(ulong value) => new BigInt(value);
  public static implicit operator BigInt(BigInteger value) => new BigInt(value);

  #endregion

  #region Explicit Conversions from BigInt

  public static explicit operator byte(BigInt value) => (byte) value._value;
  public static explicit operator sbyte(BigInt value) => (sbyte) value._value;
  public static explicit operator short(BigInt value) => (short) value._value;
  public static explicit operator ushort(BigInt value) => (ushort) value._value;
  public static explicit operator int(BigInt value) => (int) value._value;
  public static explicit operator uint(BigInt value) => (uint) value._value;
  public static explicit operator long(BigInt value) => (long) value._value;
  public static explicit operator ulong(BigInt value) => (ulong) value._value;
  public static explicit operator float(BigInt value) => (float) value._value;
  public static explicit operator double(BigInt value) => (double) value._value;
  public static explicit operator decimal(BigInt value) => (decimal) value._value;

  #endregion

  #region Implicit Conversions to BigInteger

  public static implicit operator BigInteger(BigInt value) => value._value;

  #endregion

  #region Static Methods

  public static BigInt GreatestCommonDivisor(BigInt left, BigInt right) => new BigInt(BigInteger.GreatestCommonDivisor(left._value, right._value));
  public static BigInt Min(BigInt left, BigInt right) => new BigInt(BigInteger.Min(left._value, right._value));
  public static BigInt Max(BigInt left, BigInt right) => new BigInt(BigInteger.Max(left._value, right._value));
  public static BigInt DivRem(BigInt dividend, BigInt divisor, out BigInt remainder)
  {
    var result = BigInteger.DivRem(dividend._value, divisor._value, out var rem);
    remainder = new BigInt(rem);
    return new BigInt(result);
  }

  public static BigInt Parse(string s) => new BigInt(BigInteger.Parse(s));
  public static BigInt Parse(string s, NumberStyles style) => new BigInt(BigInteger.Parse(s, style));
  public static BigInt Parse(string s, IFormatProvider provider) => new BigInt(BigInteger.Parse(s, provider));
  public static BigInt Parse(string s, NumberStyles style, IFormatProvider provider) =>
      new BigInt(BigInteger.Parse(s, style, provider));

  public static bool TryParse(string s, out BigInt result)
  {
    if (BigInteger.TryParse(s, out var value))
    {
      result = new BigInt(value);
      return true;
    }
    result = Zero;
    return false;
  }

  /// <summary>
  /// Attempts to convert the specified string representation of a number to its equivalent <see cref="BigInt"/> value,
  /// and returns a value that indicates whether the conversion succeeded.
  /// </summary>
  /// <remarks>This method does not throw an exception if the conversion fails. If <paramref name="s"/> is not
  /// in a valid format or represents a number outside the range of <see cref="BigInt"/>, <paramref name="result"/> is
  /// set to zero and the method returns false.</remarks>
  /// <param name="s">The string containing the number to convert.</param>
  /// <param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s"/>.</param>
  /// <param name="provider">An object that supplies culture-specific formatting information about <paramref name="s"/>.</param>
  /// <param name="result">When this method returns, contains the <see cref="BigInt"/> value equivalent to the number contained in <paramref
  /// name="s"/>, if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized.</param>
  /// <returns>true if <paramref name="s"/> was converted successfully; otherwise, false.</returns>
  public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out BigInt result)
  {
    if (BigInteger.TryParse(s, style, provider, out var value))
    {
      result = new BigInt(value);
      return true;
    }
    result = Zero;
    return false;
  }

  #endregion

  #region Interface Implementations

  public int CompareTo(object? obj)
  {
    if (obj == null) return 1;
    if (obj is BigInt other) return _value.CompareTo(other._value);
    throw new ArgumentException("Object must be of type BigInt");
  }

  public int CompareTo(BigInt other) => _value.CompareTo(other._value);

  public bool Equals(BigInt other) => _value.Equals(other._value);

  public override bool Equals(object? obj) => obj is BigInt other && Equals(other);

  public override int GetHashCode() => _value.GetHashCode();

  public override string ToString() => _value.ToString();

  public string ToString(string format) => _value.ToString(format);

  public string ToString(IFormatProvider provider) => _value.ToString(provider);

  public string ToString(string? format, IFormatProvider? provider) => _value.ToString(format, provider);



  #endregion

  #region Additional Methods

  public byte[] ToByteArray() => _value.ToByteArray();

  public BigInt Negate() => new BigInt(-_value);

  public BigInt Square() => this * this;

  /// <summary>
  /// Returns the absolute value of this BigInt
  /// </summary>
  public BigInt Abs() => new BigInt(BigInteger.Abs(_value));

  /// <summary>
  /// Raises this BigInt to the specified exponent
  /// </summary>
  public BigInt Pow(int exponent) => new BigInt(BigInteger.Pow(_value, exponent));

  /// <summary>
  /// Computes the integer square root (floor(√this))
  /// Uses Newton's method for efficient computation
  /// </summary>
  public BigInt SquareRoot()
  {
    if (_value < 0)
      throw new ArgumentException("Cannot compute square root of negative number");

    if (_value == 0 || _value == 1)
      return new BigInt(_value);

    // Newton's method: x(n+1) = (x(n) + N/x(n)) / 2
    BigInteger n = _value;
    BigInteger x = n / 2; // Initial guess

    while (true)
    {
      BigInteger nextX = (x + n / x) / 2;

      if (nextX >= x)
        return new BigInt(x);

      x = nextX;
    }
  }

  public BigInt Even => _value.IsEven ? _value : _value + 1;

  /// <summary>
  /// Raises the current value to the specified power and then computes the specified root of the result.
  /// </summary>
  /// <param name="a">The exponent to which the current value is raised. Must be non-negative.</param>
  /// <param name="b">The degree of the root to compute from the powered value. Must be greater than zero.</param>
  /// <returns>A new BigInt representing the b-th root of the current value raised to the a-th power.</returns>
  public BigInt PowRoot(int a, int b) => Pow(a).Root(b);

  /// <summary>
  /// Computes the integer nth root (floor(ⁿ√this))
  /// Uses Newton's method for efficient computation
  /// </summary>
  /// <param name="n">The root degree (must be positive)</param>
  /// <returns>The integer nth root of this BigInt</returns>
  public BigInt Root(int n)
  {
    if (n <= 0)
      throw new ArgumentException("Root degree must be positive", nameof(n));

    if (_value == 0 || _value == 1)
      return new BigInt(_value);

    // Handle negative numbers with odd roots
    if (_value < 0)
    {
      if (n % 2 == 0)
        throw new ArgumentException("Cannot compute even root of negative number", nameof(n));

      var negResult = new BigInt(-_value).Root(n);
      return new BigInt(-negResult._value);
    }

    // Newton's method: x(n+1) = ((n-1)*x(n) + N/x(n)^(n-1)) / n
    BigInteger num = _value;
    BigInteger x = num > 1 ? num / 2 : num;

    while (true)
    {
      BigInteger xPowNMinus1 = BigInteger.Pow(x, n - 1);
      BigInteger nextX = ((n - 1) * x + num / xPowNMinus1) / n;

      if (nextX >= x)
        return new BigInt(x);

      x = nextX;
    }
  }

  /// <summary>
  /// Performs modular exponentiation: (this^exponent) % modulus
  /// </summary>
  public BigInt PowerMod(BigInt exponent, BigInt modulus) =>
      new BigInt(BigInteger.ModPow(_value, exponent._value, modulus._value));

  /// <summary>
  /// Performs modular squaring: (this^2) % modulus
  /// </summary>
  public BigInt SquareMod(BigInt modulus) =>
      new BigInt(BigInteger.ModPow(_value, 2, modulus._value));

  /// <summary>
  /// Performs modular addition: (this + other) % modulus
  /// </summary>
  public BigInt AddMod(BigInt other, BigInt modulus)
  {
    var result = (_value + other._value) % modulus._value;
    return new BigInt(result < 0 ? result + modulus._value : result);
  }

  /// <summary>
  /// Performs modular subtraction: (this - other) % modulus
  /// </summary>
  public BigInt SubMod(BigInt other, BigInt modulus)
  {
    var result = (_value - other._value) % modulus._value;
    return new BigInt(result < 0 ? result + modulus._value : result);
  }

  /// <summary>
  /// Performs modular multiplication: (this * other) % modulus
  /// </summary>
  public BigInt MulMod(BigInt other, BigInt modulus)
  {
    var result = (_value * other._value) % modulus._value;
    return new BigInt(result < 0 ? result + modulus._value : result);
  }

  /// <summary>
  /// Performs modular division: (this * other^(-1)) % modulus
  /// Returns null if the inverse doesn't exist
  /// </summary>
  public BigInt? DivMod(BigInt other, BigInt modulus)
  {
    var inv = other.InvMod(modulus);
    if (inv == null) return null;
    return MulMod((BigInt) inv, modulus);
  }

  /// <summary>
  /// Computes the modular multiplicative inverse: this^(-1) mod modulus
  /// Returns null if the inverse doesn't exist (when gcd(this, modulus) != 1)
  /// Uses the Extended Euclidean Algorithm
  /// </summary>
  public BigInt? InvMod(BigInt modulus)
  {
    if (modulus <= 0)
      throw new ArgumentException("Modulus must be positive", nameof(modulus));

    var (gcd, x, _) = ExtendedGcd(_value, modulus._value);

    if (gcd != 1)
      return null; // Inverse doesn't exist

    // Ensure result is positive
    var result = x % modulus._value;
    return new BigInt(result < 0 ? result + modulus._value : result);
  }

  /// <summary>
  /// Computes the modular nth root: finds r such that r^n ≡ this (mod modulus)
  /// Returns null if no root exists
  /// Uses Tonelli-Shanks algorithm for n=2 with prime modulus, brute force otherwise
  /// </summary>
  public BigInt? RootMod(int n, BigInt modulus)
  {
    if (n <= 0)
      throw new ArgumentException("Root degree must be positive", nameof(n));

    if (modulus <= 0)
      throw new ArgumentException("Modulus must be positive", nameof(modulus));

    // Special case: 0^n = 0
    if (_value.IsZero)
      return Zero;

    // Special case: n = 1
    if (n == 1)
      return new BigInt(_value % modulus._value);

    // Special case: n = 2 (square root)
    if (n == 2)
      return SquareRootMod(modulus);

    // General case: try all possible roots (inefficient for large modulus)
    // This is a brute force approach - for production use, implement more efficient algorithms
    for (BigInteger i = 0; i < modulus._value; i++)
    {
      if (BigInteger.ModPow(i, n, modulus._value) == (_value % modulus._value))
        return new BigInt(i);
    }

    return null; // No root found
  }

  /// <summary>
  /// Computes modular square root using Tonelli-Shanks algorithm
  /// Works for prime modulus, returns null if no root exists
  /// </summary>
  private BigInt? SquareRootMod(BigInt modulus)
  {
    var a = _value % modulus._value;
    var p = modulus._value;

    // Check if a is a quadratic residue using Euler's criterion
    var legendreSymbol = BigInteger.ModPow(a, (p - 1) / 2, p);
    if (legendreSymbol != 1)
      return null; // No square root exists

    // Special case: p ≡ 3 (mod 4)
    if ((p % 4) == 3)
    {
      var r1 = BigInteger.ModPow(a, (p + 1) / 4, p);
      return new BigInt(r1);
    }

    // Tonelli-Shanks algorithm for p ≡ 1 (mod 4)
    // Find Q and S such that p - 1 = Q * 2^S with Q odd
    var s = 0;
    var q = p - 1;
    while ((q % 2) == 0)
    {
      q /= 2;
      s++;
    }

    // Find a quadratic non-residue z
    BigInteger z = 2;
    while (BigInteger.ModPow(z, (p - 1) / 2, p) != p - 1)
      z++;

    var m = s;
    var c = BigInteger.ModPow(z, q, p);
    var t = BigInteger.ModPow(a, q, p);
    var r = BigInteger.ModPow(a, (q + 1) / 2, p);

    while (t != 1)
    {
      // Find the least i such that t^(2^i) = 1
      var i = 1;
      var temp = (t * t) % p;
      while (temp != 1 && i < m)
      {
        temp = (temp * temp) % p;
        i++;
      }

      if (i == m)
        return null; // Should not happen if p is prime

      var b = BigInteger.ModPow(c, BigInteger.Pow(2, m - i - 1), p);
      m = i;
      c = (b * b) % p;
      t = (t * c) % p;
      r = (r * b) % p;
    }

    return new BigInt(r);
  }

  /// <summary>
  /// Extended Euclidean Algorithm
  /// Returns (gcd, x, y) such that a*x + b*y = gcd(a, b)
  /// </summary>
  private static (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b)
  {
    if (b == 0)
      return (a, 1, 0);

    var (gcd, x1, y1) = ExtendedGcd(b, a % b);
    var x = y1;
    var y = x1 - (a / b) * y1;

    return (gcd, x, y);
  }

  #endregion
}