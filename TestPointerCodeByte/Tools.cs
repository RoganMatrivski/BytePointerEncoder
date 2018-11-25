using System;

public static class Tools
{
    public static string get_hash(byte[] data)
    {
        string result;
        using (System.Security.Cryptography.SHA1CryptoServiceProvider sha = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            result = Convert.ToBase64String(sha.ComputeHash(data));

        return result;
    }

    public static string long_to_hex(long src)
    {
        return src.ToString("X");
    }

    public static long hex_to_long(string src)
    {
        return long.Parse(src, System.Globalization.NumberStyles.HexNumber);
    }

    public static int hex_to_int(string src)
    {
        return int.Parse(src, System.Globalization.NumberStyles.HexNumber);
    }

    public static string int_to_hex(long src)
    {
        return src.ToString("X");
    }
}
