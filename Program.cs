using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace JASM {
	public static class Assembler {
		static readonly string EOL = Environment.NewLine;
		// Dictionary of opcodes and their numeric representation
		static readonly Dictionary<string, int> opcodes = new Dictionary<string, int>() {
			{"DATA", 0000},
			{"TAKE", 1000},
			{"ADD",  2000},
			{"SUB",  3000},
			{"SAVE", 4000},
			{"JMP",  5000},
			{"TST",  6000},
			{"INC",  7000},
			{"DEC",  8000},
			{"NULL", 9000},
			{"HLT", 10000}
		};
		static void Main(string[] args) {
			string inFile = "program.jasm";
			string outFile = "output.ram";
			// Iterate over arguments to update default input/output file paths
			for (int i = 0; i < args.Length; i++) {
				// Input file
				if (   args[i].StartsWith("-i:")
					|| args[i].StartsWith("--input:")
					|| args[i].StartsWith("--inputFile:")
					) {
					// Get the file path from the parameter
					inFile = getParam(args[i], ["-i:", "--input:", "--inputFile:"]);
				}
				// Output file
				else if (args[i].StartsWith("-o:")
					  || args[i].StartsWith("--output:")
					  || args[i].StartsWith("--outputFile:")
					) {
					// Get the file path from the parameter
					outFile = getParam(args[i], ["-o:", "--output:", "--outputFile:"]);
				}
				else {
					// Invalid argument, display help and exit
					help();
					return;
				}
			}

			// If the input file doesn't exist, exit
			if (!File.Exists(inFile)) throw new FileNotFoundException($"The provided input file \"{inFile.Trim('"')}\" does not exist.");

			// Get file contents
			string[] jasm = File.ReadAllLines(inFile);
			List<string> result = new List<string>();

			// Time how long it takes to assemble
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			// Iterate over all memory addresses
			for (int i = 0; i < 1000; i++) {
				int compiledInstruction = 0;
				// If the address has a corresponding line in JASM code, process it
				if (i < jasm.Length) {
					string[] jasmInstruction = jasm[i].Split([' ','	']);

					if (jasmInstruction.Length > 2) Console.WriteLine($"[WARN] Instruction in line {i} contains excessive parameters. Ignoring.");
					if (jasmInstruction[0] != "") {
						// Process an opcode
						try {
							compiledInstruction += opcodes[jasmInstruction[0]];
						} catch (KeyNotFoundException) {
							Console.WriteLine($"[ERROR] Opcode \"{jasmInstruction[0]}\" in line {i} is unknown. Terminating.");
							return;
						}
						if (jasmInstruction.Length > 1) {
							// Process an operand
							try {
								compiledInstruction += Convert.ToInt32(jasmInstruction[1]);
							} catch (FormatException) {
								Console.WriteLine($"[ERROR] Invalid parameter \"{jasmInstruction[1]}\" in line {i}. Terminating.");
								return;
							}
						}
					}
				}
				// Write the generated data value to the output buffer
				result.Add(compiledInstruction.ToString());
			}
			// Write output buffer to output file
			File.WriteAllLines(outFile, result);
			// Stop compilation timer
			stopwatch.Stop();
			Console.WriteLine($"Successfully compiled \"{inFile.Trim('"')}\" to \"{outFile.Trim('"')}\" in {stopwatch.Elapsed.Milliseconds}ms");
		}
		static void help() {
			Console.WriteLine(
				  "Usage of the JOHNNY2 Assembly Compiler:" + EOL
				+ EOL
				+ "	--inputFile:\"path\\to\\file.jasm\"	JOHNNY2 Assembly file to use (Default: \"program.jasm\")" + EOL
				+ "	  Shorthands: -i, --input" + EOL
				+ EOL
				+ "	--outputFile:\"path\\to\\file.ram\" Output file destination (Default: \"output.ram\")" + EOL
				+ "	  Shorthands: -o, --output" + EOL
				);
		}
		static string getParam(string rawArg, string[] argAliases) {
			string regexPattern = "^(";
			for (int i = 0; i < argAliases.Length;i++) {
				if (i != 0) regexPattern += "|";
				regexPattern += argAliases[i];
			}
			regexPattern += ")";
			Regex regex = new Regex(regexPattern);
			return regex.Replace(rawArg, "");
		}
	}
}