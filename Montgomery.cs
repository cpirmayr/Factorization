using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Montgomery-Arithmetik für modulare Multiplikation.
/// 
/// Grundidee: Statt x·y mod n direkt zu berechnen, arbeiten wir mit
/// "Montgomery-Repräsentanten" x̄ = x·R mod n, wobei R = 2^k mit k größer-gleich Bitlänge(n).
/// 
/// Vorteil: Die Modulo-Operation kann durch schnelle Bit-Shifts ersetzt werden.
/// 
/// Montgomery-Reduktion: Für T kleiner R·n gilt:
/// REDC(T) = T·R^(-1) mod n
/// berechenbar in O(n) Zeit ohne Division durch n.
/// </summary>
public sealed class Montgomery
{
    public long ExponentBitLength;

    // === Grundparameter ===
    private readonly BigInteger n;     // Modulus (muss ungerade sein)
    private readonly BigInteger r;     // R = 2^k, wobei k größer-gleich Bitlänge(n)
    private readonly BigInteger rMask; // R - 1 (Bitmaske für x mod R)
    private readonly BigInteger nInv;  // -n^(-1) mod R (für Montgomery-Reduktion)
    private readonly BigInteger rModN; // R mod n (für schnelle Konvertierung)
    private readonly BigInteger r2ModN;// R² mod n (für ToMontgomery)
    private readonly int rBits;        // k = Bitlänge von R
    
    // === Für Exponentiation gecacht ===
    private readonly byte[] exponentBytes;

    /// <summary>
    /// Konstruktor für Montgomery-Arithmetik modulo n.
    /// </summary>
    /// <param name="modulus">Modulus n (muss ungerade und positiv sein)</param>
    public Montgomery(BigInteger modulus)
    {
        if (modulus <= 0)
            throw new ArgumentException("Modulus must be positive.");
        if (modulus.IsEven)
            throw new ArgumentException("Modulus must be odd for Montgomery reduction.");

        n = modulus;

        // Wähle R = 2^k mit k = Bitlänge(n)
        // Dies stellt sicher, dass R > n (notwendig für korrekte Arithmetik)
        rBits = (int)n.GetBitLength();  // OPTIMIERT: Nutze eingebaute Methode
        r = BigInteger.One << rBits;    // R = 2^rBits
        rMask = r - 1;                  // Für schnelles x mod R via x AND rMask

        // Berechne nInv = -n^(-1) mod R
        // Dies ist der Schlüsselwert für die Montgomery-Reduktion
        nInv = -ModInverse(n, r) & rMask;

        // OPTIMIERUNG: Vorberechne häufig benötigte Werte
        rModN = r % n;           // R mod n
        r2ModN = r * r % n;    // R² mod n (für schnelle ToMontgomery-Konvertierung)

        exponentBytes = [];
    }

    /// <summary>
    /// Konstruktor mit Exponent (für ModPow-Optimierungen).
    /// Der Exponent wird als Byte-Array gecacht für schnellen Bit-Zugriff.
    /// </summary>
    public Montgomery(BigInteger modulus, BigInteger exponent)
        : this(modulus)
    {
        // Little-Endian Format für schnellen Bit-Zugriff
        exponentBytes = exponent.ToByteArray(isUnsigned: true, isBigEndian: false);
        ExponentBitLength = exponent.GetBitLength();
    }

    /// <summary>
    /// Prüft, ob Bit an Position bitIndex im gespeicherten Exponenten gesetzt ist.
    /// Verwendet gecachtes Byte-Array für schnellen Zugriff.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBitSet(int bitIndex)
    {
        int byteIndex = bitIndex >> 3;  // Division durch 8
        if (byteIndex >= exponentBytes.Length)
            return false;

        int bitInByte = bitIndex & 7;   // Modulo 8
        return ((exponentBytes[byteIndex] >> bitInByte) & 1) != 0;
    }

    // =========================================================================
    // Montgomery-Reduktion: Das Herzstück des Algorithmus
    // =========================================================================
    
    /// <summary>
    /// Montgomery-Reduktion: REDC(T) = T·R^(-1) mod n
    /// 
    /// Algorithmus:
    /// 1. m = (T mod R) · nInv mod R
    /// 2. u = (T + m·n) / R
    /// 3. Falls u größer-gleich n, dann u = u - n
    /// 
    /// Invariante: Wenn T kleiner R·n, dann ist 0 kleiner-gleich u kleiner 2n.
    /// Die finale Subtraktion stellt sicher: 0 kleiner-gleich Ergebnis kleiner n.
    /// </summary>
    /// <param name="t">Eingabe T (sollte kleiner R·n sein für korrekte Ergebnisse)</param>
    /// <returns>T·R^(-1) mod n</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BigInteger Reduce(BigInteger t)
    {
        // Schritt 1: m = (T mod R) · nInv mod R
        // Da nInv = -n^(-1) mod R, kompensiert m·n genau den Rest von T mod R
        BigInteger m = ((t & rMask) * nInv) & rMask;

        // Schritt 2: u = (T + m·n) / R
        // Durch Konstruktion ist (T + m·n) kongruent 0 mod R, also Division ohne Rest
        // Division durch R = 2^k erfolgt als Rechts-Shift (sehr schnell!)
        BigInteger u = (t + m * n) >> rBits;

        // Schritt 3: Finale Reduktion auf [0, n)
        // Nach obiger Operation gilt: 0 kleiner-gleich u kleiner 2n
        // Eine einzige bedingte Subtraktion genügt
        if (u >= n)
        {
          u -= n;
        }
        return u;
    }

    // =========================================================================
    // Konvertierung zwischen normaler und Montgomery-Form
    // =========================================================================
    
    /// <summary>
    /// Konvertiert x in Montgomery-Form: x̄ = x·R mod n
    /// 
    /// OPTIMIERT: Statt (x·R) mod n direkt zu berechnen, nutzen wir:
    /// x̄ = REDC(x · R²) 
    /// 
    /// Da R² mod n vorberechnet ist, sind nur eine Multiplikation und
    /// eine Montgomery-Reduktion nötig (keine teure Modulo-Division!).
    /// </summary>
    /// <param name="x">Normale Darstellung</param>
    /// <returns>Montgomery-Darstellung x·R mod n</returns>
    public BigInteger ToMontgomery(BigInteger x)
    {
        // Normalisiere x auf [0, n)
        x %= n;
        if (x.Sign < 0)
        {
          x += n;
        }

        // OPTIMIERUNG: Nutze vorberechnetes R² mod n
        // x·R mod n = REDC(x · R²) weil:
        // REDC(x·R²) = (x·R²)·R^(-1) mod n = x·R mod n
        return Reduce(x * r2ModN);
    }

    /// <summary>
    /// Konvertiert aus Montgomery-Form zurück: x = x̄·R^(-1) mod n
    /// 
    /// Dies ist einfach eine Montgomery-Reduktion mit x̄ als Eingabe.
    /// </summary>
    /// <param name="xMont">Montgomery-Darstellung</param>
    /// <returns>Normale Darstellung x</returns>
    public BigInteger FromMontgomery(BigInteger xMont)
    {
        // REDC(x̄) = x̄·R^(-1) mod n = (x·R)·R^(-1) mod n = x mod n
        return Reduce(xMont);
    }

    // =========================================================================
    // Arithmetik in Montgomery-Form
    // =========================================================================
    
    /// <summary>
    /// Montgomery-Multiplikation: ā·b̄·R^(-1) mod n
    /// 
    /// Wenn ā = a·R und b̄ = b·R, dann:
    /// REDC(ā·b̄) = (a·R·b·R)·R^(-1) mod n = a·b·R mod n
    /// 
    /// Das Ergebnis ist also wieder in Montgomery-Form!
    /// Dies ermöglicht Kettenoperationen ohne Hin-und-Her-Konvertierung.
    /// </summary>
    /// <param name="aMont">Erster Faktor in Montgomery-Form</param>
    /// <param name="bMont">Zweiter Faktor in Montgomery-Form</param>
    /// <returns>Produkt in Montgomery-Form</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigInteger Multiply(BigInteger aMont, BigInteger bMont)
    {
        return Reduce(aMont * bMont);
    }

    /// <summary>
    /// Montgomery-Quadrierung: ā²·R^(-1) mod n
    /// 
    /// Spezialfall der Multiplikation. BigInteger hat leider keine
    /// spezielle Squaring-Optimierung, aber semantisch klarer als Code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigInteger Square(BigInteger aMont)
    {
        return Reduce(aMont * aMont);
    }

    // =========================================================================
    // Hilfsfunktionen
    // =========================================================================
    
    /// <summary>
    /// Berechnet Division durch 2 (Rechts-Shift).
    /// Nützlich für bestimmte Algorithmen, die Halbierung benötigen.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigInteger ShiftRight(BigInteger x)
    {
        return x >> 1;
    }

    /// <summary>
    /// Berechnet modulares Inverses a^(-1) mod m mittels erweitertem
    /// Euklidischem Algorithmus.
    /// 
    /// Algorithmus:
    /// - Verwalte zwei Zeilen (t, r) und (newT, newR)
    /// - Invariante: t·a + ...·m = r
    /// - Am Ende gilt: r = gcd(a,m) und t = a^(-1) mod m (falls gcd = 1)
    /// </summary>
    private static BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger t = 0, newT = 1;
        BigInteger r = m, newR = a;

        while (newR != 0)
        {
            BigInteger q = r / newR;

            // Update (t, newT) und (r, newR) gleichzeitig
            (t, newT) = (newT, t - q * newT);
            (r, newR) = (newR, r - q * newR);
        }

        if (r > 1)
            throw new ArgumentException("Element has no modular inverse.");

        // Normalisiere Ergebnis auf [0, m)
        if (t.Sign < 0)
        {
          t += m;
        }
        return t;
    }

    /// <summary>
    /// STATISCHE Bit-Test-Funktion (ACHTUNG: Ineffizient bei wiederholtem Aufruf!)
    /// 
    /// Diese Methode konvertiert bei jedem Aufruf das BigInteger zu einem Byte-Array.
    /// Für wiederholte Aufrufe: Nutze die Instanzmethode IsBitSet() mit gecachtem
    /// exponentBytes!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitSet(BigInteger x, int bitIndex)
    {
        if (bitIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));

        // Konvertiere zu Byte-Array (Little-Endian)
        byte[] bytes = x.ToByteArray(isUnsigned: true, isBigEndian: false);
        
        int byteIndex = bitIndex >> 3;      // Division durch 8
        if (byteIndex >= bytes.Length)
            return false;

        int bitInByte = bitIndex & 7;       // Modulo 8
        return ((bytes[byteIndex] >> bitInByte) & 1) != 0;
    }
}