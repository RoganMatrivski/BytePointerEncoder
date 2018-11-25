using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

using System.IO;

using CommandLine;
using ShellProgressBar;

using static Tools;
using static Configurations;

namespace TestPointerCodeByte
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<encode_option, decode_option>(args).MapResult(
                (encode_option opts) =>
                {
                    string output = opts.output_path;

                    if (output == null)
                    {
                        var fileinfo = new FileInfo(opts.input_path);

                        output = $"{Path.GetFileNameWithoutExtension(fileinfo.Name)}_result{fileinfo.Extension}";
                    }

                    encode(opts.input_path, opts.key_path, output);
                    return 1;
                },
                (decode_option opts) =>
                {
                    string output = opts.output_path;

                    if (output == null)
                    {
                        var fileinfo = new FileInfo(opts.input_path);

                        output = $"{Path.GetFileNameWithoutExtension(fileinfo.Name)}_result{fileinfo.Extension}";
                    }

                    decode(opts.input_path, opts.key_path, output);

                    return 1;
                },
                errs => 1
                );
        }

        static void encode(string input_path, string key_path, string output_path)
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

            using (StreamWriter writer = new StreamWriter(output_path, false, Encoding.UTF8))
            {
                writer.WriteLine(ref_hash);
                foreach (var ibytes in src_bytes)
                {
                    var pos = long_to_hex(positions[ibytes][rand.Next(positions[ibytes].Length)]);
                    writer.Write(int_to_hex(pos.Length) + pos);
                }
            }

            progress_bar.Tick("Done!");

            progress_bar.Dispose();
        }

        static void decode(string input_path, string key_path, string output_path)
        {
            var progress_bar = new ProgressBar(4, "Initializing", options);

            progress_bar.Tick("Loading key file...");

            var ref_bytes = File.ReadAllBytes(key_path); // Step1

            progress_bar.Tick("Reading Encoded file...");

            List<byte> container = new List<byte>();
            using (var child_progress_bar = progress_bar.Spawn(2, "Initializing", childOptions))
            using (StreamReader reader = new StreamReader(input_path, Encoding.UTF8)) // Step2
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

            progress_bar.Tick("Writing decoded file...");

            File.WriteAllBytes(output_path, container.ToArray()); // Step3

            progress_bar.Tick("Done!");
            progress_bar.Dispose();
        }
    }
}
