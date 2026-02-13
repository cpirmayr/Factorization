using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;

namespace Factorization;

/// <summary>
/// Wrapper-Struktur für BigInteger, die sich wie eingebaute numerische Typen verhält.
/// Bietet umfassende arithmetische, bitweise und modulare Operationen für beliebig große Ganzzahlen,
/// sowie spezialisierte Funktionen für Zahlentheorie und Faktorisierungsalgorithmen.
/// </summary>
public struct BigInt : IComparable, IComparable<BigInt>, IEquatable<BigInt>, IFormattable
{
  // ====================================================================
  // FELDER UND KONSTRUKTOREN
  // ====================================================================

  /// <summary>Der interne BigInteger-Wert.</summary>
  public readonly BigInteger _value;

  /// <summary>Erstellt einen BigInt aus einem BigInteger.</summary>
  public BigInt(BigInteger value) => _value = value;

  /// <summary>Erstellt einen BigInt aus einem Byte-Array.</summary>
  public BigInt(byte[] value) => _value = new BigInteger(value);

  /// <summary>Erstellt einen BigInt aus einem Decimal-Wert.</summary>
  public BigInt(decimal value) => _value = new BigInteger(value);

  /// <summary>Erstellt einen BigInt aus einem Double-Wert.</summary>
  public BigInt(double value) => _value = new BigInteger(value);

  /// <summary>Erstellt einen BigInt aus einem Int32-Wert.</summary>
  public BigInt(int value) => _value = new BigInteger(value);

  /// <summary>Erstellt einen BigInt aus einem Int64-Wert.</summary>
  public BigInt(long value) => _value = new BigInteger(value);

  /// <summary>Erstellt einen BigInt durch Parsen eines Strings.</summary>
  public BigInt(string value) => _value = BigInteger.Parse(value);

  /// <summary>Erstellt einen BigInt aus einem UInt32-Wert.</summary>
  public BigInt(uint value) => _value = new BigInteger(value);

  /// <summary>Erstellt einen BigInt aus einem UInt64-Wert.</summary>
  public BigInt(ulong value) => _value = new BigInteger(value);

  // ====================================================================
  // STATISCHE KONSTANTEN
  // ====================================================================

  /// <summary>Repräsentiert den Wert -1.</summary>
  public static BigInt MinusOne => new(BigInteger.MinusOne);

  /// <summary>Repräsentiert den Wert 1.</summary>
  public static BigInt One => new(BigInteger.One);

  /// <summary>Repräsentiert den Wert 0.</summary>
  public static BigInt Zero => new(BigInteger.Zero);

  /// <summary>Kleine Primzahlen für schnelle Primzahltests.</summary>
  private static readonly int[] SmallPrimes =
  [
    3, 5, 7, 11, 13, 17, 19, 23, 29, 31,
    37, 41, 43, 47, 53, 59, 61, 67
  ];

  // ====================================================================
  // EIGENSCHAFTEN
  // ====================================================================

  /// <summary>Gibt den nächsten geraden Wert zurück (aktueller Wert wenn gerade, sonst +1).</summary>
  public BigInt Even => _value.IsEven ? _value : _value + 1;

  /// <summary>Prüft, ob der Wert gerade ist.</summary>
  public bool IsEven => _value.IsEven;

  /// <summary>Prüft, ob der Wert ungerade ist.</summary>
  public bool IsOdd => !_value.IsEven;

  /// <summary>Prüft, ob der Wert 1 ist.</summary>
  public bool IsOne => _value.IsOne;

  /// <summary>Prüft, ob der Wert eine Zweierpotenz ist.</summary>
  public bool IsPowerOfTwo => _value.IsPowerOfTwo;

  /// <summary>Prüft, ob der Wert 0 ist.</summary>
  public bool IsZero => _value.IsZero;

  /// <summary>Gibt das Vorzeichen zurück: -1 für negativ, 0 für null, 1 für positiv.</summary>
  public int Sign => _value.Sign;

  // ====================================================================
  // ARITHMETISCHE OPERATOREN
  // ====================================================================

  public static BigInt operator +(BigInt left, BigInt right) => new(left._value + right._value);
  public static BigInt operator +(BigInt value) => value;
  public static BigInt operator ++(BigInt value) => new(value._value + 1);
  public static BigInt operator -(BigInt left, BigInt right) => new(left._value - right._value);
  public static BigInt operator -(BigInt value) => new(-value._value);
  public static BigInt operator --(BigInt value) => new(value._value - 1);
  public static BigInt operator *(BigInt left, BigInt right) => new(left._value * right._value);
  public static BigInt operator /(BigInt left, BigInt right) => new(left._value / right._value);
  public static BigInt operator %(BigInt left, BigInt right) => new(left._value % right._value);

  // ====================================================================
  // BITWEISE OPERATOREN
  // ====================================================================

  public static BigInt operator &(BigInt left, BigInt right) => new(left._value & right._value);
  public static BigInt operator |(BigInt left, BigInt right) => new(left._value | right._value);
  public static BigInt operator ^(BigInt left, BigInt right) => new(left._value ^ right._value);
  public static BigInt operator ~(BigInt value) => new(~value._value);
  public static BigInt operator <<(BigInt value, int shift) => new(value._value << shift);
  public static BigInt operator >>(BigInt value, int shift) => new(value._value >> shift);

  // ====================================================================
  // VERGLEICHSOPERATOREN
  // ====================================================================

  public static bool operator ==(BigInt left, BigInt right) => left._value == right._value;
  public static bool operator !=(BigInt left, BigInt right) => left._value != right._value;
  public static bool operator <(BigInt left, BigInt right) => left._value < right._value;
  public static bool operator <=(BigInt left, BigInt right) => left._value <= right._value;
  public static bool operator >(BigInt left, BigInt right) => left._value > right._value;
  public static bool operator >=(BigInt left, BigInt right) => left._value >= right._value;

  // ====================================================================
  // IMPLIZITE KONVERTIERUNGEN (verlustfrei)
  // ====================================================================

  public static implicit operator BigInt(byte value) => new(value);
  public static implicit operator BigInt(sbyte value) => new(value);
  public static implicit operator BigInt(short value) => new(value);
  public static implicit operator BigInt(ushort value) => new(value);
  public static implicit operator BigInt(int value) => new(value);
  public static implicit operator BigInt(uint value) => new(value);
  public static implicit operator BigInt(long value) => new(value);
  public static implicit operator BigInt(ulong value) => new(value);
  public static implicit operator BigInt(BigInteger value) => new(value);
  public static implicit operator BigInteger(BigInt value) => value._value;

  // ====================================================================
  // EXPLIZITE KONVERTIERUNGEN (möglicherweise verlustbehaftet)
  // ====================================================================

  public static explicit operator BigInt(decimal value) => new(value);
  public static explicit operator BigInt(double value) => new(value);
  public static explicit operator byte(BigInt value) => (byte)value._value;
  public static explicit operator sbyte(BigInt value) => (sbyte)value._value;
  public static explicit operator short(BigInt value) => (short)value._value;
  public static explicit operator ushort(BigInt value) => (ushort)value._value;
  public static explicit operator int(BigInt value) => (int)value._value;
  public static explicit operator uint(BigInt value) => (uint)value._value;
  public static explicit operator long(BigInt value) => (long)value._value;
  public static explicit operator ulong(BigInt value) => (ulong)value._value;
  public static explicit operator decimal(BigInt value) => (decimal)value._value;
  public static explicit operator double(BigInt value) => (double)value._value;

  // ====================================================================
  // INTERFACE-IMPLEMENTIERUNGEN
  // ====================================================================

  public int CompareTo(object? obj)
  {
    if (obj == null) return 1;
    if (obj is not BigInt other)
      throw new ArgumentException("Object must be of type BigInt");
    return _value.CompareTo(other._value);
  }

  public int CompareTo(BigInt other) => _value.CompareTo(other._value);
  public bool Equals(BigInt other) => _value.Equals(other._value);
  public override bool Equals(object? obj) => obj is BigInt other && Equals(other);
  public override int GetHashCode() => _value.GetHashCode();
  public override string ToString() => _value.ToString();
  public string ToString(string? format) => _value.ToString(format);
  public string ToString(IFormatProvider? provider) => _value.ToString(provider);
  public string ToString(string? format, IFormatProvider? provider) => _value.ToString(format, provider);

  // ====================================================================
  // STATISCHE BASISMETHODEN
  // ====================================================================

  /// <summary>Gibt den Absolutwert zurück.</summary>
  public static BigInt Abs(BigInt value) => new(BigInteger.Abs(value._value));

  /// <summary>Addiert zwei Werte.</summary>
  public static BigInt Add(BigInt left, BigInt right) => left + right;

  /// <summary>Vergleicht zwei Werte.</summary>
  public static int Compare(BigInt left, BigInt right) => BigInteger.Compare(left._value, right._value);

  /// <summary>Dividiert zwei Werte.</summary>
  public static BigInt Divide(BigInt dividend, BigInt divisor) => dividend / divisor;

  /// <summary>Dividiert mit Rest.</summary>
  public static BigInt DivRem(BigInt dividend, BigInt divisor, out BigInt remainder)
  {
    BigInteger quotient = BigInteger.DivRem(dividend._value, divisor._value, out BigInteger rem);
    remainder = new BigInt(rem);
    return new BigInt(quotient);
  }

  /// <summary>Berechnet den größten gemeinsamen Teiler (GCD).</summary>
  public static BigInt GreatestCommonDivisor(BigInt left, BigInt right) =>
      new(BigInteger.GreatestCommonDivisor(left._value, right._value));

  /// <summary>
  /// Berechnet das kleinste gemeinsame Vielfache (LCM).
  /// Verwendet die Formel: LCM(a,b) = |a * b| / GCD(a,b)
  /// </summary>
  public static BigInt LeastCommonMultiple(BigInt left, BigInt right)
  {
    if (left.IsZero || right.IsZero)
      return Zero;
    
    BigInt gcd = GreatestCommonDivisor(left, right);
    return Abs((left / gcd) * right);
  }

  /// <summary>Berechnet den natürlichen Logarithmus.</summary>
  public static double Log(BigInt value) => BigInteger.Log(value._value);

  /// <summary>Berechnet den Logarithmus zur angegebenen Basis.</summary>
  public static double Log(BigInt value, double baseValue) => BigInteger.Log(value._value, baseValue);

  /// <summary>Berechnet den Logarithmus zur Basis 10.</summary>
  public static double Log10(BigInt value) => BigInteger.Log10(value._value);

  /// <summary>Gibt den größeren von zwei Werten zurück.</summary>
  public static BigInt Max(BigInt left, BigInt right) => left > right ? left : right;

  /// <summary>Gibt den kleineren von zwei Werten zurück.</summary>
  public static BigInt Min(BigInt left, BigInt right) => left < right ? left : right;

  /// <summary>Multipliziert zwei Werte.</summary>
  public static BigInt Multiply(BigInt left, BigInt right) => left * right;

  /// <summary>Negiert einen Wert.</summary>
  public static BigInt Negate(BigInt value) => -value;

  /// <summary>Potenziert einen Wert.</summary>
  public static BigInt Pow(BigInt value, int exponent) => new(BigInteger.Pow(value._value, exponent));

  /// <summary>Berechnet den Rest einer Division.</summary>
  public static BigInt Remainder(BigInt dividend, BigInt divisor) => dividend % divisor;

  /// <summary>Subtrahiert zwei Werte.</summary>
  public static BigInt Subtract(BigInt left, BigInt right) => left - right;

  // ====================================================================
  // PARSING-METHODEN
  // ====================================================================

  /// <summary>Konvertiert einen String in einen BigInt.</summary>
  public static BigInt Parse(string value) => new(BigInteger.Parse(value));

  /// <summary>Konvertiert einen String mit angegebenem NumberStyle in einen BigInt.</summary>
  public static BigInt Parse(string value, NumberStyles style) => new(BigInteger.Parse(value, style));

  /// <summary>Konvertiert einen String mit angegebenem FormatProvider in einen BigInt.</summary>
  public static BigInt Parse(string value, IFormatProvider? provider) => new(BigInteger.Parse(value, provider));

  /// <summary>Konvertiert einen String mit NumberStyle und FormatProvider in einen BigInt.</summary>
  public static BigInt Parse(string value, NumberStyles style, IFormatProvider? provider) =>
      new(BigInteger.Parse(value, style, provider));

  /// <summary>Versucht, einen String in einen BigInt zu konvertieren.</summary>
  public static bool TryParse(string? value, out BigInt result)
  {
    bool success = BigInteger.TryParse(value, out BigInteger bigIntResult);
    result = new BigInt(bigIntResult);
    return success;
  }

  /// <summary>Versucht, einen String mit NumberStyle und FormatProvider in einen BigInt zu konvertieren.</summary>
  public static bool TryParse(string? value, NumberStyles style, IFormatProvider? provider, out BigInt result)
  {
    bool success = BigInteger.TryParse(value, style, provider, out BigInteger bigIntResult);
    result = new BigInt(bigIntResult);
    return success;
  }

  // ====================================================================
  // BITMANIPULATION
  // ====================================================================

  /// <summary>Gibt die Anzahl der Bits zurück.</summary>
  public long GetBitLength() => _value.GetBitLength();

  /// <summary>Gibt die Anzahl der Bytes zurück.</summary>
  public int GetByteCount(bool isUnsigned = false) => _value.GetByteCount(isUnsigned);

  /// <summary>Prüft, ob ein bestimmtes Bit gesetzt ist.</summary>
  public bool GetBit(int index)
  {
    if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
    return ((_value >> index) & BigInteger.One) != BigInteger.Zero;
  }

  /// <summary>Setzt ein bestimmtes Bit.</summary>
  public BigInt SetBit(int index) => new(_value | (BigInteger.One << index));

  /// <summary>Löscht ein bestimmtes Bit.</summary>
  public BigInt ClearBit(int index) => new(_value & ~(BigInteger.One << index));

  /// <summary>Invertiert ein bestimmtes Bit.</summary>
  public BigInt ToggleBit(int index) => new(_value ^ (BigInteger.One << index));

  /// <summary>Zählt die Anzahl der führenden Nullen (von rechts).</summary>
  public int TrailingZeroCount()
  {
    if (_value.IsZero) return 0;
    int count = 0;
    BigInteger temp = _value;
    while ((temp & BigInteger.One).IsZero)
    {
      count++;
      temp >>= 1;
    }
    return count;
  }

  /// <summary>Konvertiert den Wert in ein Byte-Array.</summary>
  public byte[] ToByteArray() => _value.ToByteArray();

  // ====================================================================
  // MODULARE ARITHMETIK
  // ====================================================================

  /// <summary>Modulare Addition: (this + other) mod modulus.</summary>
  public BigInt AddMod(BigInt other, BigInt modulus)
  {
    var result = (_value + other._value) % modulus._value;
    return new BigInt(result < 0 ? result + modulus._value : result);
  }

  /// <summary>Modulare Subtraktion: (this - other) mod modulus.</summary>
  public BigInt SubMod(BigInt other, BigInt modulus)
  {
    var result = (_value - other._value) % modulus._value;
    return new BigInt(result < 0 ? result + modulus._value : result);
  }

  /// <summary>Modulare Multiplikation: (this * other) mod modulus.</summary>
  public BigInt MulMod(BigInt other, BigInt modulus)
  {
    var result = (_value * other._value) % modulus._value;
    return new BigInt(result < 0 ? result + modulus._value : result);
  }

  /// <summary>Modulare Quadrierung: (this²) mod modulus.</summary>
  public BigInt SquareMod(BigInt modulus) => new(BigInteger.ModPow(_value, 2, modulus._value));

  /// <summary>Modulare Potenzierung: (this^exponent) mod modulus.</summary>
  public BigInt PowerMod(BigInt exponent, BigInt modulus) =>
      new(BigInteger.ModPow(_value, exponent._value, modulus._value));

  /// <summary>
  /// Berechnet das modulare Inverse: findet x so dass (this * x) ≡ 1 (mod modulus).
  /// Gibt null zurück, wenn kein Inverses existiert (wenn GCD(this, modulus) ≠ 1).
  /// </summary>
  public BigInt? InverseMod(BigInt modulus)
  {
    if (modulus <= 0)
      throw new ArgumentException("Modulus must be positive", nameof(modulus));

    var (gcd, x, _) = ExtendedGcd(_value, modulus._value);
    if (!gcd.IsOne) return null;

    return new BigInt(x < 0 ? x + modulus._value : x);
  }

  /// <summary>Alias für InverseMod.</summary>
  public BigInt? InvMod(BigInt modulus) => InverseMod(modulus);

  /// <summary>
  /// Berechnet das Montgomery-Inverse für Montgomery-Reduktion.
  /// Benötigt einen ungeraden Modulus.
  /// </summary>
  public BigInt InverseModMontgomery(BigInt modulus, int rBits)
  {
    if (modulus <= 0 || modulus.IsEven)
      throw new ArgumentException("Modulus must be positive and odd for Montgomery reduction");

    BigInteger m = modulus._value;
    BigInteger two = 2;
    BigInteger mInv = 1;

    int currentBits = 8;
    while (currentBits < rBits)
    {
      currentBits *= 2;
      BigInteger mask = (BigInteger.One << currentBits) - 1;
      mInv = (mInv * (two - (m * mInv & mask))) & mask;
    }

    if (currentBits != rBits)
    {
      BigInteger mask = (BigInteger.One << rBits) - 1;
      mInv = (mInv * (two - (m * mInv & mask))) & mask;
    }

    BigInteger r = BigInteger.One << rBits;
    mInv = (r - mInv) & ((BigInteger.One << rBits) - 1);

    return new BigInt(mInv);
  }

  /// <summary>
  /// Berechnet die modulare n-te Wurzel: findet r so dass r^n ≡ this (mod modulus).
  /// Verwendet Tonelli-Shanks für n=2 mit Primzahl-Modulus, sonst Brute-Force.
  /// </summary>
  public BigInt? RootMod(int n, BigInt modulus)
  {
    if (n <= 0) throw new ArgumentException("Root degree must be positive", nameof(n));
    if (modulus <= 0) throw new ArgumentException("Modulus must be positive", nameof(modulus));
    if (_value.IsZero) return Zero;
    if (n == 1) return new BigInt(_value % modulus._value);
    if (n == 2) return SquareRootMod(modulus);

    // Brute-Force für allgemeinen Fall (ineffizient für große Moduln)
    for (BigInteger i = 0; i < modulus._value; i++)
    {
      if (BigInteger.ModPow(i, n, modulus._value) == (_value % modulus._value))
        return new BigInt(i);
    }
    return null;
  }

  /// <summary>
  /// Berechnet den diskreten Logarithmus: findet x so dass baseValue^x ≡ value (mod modulus).
  /// Verwendet den Baby-Step Giant-Step (BSGS) Algorithmus.
  /// Laufzeit: O(√modulus) Zeit und Speicher.
  /// </summary>
  /// <param name="value">Der Zielwert (g^x mod p).</param>
  /// <param name="baseValue">Die Basis g.</param>
  /// <param name="modulus">Der Modulus p (sollte eine Primzahl sein).</param>
  /// <returns>Der diskrete Logarithmus x, oder null wenn keine Lösung existiert.</returns>
  public static BigInt? LogMod(BigInt value, BigInt baseValue, BigInt modulus)
  {
    if (modulus <= 0)
      throw new ArgumentException("Modulus must be positive", nameof(modulus));
    if (baseValue <= 0 || baseValue >= modulus)
      throw new ArgumentException("Base must be in range (0, modulus)", nameof(baseValue));
    if (value < 0 || value >= modulus)
      throw new ArgumentException("Value must be in range [0, modulus)", nameof(value));

    // Spezialfall: value = 1 => x = 0 (da g^0 = 1)
    if (value == 1) return Zero;

    // Spezialfall: value = baseValue => x = 1
    if (value == baseValue) return One;

    // Baby-Step Giant-Step Algorithmus
    // Wir suchen x mit g^x ≡ h (mod p)
    // Schreibe x = im + j, wobei m = ceil(√(p-1))
    // Dann: g^(im+j) ≡ h (mod p)
    //       g^j ≡ h * (g^(-m))^i (mod p)

    // Berechne m = ceil(√(modulus-1)) oder ceil(√φ(modulus))
    // Für Primzahlen: φ(p) = p-1
    BigInt m = Sqrt(modulus - 1) + 1;

    // Baby steps: Berechne Hash-Tabelle mit g^j mod p für j = 0, 1, ..., m-1
    Dictionary<BigInt, BigInt> babySteps = new();
    BigInt gPowJ = One;

    for (BigInt j = Zero; j < m; j++)
    {
      if (!babySteps.ContainsKey(gPowJ))
      {
        babySteps[gPowJ] = j;
      }
      gPowJ = gPowJ.MulMod(baseValue, modulus);
    }

    // Berechne g^(-m) mod p
    BigInt gPowM = baseValue.PowerMod(m, modulus);
    BigInt gPowMinusM = gPowM.InverseMod(modulus) ?? throw new InvalidOperationException(
      "Base is not coprime with modulus - no inverse exists");

    // Giant steps: Für i = 0, 1, 2, ... berechne γ = h * (g^(-m))^i mod p
    BigInt gamma = value;

    for (BigInt i = Zero; i < m; i++)
    {
      // Prüfe, ob γ in der Baby-Steps-Tabelle ist
      if (babySteps.TryGetValue(gamma, out BigInt j))
      {
        // Gefunden: x = im + j
        return i * m + j;
      }

      // Nächster Giant Step: γ = γ * g^(-m) mod p
      gamma = gamma.MulMod(gPowMinusM, modulus);
    }

    // Keine Lösung gefunden
    return null;
  }

  /// <summary>
  /// Instanzmethode für diskreten Logarithmus: findet x so dass baseValue^x ≡ this (mod modulus).
  /// </summary>
  public BigInt? LogMod(BigInt baseValue, BigInt modulus) => LogMod(this, baseValue, modulus);

  /// <summary>
  /// Berechnet die multiplikative Ordnung von a modulo p.
  /// Die Ordnung ist die kleinste positive Zahl k, so dass a^k ≡ 1 (mod p).
  /// Verwendet Faktorisierung von p-1 für effiziente Berechnung.
  /// </summary>
  /// <param name="a">Die Basis (muss teilerfremd zu p sein).</param>
  /// <param name="p">Der Modulus (sollte eine Primzahl sein).</param>
  /// <returns>Die multiplikative Ordnung von a modulo p, oder null wenn a nicht teilerfremd zu p ist.</returns>
  public static BigInt? Order(BigInt a, BigInt p)
  {
    if (p <= 1)
      throw new ArgumentException("Modulus must be > 1", nameof(p));
    if (a <= 0 || a >= p)
      throw new ArgumentException("a must be in range (0, p)", nameof(a));

    // Prüfe, ob a und p teilerfremd sind
    BigInt gcd = GreatestCommonDivisor(a, p);
    if (gcd != 1)
      return null; // a ist nicht teilerfremd zu p

    // Nach dem Satz von Euler: a^φ(p) ≡ 1 (mod p)
    // Für Primzahlen p: φ(p) = p - 1
    // Die Ordnung teilt φ(p), also müssen wir nur Teiler von p-1 testen

    BigInt phi = p - 1;

    // Spezialfall: a^1 ≡ 1 (mod p) bedeutet a = 1
    if (a == 1)
      return One;

    // Faktorisiere p-1
    List<(BigInt prime, int exponent)> factors = Factorize(phi);

    // Die Ordnung ist ein Teiler von φ(p) = p-1
    // Wir starten mit order = φ(p) und versuchen, es zu reduzieren
    BigInt order = phi;

    // Für jeden Primfaktor q^e von p-1:
    // Versuche, order durch q zu teilen, solange a^(order/q) ≡ 1 (mod p)
    foreach (var (prime, exponent) in factors)
    {
      // Teile order durch prime so oft wie möglich
      for (int i = 0; i < exponent; i++)
      {
        BigInt reducedOrder = order / prime;
        
        // Prüfe, ob a^(order/prime) ≡ 1 (mod p)
        if (a.PowerMod(reducedOrder, p) == 1)
        {
          order = reducedOrder;
        }
        else
        {
          // Kann nicht weiter durch prime teilen
          break;
        }
      }
    }

    return order;
  }

  /// <summary>
  /// Instanzmethode für multiplikative Ordnung: berechnet die Ordnung von this modulo p.
  /// </summary>
  public BigInt? Order(BigInt p) => Order(this, p);

  /// <summary>
  /// Prüft, ob a eine primitive Wurzel (Generator) modulo p ist.
  /// Eine primitive Wurzel hat die Ordnung φ(p) = p-1.
  /// </summary>
  /// <param name="a">Die zu testende Zahl.</param>
  /// <param name="p">Der Modulus (sollte eine Primzahl sein).</param>
  /// <returns>True wenn a eine primitive Wurzel modulo p ist.</returns>
  public static bool IsPrimitiveRoot(BigInt a, BigInt p)
  {
    BigInt? order = Order(a, p);
    return order.HasValue && order.Value == p - 1;
  }

  /// <summary>
  /// Instanzmethode: Prüft, ob this eine primitive Wurzel modulo p ist.
  /// </summary>
  public bool IsPrimitiveRoot(BigInt p) => IsPrimitiveRoot(this, p);

  /// <summary>
  /// Findet eine primitive Wurzel (Generator) modulo p.
  /// Für eine Primzahl p generiert eine primitive Wurzel g alle Elemente von Z_p*.
  /// </summary>
  /// <param name="p">Eine Primzahl.</param>
  /// <returns>Eine primitive Wurzel modulo p.</returns>
  public static BigInt FindPrimitiveRoot(BigInt p)
  {
    if (p <= 2)
      return One;

    // Kleine Primzahlen: verwende bekannte primitive Wurzeln
    if (p == 3) return new BigInt(2);
    if (p == 5) return new BigInt(2);
    if (p == 7) return new BigInt(3);

    // Für größere Primzahlen: teste zufällige Kandidaten
    // Im Durchschnitt ist etwa φ(p-1)/(p-1) der Zahlen eine primitive Wurzel
    // Für Primzahlen ist das typischerweise etwa 1/log(log(p))
    
    Random rng = new();
    int maxAttempts = 1000;

    for (int attempt = 0; attempt < maxAttempts; attempt++)
    {
      // Teste kleine Zahlen zuerst, dann zufällige
      BigInt candidate;
      if (attempt < 10)
      {
        candidate = new BigInt(attempt + 2);
      }
      else
      {
        candidate = RandomBigInteger(2, p - 1, rng);
      }

      if (IsPrimitiveRoot(candidate, p))
      {
        return candidate;
      }
    }

    throw new InvalidOperationException($"Could not find primitive root for p={p} after {maxAttempts} attempts");
  }
  // ====================================================================
  // WURZELFUNKTIONEN
  // ====================================================================

  /// <summary>
  /// Berechnet die ganzzahlige Quadratwurzel (größte ganze Zahl r mit r² ≤ n).
  /// </summary>
  public static BigInt Sqrt(BigInt n)
  {
    if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
    if (n < 2) return n;

    int bitLength = (int)n.GetBitLength();
    BigInt x = One << ((bitLength + 1) / 2);

    while (true)
    {
      BigInt y = (x + n / x) >> 1;
      if (y >= x) return x;
      x = y;
    }
  }

  /// <summary>Instanzmethode für Quadratwurzel.</summary>
  public BigInt SquareRoot() => Sqrt(this);

  /// <summary>Prüft, ob eine Zahl ein perfektes Quadrat ist.</summary>
  public static bool IsSquare(BigInt n)
  {
    if (n < 0) return false;
    if (n.IsZero || n.IsOne) return true;
    BigInt r = Sqrt(n);
    return r * r == n;
  }

  /// <summary>
  /// Berechnet die ganzzahlige n-te Wurzel (größte ganze Zahl r mit r^n ≤ value).
  /// Verwendet Newton-Raphson-Iteration.
  /// </summary>
  public static BigInt Root(BigInt value, int n)
  {
    if (n <= 0) throw new ArgumentException("Root degree must be positive", nameof(n));
    if (n == 1) return value;
    if (n == 2) return Sqrt(value);
    if (value < 0 && n % 2 == 0)
      throw new ArgumentException("Even root of negative number", nameof(value));

    if (value.IsZero || value.IsOne) return value;

    // Newton-Raphson: x_new = ((n-1) * x + value / x^(n-1)) / n
    BigInt x = value;
    BigInt nBig = new BigInt(n);
    BigInt nMinus1 = new BigInt(n - 1);

    while (true)
    {
      BigInt xToNMinus1 = Pow(x, n - 1);
      if (xToNMinus1.IsZero) return x;

      BigInt y = ((nMinus1 * x) + (value / xToNMinus1)) / nBig;
      if (y >= x) return x;
      x = y;
    }
  }

  /// <summary>Instanzmethode für n-te Wurzel.</summary>
  public BigInt Root(int n) => Root(this, n);

  /// <summary>
  /// Berechnet value^(1/n) * multiplier.
  /// Nützlich für Faktorisierungsalgorithmen.
  /// </summary>
  public BigInt PowRoot(int n, BigInt multiplier) => Root(this, n) * multiplier;

  // ====================================================================
  // CHEBYSHEV-POLYNOME
  // ====================================================================

  /// <summary>
  /// Berechnet das k-te Chebyshev-Polynom erster Art: T_k(x) mod n.
  /// Verwendet einen binären Leiteralgorithmus für effiziente Berechnung.
  /// </summary>
  public static BigInt CalculateChebyshev(BigInt x, BigInt k, BigInt n)
  {
    if (k.IsZero) return One;
    if (k.IsOne) return Mod(x, n);

    BigInt a = One;
    BigInt b = Mod(x, n);
    int bitLength = (int)k.GetBitLength();

    for (int i = bitLength - 1; i >= 0; i--)
    {
      bool bitSet = !((k >> i) & One).IsZero;
      if (bitSet)
      {
        a = Mod(2 * a * b - x, n);
        b = Mod(2 * b * b - One, n);
      }
      else
      {
        b = Mod(2 * a * b - x, n);
        a = Mod(2 * a * a - One, n);
      }
    }
    return a;
  }

  /// <summary>
  /// Berechnet das k-te Chebyshev-Polynom in nahezu konstanter Zeit.
  /// Vermeidet Timing-Angriffe durch branchless Berechnung.
  /// </summary>
  public static BigInt CalculateChebyshevConstantTime(BigInt x, BigInt k, BigInt n)
  {
    BigInt a = One;
    BigInt b = Mod(x, n);
    int bitLength = (int)k.GetBitLength();

    for (int i = bitLength - 1; i >= 0; i--)
    {
      BigInt t0A = Mod(2 * a * a - One, n);
      BigInt t0B = Mod(2 * a * b - x, n);
      BigInt t1B = Mod(2 * b * b - One, n);
      bool bit = !((k >> i) & One).IsZero;
      a = bit ? t0B : t0A;
      b = bit ? t1B : t0B;
    }
    return a;
  }

  // ====================================================================
  // KETTENBRÜCHE (CONTINUED FRACTIONS)
  // ====================================================================

  /// <summary>
  /// Berechnet die Näherungsbrüche (Convergents) der Kettenbruchentwicklung einer reellen Zahl.
  /// Nützlich für rationale Approximationen.
  /// </summary>
  public static IEnumerable<(long Numerator, long Denominator)> Convergents(double x, int maxIterations = 20)
  {
    long p0 = 0, p1 = 1;
    long q0 = 1, q1 = 0;
    double value = x;

    for (int i = 0; i < maxIterations; i++)
    {
      long a = (long)Math.Floor(value);
      long p = a * p1 + p0;
      long q = a * q1 + q0;
      yield return (p, q);

      double remainder = value - a;
      if (Math.Abs(remainder) < 1e-15) yield break;

      value = 1.0 / remainder;
      p0 = p1; p1 = p;
      q0 = q1; q1 = q;
    }
  }

  /// <summary>
  /// Berechnet die Näherungsbrüche der Quadratwurzel einer großen Zahl.
  /// Verwendet den exakten Algorithmus für quadratische Irrationale (keine Fließkommafehler).
  /// Wichtig für den Wiener-Angriff auf RSA und andere zahlentheoretische Anwendungen.
  /// </summary>
  public static IEnumerable<(BigInt Numerator, BigInt Denominator)> SqrtConvergents(BigInt n, int maxIterations = 100)
  {
    if (n < 0) throw new ArgumentException("n must be non-negative.");

    BigInt a0 = Sqrt(n);
    if (a0 * a0 == n)
    {
      yield return (a0, One);
      yield break;
    }

    BigInt m = Zero, d = One, a = a0;
    BigInt pMinus2 = Zero, pMinus1 = One;
    BigInt qMinus2 = One, qMinus1 = Zero;

    for (int i = 0; i < maxIterations; i++)
    {
      BigInt p = a * pMinus1 + pMinus2;
      BigInt q = a * qMinus1 + qMinus2;
      yield return (p, q);

      m = d * a - m;
      d = (n - m * m) / d;
      a = (a0 + m) / d;

      pMinus2 = pMinus1; pMinus1 = p;
      qMinus2 = qMinus1; qMinus1 = q;
    }
  }

  // ====================================================================
  // PRIMZAHLEN UND ZUFALLSZAHLEN
  // ====================================================================

  /// <summary>
  /// Generiert eine Semiprime-Zahl (Produkt zweier Primzahlen) mit der angegebenen
  /// Anzahl von Dezimalstellen. Nützlich zum Testen von Faktorisierungsalgorithmen.
  /// </summary>
  public static BigInt GenerateSemiPrime(int decimalPlaces, int seed = -1) =>
      GenerateSemiPrime(decimalPlaces, out _, out _, seed);

  /// <summary>
  /// Generiert eine Semiprime-Zahl und gibt die beiden Primfaktoren zurück.
  /// </summary>
  public static BigInt GenerateSemiPrime(int decimalPlaces, out BigInt p, out BigInt q, int seed = -1)
  {
    if (decimalPlaces < 2)
      throw new ArgumentException("Decimal places must be at least 2.");

    int d1 = decimalPlaces / 2;
    int d2 = decimalPlaces - d1;
    BigInt min1 = Pow(10, d1 - 1);
    BigInt max1 = Pow(10, d1) - 1;
    BigInt min2 = Pow(10, d2 - 1);
    BigInt max2 = Pow(10, d2) - 1;

    Random rng = seed >= 0 ? new Random(seed) : Random.Shared;
    p = GeneratePrimeInRange(min1, max1, rng);
    do
    {
      q = GeneratePrimeInRange(min2, max2, rng);
    } while (q == p);

    return p * q;
  }

  /// <summary>
  /// Erzeugt eine kryptographisch sichere Zufallszahl im Bereich [min, max].
  /// </summary>
  public static BigInt RandomBigIntegerCrypto(BigInt min, BigInt max)
  {
    BigInt range = max - min;
    byte[] bytes = range.ToByteArray();
    BigInt value;
    do
    {
      RandomNumberGenerator.Fill(bytes);
      bytes[^1] &= 0x7F; // Stelle sicher, dass die Zahl positiv bleibt
      value = new BigInt(bytes);
    } while (value > range);
    return min + value;
  }

  /// <summary>
  /// Erzeugt eine Zufallszahl im Bereich [0, modulus).
  /// </summary>
  public static BigInt RandomMod(BigInt modulus) => RandomBigIntegerCrypto(Zero, modulus - 1);

  // ====================================================================
  // GEFALTETE FOLGEN (für spezielle Faktorisierungsalgorithmen)
  // ====================================================================

  /// <summary>
  /// Initialisiert die Differenzen-Eimerkette für gefaltete Folgen.
  /// Wird für bestimmte Faktorisierungsansätze verwendet.
  /// </summary>
  public static void InitializeDifferences(BigInt N, BigInt foldings, out List<BigInt> differences)
  {
    int m = (int)foldings._value;
    int degree = 1 << m;

    List<BigInt> sequenceValues = new();
    for (int n = 1; n <= degree + 1; n++)
    {
      sequenceValues.Add(CalculateFoldedValue(n, N, m));
    }

    differences = new List<BigInt>();
    List<BigInt> currentLevel = sequenceValues;
    differences.Add(currentLevel[0]);

    for (int i = 0; i < degree; i++)
    {
      List<BigInt> nextLevel = new();
      for (int j = 0; j < currentLevel.Count - 1; j++)
      {
        nextLevel.Add(currentLevel[j + 1] - currentLevel[j]);
      }
      if (nextLevel.Count > 0)
      {
        differences.Add(nextLevel[0]);
      }
      currentLevel = nextLevel;
    }
  }

  /// <summary>
  /// Berechnet einen gefalteten Wert für die Eimerketten-Berechnung.
  /// </summary>
  public static BigInt CalculateFoldedValue(int n, BigInt N, int foldings)
  {
    List<BigInt> factors = new() { new BigInt(n) };
    BigInt currentLen = N >> foldings;

    for (int i = 0; i < foldings; i++)
    {
      currentLen *= 2;
      int count = factors.Count;
      for (int j = 0; j < count; j++)
      {
        BigInt partner = currentLen + 1 - factors[j];
        factors.Add(partner);
      }
    }

    BigInt result = One;
    foreach (var f in factors)
    {
      result *= f;
    }
    return result;
  }

  // ====================================================================
  // PRIVATE HILFSMETHODEN
  // ====================================================================

  /// <summary>
  /// Erweiterter euklidischer Algorithmus (iterativ).
  /// Gibt (gcd, x, y) zurück, so dass a*x + b*y = gcd(a, b).
  /// </summary>
  private static (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b)
  {
    if (b == 0) return (a, 1, 0);

    BigInteger oldR = a, r = b;
    BigInteger oldS = 1, s = 0;
    BigInteger oldT = 0, t = 1;

    while (r != 0)
    {
      BigInteger quotient = oldR / r;

      (oldR, r) = (r, oldR - quotient * r);
      (oldS, s) = (s, oldS - quotient * s);
      (oldT, t) = (t, oldT - quotient * t);
    }

    return (oldR, oldS, oldT);
  }

  /// <summary>
  /// Berechnet value % modulus und stellt sicher, dass das Ergebnis nicht-negativ ist.
  /// </summary>
  private static BigInt Mod(BigInt value, BigInt modulus)
  {
    BigInt res = value % modulus;
    return res.Sign < 0 ? res + modulus : res;
  }

  /// <summary>
  /// Berechnet die modulare Quadratwurzel mit dem Tonelli-Shanks-Algorithmus.
  /// Funktioniert für Primzahl-Moduln.
  /// </summary>
  private BigInt? SquareRootMod(BigInt modulus)
  {
    var a = _value % modulus._value;
    var p = modulus._value;

    // Prüfe mit Euler-Kriterium, ob a ein quadratischer Rest ist
    var legendreSymbol = BigInteger.ModPow(a, (p - 1) / 2, p);
    if (legendreSymbol != 1) return null;

    // Spezialfall: p ≡ 3 (mod 4)
    if ((p % 4) == 3)
    {
      var r1 = BigInteger.ModPow(a, (p + 1) / 4, p);
      return new BigInt(r1);
    }

    // Tonelli-Shanks für p ≡ 1 (mod 4)
    var s = 0;
    var q = p - 1;
    while ((q % 2) == 0)
    {
      q /= 2;
      s++;
    }

    // Finde quadratischen Nichtrest z
    BigInteger z = 2;
    while (BigInteger.ModPow(z, (p - 1) / 2, p) != p - 1) z++;

    var m = s;
    var c = BigInteger.ModPow(z, q, p);
    var t = BigInteger.ModPow(a, q, p);
    var r = BigInteger.ModPow(a, (q + 1) / 2, p);

    while (t != 1)
    {
      var i = 1;
      var temp = (t * t) % p;
      while (temp != 1 && i < m)
      {
        temp = (temp * temp) % p;
        i++;
      }

      if (i == m) return null;

      var b = BigInteger.ModPow(c, BigInteger.Pow(2, m - i - 1), p);
      m = i;
      c = (b * b) % p;
      t = (t * c) % p;
      r = (r * b) % p;
    }

    return new BigInt(r);
  }

  /// <summary>
  /// Prüft, ob eine Zahl durch kleine Primzahlen teilbar ist.
  /// Schneller Vortest für Primzahltests.
  /// </summary>
  private static bool PassesSmallPrimeTest(BigInt n)
  {
    foreach (int p in SmallPrimes)
    {
      if (n == p) return true;
      if (n % p == 0) return false;
    }
    return true;
  }

  /// <summary>
  /// Miller-Rabin-Primzahltest mit konfigurierbarer Anzahl von Runden.
  /// Probabilistischer Test: Wahrscheinlichkeit für Fehler ist 4^(-rounds).
  /// </summary>
  private static bool IsProbablePrimeCrypto(BigInt n, int rounds = 64)
  {
    if (n <= 1) return false;
    if (n == 2 || n == 3) return true;
    if (n.IsEven) return false;
    if (!PassesSmallPrimeTest(n)) return false;

    // Schreibe n-1 als 2^r * d
    BigInt d = n - 1;
    int r = 0;
    while (d.IsEven)
    {
      d >>= 1;
      r++;
    }

    // Führe rounds Runden des Miller-Rabin-Tests durch
    for (int i = 0; i < rounds; i++)
    {
      BigInt a = RandomBigIntegerCrypto(2, n - 2);
      BigInt x = a.PowerMod(d, n);
      if (x == 1 || x == n - 1) continue;

      bool composite = true;
      for (int j = 0; j < r - 1; j++)
      {
        x = x.PowerMod(2, n);
        if (x == n - 1)
        {
          composite = false;
          break;
        }
      }
      if (composite) return false;
    }
    return true;
  }

  /// <summary>
  /// Erzeugt eine Zufallszahl im angegebenen Bereich (nicht-kryptographisch).
  /// </summary>
  private static BigInt RandomBigInteger(BigInt min, BigInt max, Random rng)
  {
    BigInt range = max - min;
    byte[] bytes = range.ToByteArray();
    BigInt value;
    do
    {
      rng.NextBytes(bytes);
      bytes[^1] &= 0x7F;
      value = new BigInt(bytes);
    } while (value > range);
    return min + value;
  }

  /// <summary>
  /// Erzeugt eine zufällige Primzahl im angegebenen Bereich.
  /// </summary>
  private static BigInt GeneratePrimeInRange(BigInt min, BigInt max, Random rng)
  {
    while (true)
    {
      BigInt p = RandomBigInteger(min, max, rng);
      if (IsProbablePrimeCrypto(p)) return p;
    }
  }

  // ====================================================================
  // VOLLSTÄNDIGE FAKTORISIERUNG
  // ====================================================================

  /// <summary>
  /// Vollständige Faktorisierung einer Zahl.
  /// Kleine Primfaktoren werden per Probedivision entfernt,
  /// größere Faktoren mittels CFRAC bestimmt.
  /// </summary>
  /// <param name="n">Zu faktorisierende Zahl (n >= 2).</param>
  /// <returns>Liste von Paaren (Primfaktor, Exponent), sortiert nach aufsteigenden Faktoren.</returns>
  public static List<(BigInt prime, int exponent)> Factorize(BigInt n)
  {
    if (n < 2)
      throw new ArgumentException("n must be >= 2", nameof(n));

    Dictionary<BigInt, int> factors = new();

    void AddFactor(BigInt p)
    {
      if (factors.ContainsKey(p))
        factors[p]++;
      else
        factors[p] = 1;
    }

    // Probedivision für kleine Primzahlen
    int[] smallPrimes =
    {
      2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
      31, 37, 41, 43, 47, 53, 59, 61, 67, 71,
      73, 79, 83, 89, 97
    };

    foreach (int p in smallPrimes)
    {
      BigInt bp = p;
      while (n % bp == 0)
      {
        AddFactor(bp);
        n /= bp;
      }
    }

    if (n == 1)
    {
      return factors
        .OrderBy(f => f.Key)
        .Select(f => (f.Key, f.Value))
        .ToList();
    }

    // Rekursive Faktorisierung des Restes
    Stack<BigInt> stack = new();
    stack.Push(n);

    while (stack.Count > 0)
    {
      BigInt current = stack.Pop();

      if (current == 1) continue;

      // Primzahltest
      if (IsProbablePrime(current))
      {
        AddFactor(current);
        continue;
      }

      // Quadratzahl?
      if (IsPerfectSquare(current, out BigInt root))
      {
        stack.Push(root);
        stack.Push(root);
        continue;
      }

      // CFRAC-Faktorisierung versuchen
      BigInt factor = FactorizeCFRAC(current);

      if (factor <= 1 || factor == current)
      {
        // Notfall: als "Primzahl" akzeptieren
        AddFactor(current);
      }
      else
      {
        stack.Push(factor);
        stack.Push(current / factor);
      }
    }

    return factors
      .OrderBy(f => f.Key)
      .Select(f => (f.Key, f.Value))
      .ToList();
  }

  /// <summary>
  /// Schneller probabilistischer Primzahltest (Miller-Rabin).
  /// </summary>
  /// <param name="n">Zu testende Zahl.</param>
  /// <param name="rounds">Anzahl der Testrunden (höher = sicherer).</param>
  /// <returns>True wenn n wahrscheinlich prim ist, false wenn n definitiv zusammengesetzt ist.</returns>
  public static bool IsProbablePrime(BigInt n, int rounds = 10)
  {
    if (n < 2) return false;
    if (n == 2 || n == 3) return true;
    if (n.IsEven) return false;

    // Schreibe n-1 als 2^s * d
    BigInt d = n - 1;
    int s = 0;
    while (d.IsEven)
    {
      d >>= 1;
      s++;
    }

    Random rng = new();

    for (int i = 0; i < rounds; i++)
    {
      BigInt a = RandomBigInteger(2, n - 2, rng);
      BigInt x = a.PowerMod(d, n);
      if (x == 1 || x == n - 1) continue;

      bool witness = true;
      for (int r = 1; r < s; r++)
      {
        x = x.PowerMod(2, n);
        if (x == n - 1)
        {
          witness = false;
          break;
        }
      }

      if (witness) return false;
    }

    return true;
  }

  /// <summary>
  /// Prüft, ob eine Zahl ein perfektes Quadrat ist und gibt die Wurzel zurück.
  /// </summary>
  private static bool IsPerfectSquare(BigInt n, out BigInt root)
  {
    root = Sqrt(n);
    return root * root == n;
  }

  /// <summary>
  /// Faktorisierung mittels CFRAC (Continued Fraction)-Algorithmus.
  /// Diese Methode muss mit der externen CFRAC-Klasse verknüpft werden.
  /// </summary>
  /// <param name="n">Zu faktorisierende Zahl.</param>
  /// <returns>Ein nichttrivialer Faktor, oder 1 wenn keine Faktorisierung gefunden wurde.</returns>
  private static BigInt FactorizeCFRAC(BigInt n)
  {
    // Hier wird die externe CFRAC.Factorize-Methode aufgerufen
    // Falls CFRAC nicht verfügbar ist, gibt diese Methode 1 zurück
    try
    {
      return CFRAC.Factorize(n);
    }
    catch
    {
      return One;
    }
  }

  /// <summary>
  /// Berechnet (base^exponent) mod modulus mit der Sliding-Window-Methode.
  /// </summary>
  /// <param name="base">Die Basis</param>
  /// <param name="exponent">Der Exponent (muss ≥ 0 sein)</param>
  /// <param name="modulus">Der Modulus</param>
  /// <param name="windowSize">Fenstergröße in Bits (typisch 4–8)</param>
  /// <returns>base^exponent mod modulus</returns>
  public static BigInteger ModPowSlidingWindow(
      BigInteger @base,
      BigInteger exponent,
      BigInteger modulus,
      int windowSize = 5)
  {
    if (exponent < 0)
      throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent muss nicht-negativ sein.");
    if (modulus <= 0)
      throw new ArgumentOutOfRangeException(nameof(modulus), "Modulus muss positiv sein.");

    if (exponent == 0) return BigInteger.One % modulus;
    if (exponent == 1) return @base % modulus;
    if (@base == 0) return BigInteger.Zero;
    if (@base == 1) return BigInteger.One;

    // Sonderfall: sehr kleiner Exponent → Binary-Methode ist meist schneller
    if (exponent < 256)
      return BigInteger.ModPow(@base, exponent, modulus);

    // Normalisieren
    @base = @base % modulus;
    if (@base.IsZero) return BigInteger.Zero;

    // Fenstergröße validieren / sinnvoll begrenzen
    windowSize = Math.Clamp(windowSize, 3, 8);
    int tableSize = 1 << windowSize;           // 2^windowSize
    int maxPrecompute = tableSize - 1;

    // Precomputation: a^1, a^2, ..., a^(2^w - 1)
    var powers = new BigInteger[tableSize];
    powers[0] = BigInteger.One;
    powers[1] = @base;

    for (int i = 2; i <= maxPrecompute; i++)
    {
      powers[i] = (powers[i - 1] * @base) % modulus;
    }

    // Exponent in Binärdarstellung holen
    int bitLength = (int) exponent.GetBitLength();
    if (bitLength == 0) return BigInteger.One;

    BigInteger result = BigInteger.One;

    int currentBit = bitLength - 1;

    while (currentBit >= 0)
    {
      // Finde das nächste Fenster mit führender 1 (Sliding Window)
      int windowStart = currentBit;
      int windowEnd = Math.Max(currentBit - windowSize + 1, 0);

      // Suche die längste Folge beginnend mit 1
      int windowValue = 0;
      int windowLength = 0;

      for (int i = windowStart; i >= windowEnd; i--)
      {
        if (IsBitSet(exponent, i))    
        {
          windowValue = (windowValue << 1) | 1;
          windowLength++;
        }
        else if (windowLength > 0)
        {
          // Wir haben bereits ein Fenster → abbrechen
          break;
        }
        else
        {
          // Noch keine 1 gesehen → weiter nach links
          windowValue <<= 1;
        }
      }

      // Wenn wir ein Fenster gefunden haben
      if (windowLength > 0)
      {
        // Quadrate für die Fensterlänge
        for (int i = 0; i < windowLength; i++)
        {
          result = (result * result) % modulus;
        }

        // Multipliziere mit dem vorgefertigten Wert
        result = (result * powers[windowValue]) % modulus;

        // Springe vor
        currentBit -= windowLength;
      }
      else
      {
        // Kein Bit gesetzt → nur quadrieren
        result = (result * result) % modulus;
        currentBit--;
      }
    }

    return result;
  }

  // Hilfsfunktion: Prüft ob das i-te Bit gesetzt ist (0 = LSB)
  private static bool IsBitSet(BigInteger value, int bitPosition)
  {
    return (value & (BigInteger.One << bitPosition)) != BigInteger.Zero;
  }

}

/// <summary>
/// Extension-Methoden für BigInt und IEnumerable.
/// </summary>
public static class BigIntExtensions
{
  /// <summary>
  /// Gibt das n-te Element eines IEnumerable zu Testzwecken aus.
  /// </summary>
  public static void Test<T>(this IEnumerable<T> enumerable, int index)
  {
    Console.WriteLine(enumerable.ElementAtOrDefault(index));
  }
}