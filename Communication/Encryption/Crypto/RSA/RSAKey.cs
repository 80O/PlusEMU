using Plus.Utilities;

namespace Plus.Communication.Encryption.Crypto.RSA;

public class RsaKey
{
    protected bool CanDecrypt;
    protected bool CanEncrypt;

    public RsaKey(BigInteger n, int e,
        BigInteger d,
        BigInteger p, BigInteger q,
        BigInteger dmp1, BigInteger dmq1,
        BigInteger coeff)
    {
        E = e;
        this.e = e;
        N = n;
        D = d;
        P = p;
        Q = q;
        Dmp1 = dmp1;
        Dmq1 = dmq1;
        Coeff = coeff;
        CanEncrypt = N != 0 && E != 0;
        CanDecrypt = CanEncrypt && D != 0;

        //this.GeneratePair(1024, this.e);
    }

    public int E { get; }
    public BigInteger e { get; private set; }
    public BigInteger N { get; private set; }
    public BigInteger D { get; private set; }
    public BigInteger P { get; private set; }
    public BigInteger Q { get; private set; }
    public BigInteger Dmp1 { get; private set; }
    public BigInteger Dmq1 { get; private set; }
    public BigInteger Coeff { get; private set; }

    public void GeneratePair(int b, BigInteger e)
    {
        this.e = e;
        var qs = b >> 1;
        while (true)
        {
            while (true)
            {
                P = BigInteger.genPseudoPrime(b - qs, 1, Random.Shared);
                if ((P - 1).gcd(this.e) == 1 && P.isProbablePrime(10)) break;
            }
            while (true)
            {
                Q = BigInteger.genPseudoPrime(qs, 1, Random.Shared);
                if ((Q - 1).gcd(this.e) == 1 && P.isProbablePrime(10)) break;
            }
            if (P < Q)
            {
                var t = P;
                P = Q;
                Q = t;
            }
            var phi = (P - 1) * (Q - 1);
            if (phi.gcd(this.e) == 1)
            {
                N = P * Q;
                D = this.e.modInverse(phi);
                Dmp1 = D % (P - 1);
                Dmq1 = D % (Q - 1);
                Coeff = Q.modInverse(P);
                break;
            }
        }
        CanEncrypt = N != 0 && this.e != 0;
        CanDecrypt = CanEncrypt && D != 0;
        Console.WriteLine(N.ToString(16));
        Console.WriteLine(D.ToString(16));
    }

    public static RsaKey ParsePublicKey(string n, string e) => new RsaKey(new BigInteger(n, 16), Convert.ToInt32(e, 16), 0, 0, 0, 0, 0, 0);

    public static RsaKey ParsePrivateKey(string n, string e,
        string d,
        string p = null, string q = null,
        string dmp1 = null, string dmq1 = null,
        string coeff = null)
    {
        if (p == null)
            return new RsaKey(new BigInteger(n, 16), Convert.ToInt32(e, 16), new BigInteger(d, 16), 0, 0, 0, 0, 0);
        return new RsaKey(new BigInteger(n, 16), Convert.ToInt32(e, 16), new BigInteger(d, 16), new BigInteger(p, 16), new BigInteger(q, 16),
            new BigInteger(dmp1, 16), new BigInteger(dmq1, 16), new BigInteger(coeff, 16));
    }

    public int GetBlockSize() => (N.bitCount() + 7) / 8;

    public byte[] Encrypt(byte[] src) => DoEncrypt(DoPublic, src, Pkcs1PadType.FullByte);

    public byte[] Decrypt(byte[] src) => DoDecrypt(DoPublic, src, Pkcs1PadType.FullByte);

    public byte[] Sign(byte[] src) => DoEncrypt(DoPrivate, src, Pkcs1PadType.FullByte);

    public byte[] Verify(byte[] src) => DoDecrypt(DoPrivate, src, Pkcs1PadType.FullByte);

    private byte[] DoEncrypt(DoCalculateionDelegate method, byte[] src, Pkcs1PadType type)
    {
        try
        {
            var bl = GetBlockSize();
            var paddedBytes = Pkcs1Pad(src, bl, type);
            var m = new BigInteger(paddedBytes);
            if (m == 0) return null;
            var c = method(m);
            if (c == 0) return null;
            return c.getBytes();
        }
        catch
        {
            return null;
        }
    }

    private byte[] DoDecrypt(DoCalculateionDelegate method, byte[] src, Pkcs1PadType type)
    {
        try
        {
            var c = new BigInteger(src);
            var m = method(c);
            if (m == 0) return null;
            var bl = GetBlockSize();
            var bytes = Pkcs1Unpad(m.getBytes(), bl, type);
            return bytes;
        }
        catch
        {
            return null;
        }
    }

    private byte[] Pkcs1Pad(byte[] src, int n, Pkcs1PadType type)
    {
        var bytes = new byte[n];
        var i = src.Length - 1;
        while (i >= 0 && n > 11) bytes[--n] = src[i--];
        bytes[--n] = 0;
        while (n > 2)
        {
            byte x = 0;
            switch (type)
            {
                case Pkcs1PadType.FullByte:
                    x = 0xFF;
                    break;
                case Pkcs1PadType.RandomByte:
                    x = Randomizer.NextByte(1, 255);
                    break;
            }
            bytes[--n] = x;
        }
        bytes[--n] = (byte)type;
        bytes[--n] = 0;
        return bytes;
    }

    private byte[] Pkcs1Unpad(byte[] src, int n, Pkcs1PadType type)
    {
        var i = 0;
        while (i < src.Length && src[i] == 0) ++i;
        if (src.Length - i != n - 1 || src[i] > 2)
        {
            Console.WriteLine("PKCS#1 unpad: i={0}, expected src[i]==[0,1,2], got src[i]={1}", i, src[i].ToString("X"));
            return null;
        }
        ++i;
        while (src[i] != 0)
            if (++i >= src.Length)
                Console.WriteLine("PKCS#1 unpad: i={0}, src[i-1]!=0 (={1})", i, src[i - 1].ToString("X"));
        var bytes = new byte[src.Length - i - 1];
        for (var p = 0; ++i < src.Length; p++) bytes[p] = src[i];
        return bytes;
    }

    protected BigInteger DoPublic(BigInteger m) => m.modPow(E, N);

    protected BigInteger DoPrivate(BigInteger m)
    {
        if (P == 0 && Q == 0)
            return m.modPow(D, N);
        return 0;
    }
}