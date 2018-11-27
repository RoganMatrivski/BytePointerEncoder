using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

using System.IO;

using CommandLine;
using ShellProgressBar;

using System.IO.Compression;

using static Tools;
using static Configurations;

namespace TestPointerCodeByte
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<encode_option, decode_option, compare_option>(args).MapResult(
                (encode_option opts) =>
                {
                    string output = opts.output_path;

                    if (output == null)
                    {
                        var fileinfo = new FileInfo(opts.input_path);

                        output = $"{Path.GetFileNameWithoutExtension(fileinfo.Name)}_result{fileinfo.Extension}";
                    }

                    //encode(opts.input_path, opts.key_path, output, opts.compress_option);
                    encode(opts.input_path, opts.key_path, output, true);
                    return 1;
                },
                (decode_option opts) =>
                {
                    bool isCompressed = false;
                    if (new FileInfo(opts.input_path).Extension == ".deflate")
                        isCompressed = true;

                    string output = opts.output_path;

                    if (output == null)
                    {
                        var fileinfo = new FileInfo(opts.input_path);

                        if (isCompressed)
                            output = $"{Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fileinfo.Name))}_result{new FileInfo(Path.GetFileNameWithoutExtension(fileinfo.Name)).Extension}";
                        else
                            output = $"{Path.GetFileNameWithoutExtension(fileinfo.Name)}_result{fileinfo.Extension}";
                    }

                    //if (isCompressed)
                    //    output = Path.GetFileNameWithoutExtension(output);

                    decode(opts.input_path, opts.key_path, output, isCompressed);

                    return 1;
                },
                (compare_option opts) =>
                {
                    compare(opts.a_path, opts.b_path);

                    return 1;
                },
                errs => 1
                );
        }

        static void encode(string input_path, string key_path, string output_path, bool compress_option)
        {
            var progress_bar = new ProgressBar(5, "Initializing", options);

            progress_bar.Tick("Loading key file...");

            var ref_bytes = File.ReadAllBytes(key_path);
            string ref_hash = get_hash(ref_bytes);

            progress_bar.Tick("Checking if key can be used...");

            byte[] dataset = new byte[256];
            for (int i = 0; i < dataset.Length; i++)
                dataset[i] = Convert.ToByte(i);

            long[][] positions = new long[256][];

            var child_progress_bar = progress_bar.Spawn(256, "Initializing...", childOptions);

            for (int j = 0; j < dataset.Length; j++)
            {
                ConcurrentBag<long> pos = new ConcurrentBag<long>();
                Parallel.For(0, ref_bytes.Length, (i) =>
                {
                    if (ref_bytes[i] == dataset[j])
                        pos.Add(i);
                });

                if (pos.Count > 0)
                {
                    positions[j] = pos.ToArray();
                    child_progress_bar.Tick($"Byte {j}");

                    continue;
                }

                Console.Error.WriteLine("This key file can't be used! Please use another key file.");
                Environment.Exit(1);
            }

            child_progress_bar.Dispose();

            progress_bar.Tick("Loading input file...");

            var src_bytes = File.ReadAllBytes(input_path);

            progress_bar.Tick("Encoding file...");

            Random rand = new Random();

            //using (MemoryStream mem_stream = new MemoryStream())
            //using (StreamWriter mem_writer = new StreamWriter(mem_stream, Encoding.UTF8))
            //{
            //    mem_writer.WriteLine(ref_hash);
            //    foreach (var ibytes in src_bytes)
            //    {
            //        var pos = long_to_hex(positions[ibytes][rand.Next(positions[ibytes].Length)]);
            //        mem_writer.Write(int_to_hex(pos.Length) + pos);
            //    }

            //    debug_read = compress(mem_stream.ToArray());
            //}

            //using (MemoryStream mem_stream = new MemoryStream())
            //using (StreamWriter mem_writer = new StreamWriter(mem_stream, Encoding.UTF8))
            //using (GZipStream gzip = new GZipStream(new FileStream(output_path, FileMode.Create), CompressionLevel.Optimal))
            //{
            //    mem_writer.WriteLine(ref_hash);
            //    foreach (var ibytes in src_bytes)
            //    {
            //        var pos = long_to_hex(positions[ibytes][rand.Next(positions[ibytes].Length)]);
            //        mem_writer.Write(int_to_hex(pos.Length) + pos);
            //    }

            //    debug_read = compress(mem_stream.ToArray());
            //}

            if (compress_option)
            {
                using (MemoryStream mem_stream = new MemoryStream())
                //using (StreamWriter mem_writer = new StreamWriter(mem_stream, Encoding.UTF8))
                //using (GZipStream gzip = new GZipStream(new FileStream(output_path, FileMode.Create), CompressionLevel.Optimal))
                //using (DeflateStream deflate = new DeflateStream(new FileStream(output_path, FileMode.Create), CompressionLevel.Optimal))
                using (var writer =
                    new StreamWriter(
                        new DeflateStream(
                            new FileStream(output_path + ".deflate", FileMode.Create),
                        CompressionLevel.Optimal),
                    Encoding.UTF8))
                {
                    writer.WriteLine(ref_hash);
                    foreach (var ibytes in src_bytes)
                    {
                        var pos = long_to_hex(positions[ibytes][rand.Next(positions[ibytes].Length)]);
                        writer.Write(int_to_hex(pos.Length) + pos);
                    }
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(output_path, false, Encoding.UTF8))
                {
                    writer.WriteLine(ref_hash);
                    foreach (var ibytes in src_bytes)
                    {
                        var pos = long_to_hex(positions[ibytes][rand.Next(positions[ibytes].Length)]);
                        writer.Write(int_to_hex(pos.Length) + pos);
                    }
                }
            }

            //using (var reader = new StreamReader(new DeflateStream(new FileStream(output_path, FileMode.Open), CompressionMode.Decompress), Encoding.UTF8))
            //{
            //    Console.WriteLine(reader.ReadToEnd());
            //}

            //using (StreamReader reader = new StreamReader(new MemoryStream(decompress(debug_read))))
            //{
            //    Console.WriteLine(reader.ReadLine());
            //}

            progress_bar.Tick("Done!");

            progress_bar.Dispose();
        }

        static void decode(string input_path, string key_path, string output_path, bool isCompressed)
        {
            var progress_bar = new ProgressBar(4, "Initializing", options);

            progress_bar.Tick("Loading key file...");

            var ref_bytes = File.ReadAllBytes(key_path); // Step1

            progress_bar.Tick("Reading Encoded file...");

            List<byte> container = new List<byte>();
            StreamReader reader; // Any way to simplify this?

            if (isCompressed)
                reader = new StreamReader(new DeflateStream(new FileStream(input_path, FileMode.Open), CompressionMode.Decompress), Encoding.UTF8);
            else
                reader = new StreamReader(input_path, Encoding.UTF8);

            using (var child_progress_bar = progress_bar.Spawn(2, "Initializing", childOptions))
            //using (StreamReader reader = new StreamReader(input_path, Encoding.UTF8)) // Step2
            {
                child_progress_bar.Tick("Checking if key file hash signature is the same from the encoded file...");
                if (reader.ReadLine() != get_hash(ref_bytes)) //Step2.1
                {
                    progress_bar.Dispose();
                    Console.Error.WriteLine("The hash from the key and from the encoded file is not the same!");
                    Environment.Exit(1);
                }

                // TODO : Revising this code to save the byte while decoding
                child_progress_bar.Tick("Decoding file...");
                while (!reader.EndOfStream) //Step2.2 - end
                {
                    int charcount = 0;
                    char char_read = (char)reader.Read();
                    try
                    {
                        charcount = hex_to_int(char_read.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }

                    char[] hex = new char[charcount];
                    for (int j = 0; j < charcount; j++)
                        hex[j] = (char)reader.Read();

                    long pointer = hex_to_long(new string(hex));
                    container.Add(ref_bytes[pointer]);
                }
            }
            reader.Close();
            reader.Dispose();

            progress_bar.Tick("Writing decoded file...");

            File.WriteAllBytes(output_path, container.ToArray()); // Step3

            progress_bar.Tick("Done!");
            progress_bar.Dispose();
        }

        static void compare(string a, string b)
        {
            int str_count = a.Length; if (a.Length < b.Length) str_count = b.Length; // Small snippet formatting code.

            string a_hash = get_hash(File.ReadAllBytes(a));
            string b_hash = get_hash(File.ReadAllBytes(b));

            if (a_hash == b_hash)
                Console.WriteLine($"{a} and {b} is the same file");
            else
                Console.WriteLine($"{a} and {b} is the same file");

            Console.WriteLine();

            Console.WriteLine("Hash results \n");
            Console.WriteLine($"{a + new string(' ', str_count - a.Length)} : {a_hash}");
            Console.WriteLine($"{b + new string(' ', str_count - b.Length)} : {b_hash}");
            //Console.WriteLine($"{a} : {a_hash}");
            //Console.WriteLine($"{b} : {b_hash}");

            Console.WriteLine("\n");
        }
    }
}
