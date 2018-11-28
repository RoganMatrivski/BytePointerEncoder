## BytePointerEncoder
##### TODO : Change the title to something that makes sense. Idk what this algorithm is called.
---
### What is this?
Simply put, this program encodes your data, paired with a key file as a reference, to an array of hexes.

### Wait, what?
Look. I don't know what algorithm is this called. The only thing i know, is this thing is loading the file, load a key file as a reference for encoding the file, and \**poof*\*, the encoded file is not making sense for any program other than this program, or the program that implements this algorithm.

### What's the algorithm then?
Do you know that game where you given an array of number and a text for reference, and you figure out that the number addresses to a word/letter somewhere on that text that you've given? This software does the same, although the word/letter is replaced with bytes. 
This program convert an array of byte, into an array of address. Those address is pointing to a byte somewhere within the array of byte that are used for a key. If you can trace all of the bytes that are pointed, and put them all in order, you have the original file!

### What does the encoded file looks like?
While i do said the encoded file will contain an array of address, i added some stuff in there to make the software work correctly.

First line, the checksum of the key file is added to make sure that the key file for decoding is the same as when encoding the file.
Second line, is the array of address, mixed with a single hex number. What is that hex number do? Well, it's just to store the length of the address for the program to read next. If you see the encoded file, it doesn't have any separator that splits the addresses, and i do want to use that method, but it lacks the *aesthetics*. So i use this method. Looks nice, isn't it?

### What packages do you use?
I use only two packages. Which is 
* [ShellProgressBar](https://github.com/Mpdreamz/shellprogressbar) - Displays the progress of your program with the aesthetics of the good ol'progress bar
* [Command Line Parser](https://github.com/Mpdreamz/shellprogressbar) - Parsing your command line argument easily.

Both of them uses [MIT License](https://opensource.org/licenses/MIT), so definitely check it out. It's great.

### Why use C#? 
Coz i'm suck at C++, and i'm scared on looking at it.

\**shudders\**
