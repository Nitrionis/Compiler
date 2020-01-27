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
		[TestMethod] public void Test_2() => Run(2);
		[TestMethod] public void Test_3() => Run(3);
		[TestMethod] public void Test_4() => Run(4);
		[TestMethod] public void Test_5() => Run(5);
		[TestMethod] public void Test_6() => Run(6);

		private void Run(int testIndex)
		{
			var sourcePath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
			var path = Path.Combine(sourcePath, string.Format("RuntimeTests/RuntimeTest_{0}_in.txt", testIndex));
			Console.WriteLine("↓------------------Input----------------↓\n");
			var input = File.ReadAllText(path);
			Console.WriteLine(input);
			Console.WriteLine("\n↓------------------Output----------------↓\n");
			var runtime = new Parser.Runtime(path);
			var output = "";
			try {
				runtime.Execute();
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
