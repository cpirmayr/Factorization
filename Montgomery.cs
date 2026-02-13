using System.Numerics;
using System.Runtime.CompilerServices;

public sealed class Montgomery
{
    public long ExponentBitLength;

    private readonly BigInteger n;     // Modulus
    private readonly BigInteger r;     // R = 2^k
    private readonly BigInteger rMask; // R - 1
    private readonly BigInteger nInv;  // -n^{-1} mod R
    private readonly int rBits;
    private readonly byte[] exponentBytes;

    public Montgomery(BigInteger modulus)
    {
        if (modulus <= 0)
            throw new ArgumentException("Modulus must be positive.");
        if (modulus.IsEven)
            throw new ArgumentException("Modulus must be odd for Montgomery reduction.");

        n = modulus;

        rBits = GetBitLength(n);
        r = BigInteger.One << rBits;
        rMask = r - 1;

        // nInv = -n^{-1} mod R
        nInv = (-ModInverse(n, r)) & rMask;

        exponentBytes = [];
    }

    // Optionaler zusätzlicher Konstruktor
    public Montgomery(BigInteger modulus, BigInteger exponent)
        : this(modulus)
    {
        exponentBytes = exponent.ToByteArray(isUnsigned: true, isBigEndian: false);
        ExponentBitLength = exponent.GetBitLength();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBitSet(int bitIndex)
    {
        int byteIndex = bitIndex >> 3;
        if (byteIndex >= exponentBytes.Length)
            return false;

        int bitInByte = bitIndex & 7;
        return ((exponentBytes[byteIndex] >> bitInByte) & 1) != 0;
    }

    // -------------------------
    // Core Montgomery Reduction
    // -------------------------
    private BigInteger Reduce(BigInteger t)
    {
        BigInteger m = ((t & rMask) * nInv) & rMask;
        BigInteger u = (t + m * n) >> rBits;

        if (u >= n)
            u -= n;

        return u;
    }

    // -------------------------
    // Conversion
    // -------------------------
    public BigInteger ToMontgomery(BigInteger x)
    {
        x %= n;
        if (x.Sign < 0)
            x += n;

        return (x * r) % n;
    }

    public BigInteger FromMontgomery(BigInteger xMont)
    {
        BigInteger x = Reduce(xMont);
        if (x >= n)
            x -= n;

        return x;
    }

    // -------------------------
    // Arithmetic in Montgomery form
    // -------------------------
    public BigInteger Multiply(BigInteger aMont, BigInteger bMont)
    {
        return Reduce(aMont * bMont);
    }

    public BigInteger Square(BigInteger aMont)
    {
        return Reduce(aMont * aMont);
    }

    // -------------------------
    // Division / Shift
    // -------------------------
    public static BigInteger ShiftRight(BigInteger x)
    {
        return x >> 1;
    }

    // -------------------------
    // Helpers
    // -------------------------
    private static int GetBitLength(BigInteger x)
    {
        return x.IsZero ? 1 : (int)Math.Ceiling(BigInteger.Log(x, 2));
    }

    private static BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger t = 0, newT = 1;
        BigInteger r = m, newR = a;

        while (newR != 0)
        {
            BigInteger q = r / newR;

            (t, newT) = (newT, t - q * newT);
            (r, newR) = (newR, r - q * newR);
        }

        if (r > 1)
            throw new ArgumentException("Element has no modular inverse.");

        if (t.Sign < 0)
            t += m;

        return t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitSet(BigInteger x, int bitIndex)
    {
        if (bitIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));

        int limbIndex = bitIndex / 32;
        int bitInLimb = bitIndex % 32;

        uint[] limbs = x.ToByteArray(isUnsigned: true, isBigEndian: false)
                         .Select(b => (uint)b)
                         .ToArray();

        if (limbIndex >= limbs.Length)
            return false;

        return ((limbs[limbIndex] >> bitInLimb) & 1) != 0;
    }

}
