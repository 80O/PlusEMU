namespace Plus.Utilities;

public static class Randomizer
{
    public static byte NextByte() => (byte)Random.Shared.Next(0, 255);

    public static byte NextByte(int max)
    {
        max = Math.Min(max, 255);
        return (byte)Random.Shared.Next(0, max);
    }

    public static byte NextByte(int min, int max)
    {
        max = Math.Min(max, 255);
        return (byte)Random.Shared.Next(Math.Min(min, max), max);
    }

    public static void NextBytes(byte[] toparse) => Random.Shared.NextBytes(toparse);
}