using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;
using System.Security.Cryptography;

using System.IO;

using CommandLine;

namespace TestPointerCodeByte
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<encode_option, decode_option>(args).MapResult(
                (encode_option opts) =>
                {
                    Console.WriteLine($"{opts.input_path}, {opts.key_path}, {opts.output_path}");
                    return 1;
                },
                (decode_option opts) =>
                {
                    Console.WriteLine($"{opts.input_path}, {opts.key_path}, {opts.output_path}");

                    return 1;
                },
                errs => 1
                );

            var ref_bytes = File.ReadAllBytes(@"D:\Gim\osu!\osu!.exe");
            string ref_hash = get_hash(ref_bytes);

            byte[] dataset = new byte[256];
            for (int i = 0; i < dataset.Length; i++)
                dataset[i] = Convert.ToByte(i);

            long[][] positions = new long[256][];

            for (int j = 0; j < dataset.Length; j++)
            {
                //List<long> pos = new List<long>();
                ConcurrentBag<long> pos = new ConcurrentBag<long>();
                //for (long i = 0; i < bytes.Length; i++)
                Parallel.For(0, ref_bytes.Length, (i) =>
                {
                    if (ref_bytes[i] == dataset[j])
                        pos.Add(i);
                });

                if (pos.Count > 0)
                {
                    //positions.Add(pos.ToArray());
                    positions[j] = pos.ToArray();

                    continue;
                }

                break;
            }

            var src_bytes = File.ReadAllBytes(@"D:\Gim\NotITG\Songs\Other\NEO GRAVITY\grav.sm");

            Random rand = new Random(1337);

            using (StreamWriter writer = new StreamWriter(new FileStream("Testing.txt", FileMode.Create), Encoding.UTF8))
            {
                writer.WriteLine(ref_hash);
                foreach (var ibytes in src_bytes)
                {
                    //var pos = positions[ibytes][rand.Next(positions[ibytes].Length)];
                    var pos = long_to_hex(positions[ibytes][rand.Next(positions[ibytes].Length)]);
                    writer.Write(int_to_hex(pos.Length) + pos);
                }
            }

            List<byte> container = new List<byte>();
            using (StreamReader reader = new StreamReader(new FileStream("Testing.txt", FileMode.Open), Encoding.UTF8))
            {
                if (reader.ReadLine() != get_hash(ref_bytes))
                    throw new Exception("Hash not the same");

                while (!reader.EndOfStream)
                {
                    int charcount = 0;
                    char char_read = (char)reader.Read();
                    try
                    {
                        charcount = hex_to_int(char_read.ToString());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("asdasdaf");
                    }

                    char[] hex = new char[charcount];
                    for (int j = 0; j < charcount; j++)
                    {
                        hex[j] = (char)reader.Read();
                    }

                    long pointer = hex_to_long(new string(hex));
                    container.Add(ref_bytes[pointer]);
                }
            }

            File.WriteAllBytes("grav1.sm", container.ToArray());
        }

        static void encode(string input_path, string key_path, string output_path)
        {
            var ref_bytes = File.ReadAllBytes(key_path);
            string ref_hash = get_hash(ref_bytes);

            byte[] dataset = new byte[256];
            for (int i = 0; i < dataset.Length; i++)
                dataset[i] = Convert.ToByte(i);

            long[][] positions = new long[256][];

            for (int j = 0; j < dataset.Length; j++)
            {
                //List<long> pos = new List<long>();
                ConcurrentBag<long> pos = new ConcurrentBag<long>();
                //for (long i = 0; i < bytes.Length; i++)
                Parallel.For(0, ref_bytes.Length, (i) =>
                {
                    if (ref_bytes[i] == dataset[j])
                        pos.Add(i);
                });

                if (pos.Count > 0)
                {
                    //positions.Add(pos.ToArray());
                    positions[j] = pos.ToArray();

                    continue;
                }

                break;
            }

            var src_bytes = File.ReadAllBytes(input_path);

            Random rand = new Random(1337);

            using (StreamWriter writer = new StreamWriter(output_path, false, Encoding.UTF8))
            {
                writer.WriteLine(ref_hash);
                foreach (var ibytes in src_bytes)
                {
                    //var pos = positions[ibytes][rand.Next(positions[ibytes].Length)];
                    var pos = long_to_hex(positions[ibytes][rand.Next(positions[ibytes].Length)]);
                    writer.Write(int_to_hex(pos.Length) + pos);
                }
            }
        }

        static void decode(string input_path, string key_path, string output_path)
        {
            var ref_bytes = File.ReadAllBytes(key_path);

            List<byte> container = new List<byte>();
            using (StreamReader reader = new StreamReader(input_path, Encoding.UTF8))
            {
                if (reader.ReadLine() != get_hash(ref_bytes))
                    throw new Exception("Hash not the same");

                while (!reader.EndOfStream)
                {
                    int charcount = 0;
                    char char_read = (char)reader.Read();
                    try
                    {
                        charcount = hex_to_int(char_read.ToString());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Houston, we get an illegal character on the input file");
                    }

                    char[] hex = new char[charcount];
                    for (int j = 0; j < charcount; j++)
                    {
                        hex[j] = (char)reader.Read();
                    }

                    long pointer = hex_to_long(new string(hex));
                    container.Add(ref_bytes[pointer]);
                }
            }

            File.WriteAllBytes(output_path, container.ToArray());
        }

        static string get_hash(byte[] data)
        {
            string result;
            using (SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider())
                result = Convert.ToBase64String(sha.ComputeHash(data));

            return result;
        }

        static string long_to_hex(long src)
        {
            return src.ToString("X");
        }

        static long hex_to_long(string src)
        {
            return long.Parse(src, System.Globalization.NumberStyles.HexNumber);
        }

        static int hex_to_int(string src)
        {
            return int.Parse(src, System.Globalization.NumberStyles.HexNumber);
        }

        static string int_to_hex(long src)
        {
            return src.ToString("X");
        }

        [Verb("encode", HelpText = "Encode a file with another assigned file")]
        class encode_option
        {
            [Option('i', "input", Required = true, HelpText = "File to be encoded")]
            public string input_path { get; set; }

            [Option('k', "key", Required = true, HelpText = "File that will be used as a key for encoding")]
            public string key_path { get; set; }

            [Option('o', "output", Required = false, HelpText = "File output name. Will be named as [filename]_result.[ext] if left blank")]
            public string output_path { get; set; }
        }

        [Verb("decode", HelpText = "Decode a file with another assigned file")]
        class decode_option
        {
            [Option('i', "input", Required = true, HelpText = "File to be decoded")]
            public string input_path { get; set; }

            [Option('k', "key", Required = true, HelpText = "File that will be used as a key for decoding")]
            public string key_path { get; set; }

            [Option('o', "output", Required = false, HelpText = "File output name. Will be named as [filename]_result.[ext] if left blank")]
            public string output_path { get; set; }
        }
    }
}
