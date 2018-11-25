using System;

using CommandLine;
using ShellProgressBar;

public static class Configurations
{
    [Verb("encode", HelpText = "Encode a file with another assigned file")]
    public class encode_option
    {
        [Option('i', "input", Required = true, HelpText = "File to be encoded")]
        public string input_path { get; set; }

        [Option('k', "key", Required = true, HelpText = "File that will be used as a key for encoding")]
        public string key_path { get; set; }

        [Option('o', "output", Required = false, HelpText = "File output name. Will be named as [filename]_result.[ext] if left blank")]
        public string output_path { get; set; }
    }

    [Verb("decode", HelpText = "Decode a file with another assigned file")]
    public class decode_option
    {
        [Option('i', "input", Required = true, HelpText = "File to be decoded")]
        public string input_path { get; set; }

        [Option('k', "key", Required = true, HelpText = "File that will be used as a key for decoding")]
        public string key_path { get; set; }

        [Option('o', "output", Required = false, HelpText = "File output name. Will be named as [filename]_result.[ext] if left blank")]
        public string output_path { get; set; }
    }

    public static ProgressBarOptions options = new ProgressBarOptions
    {
        ForegroundColor = ConsoleColor.Yellow,
        BackgroundColor = ConsoleColor.DarkGray,
        ProgressCharacter = '-'
    };

    public static ProgressBarOptions childOptions = new ProgressBarOptions
    {
        ForegroundColor = ConsoleColor.Green,
        BackgroundColor = ConsoleColor.DarkGray,
        ProgressCharacter = '─'
    };
}