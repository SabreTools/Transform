using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Transform
{
	// Transform files because we can
	public class Transform
	{
		/// <summary>
		/// Main entry point into the program
		/// </summary>
		public static void Main(string[] args)
		{
			// Create the vars
			TransformOperation operation = TransformOperation.None;
			List<string> files = new List<string>();
			string output = "interleaved.bin";

			// If we have no args, complain
			if (args.Length == 0)
			{
				Console.WriteLine("Need at least one argument");
				Help();
				return;
			}

			// Otherwise, we figure out what the heck we're dealing with
			foreach (string arg in args)
			{
				switch(arg)
				{
					case "-?":
					case "-h":
					case "--help":
						Help();
						return;
					case "-bi":
					case "--bitswap":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.Bitswap;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
					case "-by":
					case "--byteswap":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.Byteswap;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
					case "-w":
					case "--wordswap":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.Wordswap;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
					case "-wb":
					case "--wordbyteswap":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.WordByteswap;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
					case "-inb":
					case "--inter-byte":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.InterleaveByte;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
					case "-inw":
					case "--inter-word":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.InterleaveWord;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
					case "-sb":
					case "--split-byte":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.SplitOneByte;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
                    case "-ss":
					case "-sw":
                    case "--split-short":
					case "--split-word":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.SplitTwoBytes;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
					case "-si":
					case "--split-int":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.SplitFourBytes;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
					case "-sl":
					case "--split-long":
						if (operation == TransformOperation.None)
						{
							operation = TransformOperation.SplitEightBytes;
						}
						else
						{
							Console.Error.WriteLine("Only one transform flag allowed!");
							Help();
							return;
						}
						break;
					default:
						// If we have a file, add it as a valid input
						if (File.Exists(arg))
						{
							files.Add(arg);
						}
						// If we have a directory, add all subfiles... ALL SUBFILES
						else if (Directory.Exists(arg))
						{
							files.AddRange(Directory.EnumerateFiles(arg, "*", SearchOption.AllDirectories));
						}
						// If it's an output name, set that instead of interleaved
						else if (arg.StartsWith("-out=") || arg.StartsWith("--out="))
						{
							output = String.Join("=", arg.Split('=').Skip(1));
						}
						// Otherwise, it's an invalid flag
						else
						{
							Console.Error.WriteLine("Invalid flag: {0}", arg);
							Help();
							return;
						}
						break;
				}
			}

			// If we found no files, complain
			if (files.Count == 0)
			{
				Console.Error.WriteLine("Need at least one file");
				Help();
				return;
			}

			// If we have the interleave operation, we need at least 2 files
			if ((operation == TransformOperation.InterleaveByte || operation == TransformOperation.InterleaveWord) && files.Count < 2)
			{
				Console.Error.WriteLine("Interleaving requires at least 2 files");
				Help();
				return;
			}

			// If we have the interleave operation, run that separately
			if (operation == TransformOperation.InterleaveByte || operation == TransformOperation.InterleaveWord)
			{
				InterleaveFiles(files, output, operation);
			}

			// If we have a split operation, run the operation on everything
			else if (operation >= TransformOperation.SplitOneByte)
			{
				foreach (string file in files)
				{
					SplitFile(file, operation);
				}
			}

			// Otherwise, let's run the operation on EVERY SINGLE FILE
			else
			{
				foreach (string file in files)
				{
					TransformFile(file, file + ".new", operation);
				}
			}
		}

		/// <summary>
		/// Show the generic help text
		/// </summary>
		private static void Help()
		{
			Console.WriteLine(@"Transform - Transform files using standard operations
-------------------------------------
Usage: Transform.exe [-bi | -by | -w | -b] <file> ...

-?, -h, --help            Show this help
-bi, --bitswap            Bitswap the inputs
-by, --byteswap           Byteswap the inputs
-w, --wordswap            Wordswap the inputs
-wb, --wordbyteswap       Word-byteswap the inputs
-inb, --inter-byte        Interleave inputs by byte
    -out=, --out=             Replace the default output name
-inw, --inter-word        Interleave inputs by word
    -out=, --out=             Replace the default output name
-sb, --split-byte         Split inputs by byte (even/odd)
-ss, --split-short        Split inputs by 2-byte short (even/odd)
-sw, --split-word         Split inputs by 2-byte word (even/odd)
-si, --split-int          Split inputs by 4-byte ints (even/odd)
-sl, --split-long         Split inputs by 8-byte longs (even/odd)
");
		}

		/// <summary>
		/// Determines the header skip operation
		/// </summary>
		private enum TransformOperation
		{
			// Default
			None = 0,
			
			// Swaping operations
			Bitswap,
			Byteswap,
			Wordswap,
			WordByteswap,
			
			// Interleaving operations
			InterleaveByte,
			InterleaveWord,
			
			// Splitting operations
			SplitOneByte,
			SplitTwoBytes,
			SplitFourBytes,
			SplitEightBytes,
		}

		/// <summary>
		/// Interleave a set of files together to a single file
		/// </summary>
		/// <param name="inputs">List of valid files as inputs</param>
		/// <param name="output">Name of the output file</param>
		/// <param name="operation">Either InterleaveByte or InterleaveWord</param>
		/// <returns>True if the files were interleaved successfully, false otherwise</returns>
		private static bool InterleaveFiles(List<string> inputs, string output, TransformOperation operation)
		{
			// If the inputs are empty, return
			if (inputs.Count == 0)
			{
				return false;
			}

			// If we don't have at least 2 files, return
			if (inputs.Count < 2)
			{
				return false;
			}

			// If we don't have a proper operation, return
			if (operation != TransformOperation.InterleaveByte && operation != TransformOperation.InterleaveWord)
			{
				return false;
			}

			// Get the number of bytes to read
			int bytecount = 0;
			if (operation == TransformOperation.InterleaveByte)
			{
				bytecount = 1;
			}
			else if (operation == TransformOperation.InterleaveWord)
			{
				bytecount = 2;
			}

			// Get a list of file streams from the inputs
			List<BinaryReader> readers = new List<BinaryReader>();
			foreach (string input in inputs)
			{
				if (!File.Exists(input))
				{
					Console.WriteLine(input + " is not a valid file, exiting");
					return false;
				}
				readers.Add(new BinaryReader(File.OpenRead(input)));
			}

			// Open the output file
			var writer = new BinaryWriter(File.Open(output, FileMode.Create, FileAccess.Write));

			// Write the input files and sizes to the console
			for (int i = 0; i < inputs.Count; i++)
			{
				Console.WriteLine(inputs[i] + ": " + readers[i].BaseStream.Length);
			}

			// For each file, read and then write to the output
			while (readers[0].BaseStream.Position < readers[0].BaseStream.Length)
			{
				foreach (var reader in readers)
				{
					writer.Write(reader.ReadBytes(bytecount));
				}
			}

			writer.Dispose();
			foreach (var reader in readers)
			{
				reader.Dispose();
			}

			return true;
		}

		/// <summary>
		/// Split a file either on byte or word
		/// </summary>
		/// <param name="input">Input file name</param>
		/// <param name="operation">Split*Bytes operations</param>
		/// <returns></returns>
		private static bool SplitFile(string input, TransformOperation operation)
		{
			// If the input is null, retrn
			if (input == null)
			{
				return false;
			}

			// If the input isn't a file, return
			if (!File.Exists(input))
			{
				return false;
			}

			// If the transform operation isn't a recognized one, return
			if (operation < TransformOperation.SplitOneByte)
			{
				return false;
			}

			// Get the count we need for bytes to read before swapping
			int bytecount = 0;
			if (operation == TransformOperation.SplitOneByte)
			{
				bytecount = 1;
			}
			else if (operation == TransformOperation.SplitTwoBytes)
			{
				bytecount = 2;
			}
			else if (operation == TransformOperation.SplitFourBytes)
			{
				bytecount = 4;
			}
			else if (operation == TransformOperation.SplitEightBytes)
			{
				bytecount = 8;
			}

			// Open the input file and create two outputs, one for odd and one for even
			FileStream inputStream = File.Open(input, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			FileStream evenStream = File.Open(input + ".even", FileMode.Create, FileAccess.Write);
			FileStream oddStream = File.Open(input + ".odd", FileMode.Create, FileAccess.Write);

			// Now get the binary readers and writers
			BinaryReader bri = new BinaryReader(inputStream);
			BinaryWriter bwe = new BinaryWriter(evenStream);
			BinaryWriter bwo = new BinaryWriter(oddStream);

			// Now we loop and flip as we go
			bool even = true;
			while (bri.BaseStream.Position < inputStream.Length)
			{
				// If we're writing to even
				if (even)
				{
					bwe.Write(bri.ReadBytes(bytecount));
					bwe.Flush();
				}
				// Otherwise writing to odd
				else
				{
					bwo.Write(bri.ReadBytes(bytecount));
					bwo.Flush();
				}

				even = !even;
			}

			// Finally dispose of the readers and writers
			bri.Dispose();
			bwe.Dispose();
			bwo.Dispose();

			return true;
		}

		/// <summary>
		/// Transform an input file using the given rule
		/// </summary>
		/// <param name="input">Input file name</param>
		/// <param name="output">Output file name</param>
		/// <param name="operation">Transform operation to carry out</param>
		/// <returns>True if the file was transformed properly, false otherwise</returns>
		private static bool TransformFile(string input, string output, TransformOperation operation)
		{
			bool success = true;

			// If the input file doesn't exist, fail
			if (!File.Exists(input))
			{
				Console.Error.WriteLine("I'm sorry but '" + input + "' doesn't exist!");
				return false;
			}

			// Create the output directory if it doesn't already
			if (!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(output))))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(output));
			}

			try
			{
				FileStream inputStream = File.Open(input, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				FileStream outputStream = File.Open(output, FileMode.Create, FileAccess.Write);
				success = TransformStream(inputStream, outputStream, operation);

				// If the output file has size 0, delete it
				if (new FileInfo(output).Length == 0)
				{
					File.Delete(output);
					success = false;
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
			}

			return success;
		}

		/// <summary>
		/// Transform an input stream using the given rule
		/// </summary>
		/// <param name="input">Input stream</param>
		/// <param name="output">Output stream</param>
		/// <param name="operation">Transform operation to carry out</param>
		/// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
		/// <param name="keepWriteOpen">True if the underlying write stream should be kept open, false otherwise</param>
		/// <returns>True if the file was transformed properly, false otherwise</returns>
		private static bool TransformStream(Stream input, Stream output, TransformOperation operation, bool keepReadOpen = false, bool keepWriteOpen = false)
		{
			bool success = true;

			// If the sizes are wrong for the values, fail
			long extsize = input.Length;
			if ((operation == TransformOperation.Bitswap && (extsize % 2) != 0)
				|| (operation == TransformOperation.Byteswap && (extsize % 4) != 0)
				|| (operation == TransformOperation.Bitswap && (extsize % 4) != 0))
			{
				Console.Error.WriteLine("The stream did not have the correct size to be transformed!");
				return false;
			}

			// Now read the proper part of the file and apply the rule
			BinaryWriter bw = null;
			BinaryReader br = null;
			try
			{
				bw = new BinaryWriter(output);
				br = new BinaryReader(input);

				// Seek to the beginning offset
				br.BaseStream.Seek(0, SeekOrigin.Begin);

				// Then read and apply the operation as you go
				if (success)
				{
					byte[] buffer = new byte[4];
					int pos = 0;
					while (input.Position < input.Length)
					{
						byte b = br.ReadByte();
						switch (operation)
						{
							case TransformOperation.Bitswap:
								// http://stackoverflow.com/questions/3587826/is-there-a-built-in-function-to-reverse-bit-order
								uint r = b;
								int s = 7;
								for (b >>= 1; b != 0; b >>= 1)
								{
									r <<= 1;
									r |= (byte)(b & 1);
									s--;
								}
								r <<= s;
								buffer[pos] = (byte)r;
								break;
							case TransformOperation.Byteswap:
								if (pos % 2 == 1)
								{
									buffer[pos - 1] = b;
								}
								if (pos % 2 == 0)
								{
									buffer[pos + 1] = b;
								}
								break;
							case TransformOperation.Wordswap:
								buffer[3 - pos] = b;
								break;
							case TransformOperation.WordByteswap:
								buffer[(pos + 2) % 4] = b;
								break;
							case TransformOperation.None:
							default:
								buffer[pos] = b;
								break;
						}

						// Set the buffer position to default write to
						pos = (pos + 1) % 4;

						// If we filled a buffer, flush to the stream
						if (pos == 0)
						{
							bw.Write(buffer);
							bw.Flush();
							buffer = new byte[4];
						}
					}
					// If there's anything more in the buffer, write only the left bits
					for (int i = 0; i < pos; i++)
					{
						bw.Write(buffer[i]);
					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.ToString());
				return false;
			}
			finally
			{
				// If we're not keeping the read stream open, dispose of the binary reader
				if (!keepReadOpen)
				{
					br?.Dispose();
				}

				// If we're not keeping the write stream open, dispose of the binary reader
				if (!keepWriteOpen)
				{
					bw?.Dispose();
				}
			}

			return success;
		}
	}
}