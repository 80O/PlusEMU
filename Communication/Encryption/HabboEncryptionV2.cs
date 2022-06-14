using Plus.Communication.Encryption.Crypto.RSA;
using Plus.Communication.Encryption.KeyExchange;
using Plus.Communication.Encryption.Keys;
using Plus.Utilities;
using System.Text;

namespace Plus.Communication.Encryption;

public static class HabboEncryptionV2
{
    private static RsaKey _rsa;
    private static DiffieHellman _diffieHellman;

    public static void Initialize(RsaKeys keys)
    {
        _rsa = RsaKey.ParsePrivateKey(keys.N, keys.E, keys.D);
        _diffieHellman = new DiffieHellman();
    }

    private static string GetRsaStringEncrypted(string message)
    {
        try
        {
            var m = Encoding.Default.GetBytes(message);
            var c = _rsa.Sign(m);
            return Converter.BytesToHexString(c);
        }
        catch
        {
            return "0";
        }
    }

    public static string GetRsaDiffieHellmanPrimeKey()
    {
        var key = _diffieHellman.Prime.ToString(10);
        return GetRsaStringEncrypted(key);
    }

    public static string GetRsaDiffieHellmanGeneratorKey()
    {
        var key = _diffieHellman.Generator.ToString(10);
        return GetRsaStringEncrypted(key);
    }

    public static string GetRsaDiffieHellmanPublicKey()
    {
        var key = _diffieHellman.PublicKey.ToString(10);
        return GetRsaStringEncrypted(key);
    }

    public static BigInteger CalculateDiffieHellmanSharedKey(string publicKey)
    {
        try
        {
            var cbytes = Converter.HexStringToBytes(publicKey);
            var publicKeyBytes = _rsa.Verify(cbytes);
            var publicKeyString = Encoding.Default.GetString(publicKeyBytes);
            return _diffieHellman.CalculateSharedKey(new BigInteger(publicKeyString, 10));
        }
        catch
        {
            return 0;
        }
    }
}