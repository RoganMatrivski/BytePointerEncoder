using System;
using System.Runtime;

using System.IO.Compression;
using System.IO;

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

    static public byte[] trim(byte[] packet)
    {
        var i = packet.Length - 1;
        while (packet[i] == 0)
        {
            --i;
        }
        var temp = new byte[i + 1];
        Array.Copy(packet, temp, i + 1);
        return temp;
    }

    static public byte[] compress(byte[] data)
    {
        using (MemoryStream memstream = new MemoryStream())
        using (MemoryStream srcstream = new MemoryStream(data))
        using (DeflateStream deflate = new DeflateStream(memstream, CompressionLevel.Optimal))
        {
            srcstream.CopyTo(deflate);

            deflate.Close();
            return memstream.ToArray();
        }
    }

    static public void compress_to_file(byte[] data, string path)
    {
        using (FileStream deststream = new FileStream(path, FileMode.Create))
        using (MemoryStream srcstream = new MemoryStream(data))
        using (DeflateStream deflate = new DeflateStream(deststream, CompressionLevel.Optimal))
        {
            srcstream.CopyTo(deflate);
        }
    }

    static public byte[] decompress(byte[] data)
    {
        using (MemoryStream memstream = new MemoryStream())
        using (MemoryStream srcstream = new MemoryStream(data))
        using (DeflateStream inflate = new DeflateStream(srcstream, CompressionMode.Decompress))
        {
            inflate.CopyTo(memstream);
            inflate.Close();
            return memstream.ToArray();
        }
    }

    static public void decompress_from_file(byte[] data, string path)
    {
        using (MemoryStream deststream = new MemoryStream())
        using (FileStream srcstream = new FileStream(path, FileMode.Open))
        using (DeflateStream inflate = new DeflateStream(srcstream, CompressionMode.Decompress))
        {
            inflate.CopyTo(deststream);
        }
    }
}
