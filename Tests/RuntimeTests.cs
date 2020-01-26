using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser;

namespace Tests
{
	[TestClass]
	public class RuntimeTests
	{
		[TestMethod] public void Test_1() => Run(1);

		private void Run(int testIndex)
		{
			var sourcePath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
			var path = Path.Combine(sourcePath, string.Format("RuntimeTests/RuntimeTest_{0}_in.txt", testIndex));
			Console.WriteLine("↓------------------Input----------------↓\n");
			var input = File.ReadAllText(path);
			Console.WriteLine(input);
			Console.WriteLine("\n↓------------------Output----------------↓\n");
			var parser = new Parser.Parser(path);
			var output = "";
			try {
				foreach (var v in parser.ParseProgram()) {
					output += v?.ToString() ?? "null\n";
				}
			} catch (ParserException e) {
				output += e.Message;
			}
			Console.WriteLine(output);
			var validOutput = File.ReadAllText(Path.Combine(
				sourcePath, string.Format("RuntimeTests/RuntimeTest_{0}_out.txt", testIndex)));
			Assert.IsTrue(output.Equals(validOutput, StringComparison.OrdinalIgnoreCase));
		}
	}
}
