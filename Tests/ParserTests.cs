using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser;

namespace ParserTests
{
	[TestClass]
	public class ParserTests
	{
		[TestMethod] public void Test_1() => Run(1);
		[TestMethod] public void Test_2() => Run(2);
		[TestMethod] public void Test_3() => Run(3);
		[TestMethod] public void Test_4() => Run(4);
		[TestMethod] public void Test_5() => Run(5);
		[TestMethod] public void Test_6() => Run(6);
		[TestMethod] public void Test_7() => Run(7);
		[TestMethod] public void Test_8() => Run(8);
		[TestMethod] public void Test_9() => Run(9);
		[TestMethod] public void Test_10() => Run(10);
		[TestMethod] public void Test_11() => Run(11);
		[TestMethod] public void Test_12() => Run(12);
		[TestMethod] public void Test_13() => Run(13);
		[TestMethod] public void Test_14() => Run(14);
		[TestMethod] public void Test_15() => Run(15);
		[TestMethod] public void Test_16() => Run(16);
		[TestMethod] public void Test_17() => Run(17);
		[TestMethod] public void Test_18() => Run(18);
		[TestMethod] public void Test_19() => Run(19);
		[TestMethod] public void Test_20() => Run(20);
		[TestMethod] public void Test_22() => Run(22);
		[TestMethod] public void Test_23() => Run(23);
		[TestMethod] public void Test_24() => Run(24);
		[TestMethod] public void Test_25() => Run(25);
		[TestMethod] public void Test_26() => Run(26);
		[TestMethod] public void Test_27() => Run(27);
		[TestMethod] public void Test_28() => Run(28);
		[TestMethod] public void Test_29() => Run(29);
		[TestMethod] public void Test_30() => Run(30);
		[TestMethod] public void Test_31() => Run(31);
		[TestMethod] public void Test_32() => Run(32);
		[TestMethod] public void Test_33() => Run(33);
		[TestMethod] public void Test_34() => Run(34);
		[TestMethod] public void Test_35() => Run(35);
		[TestMethod] public void Test_36() => Run(36);
		[TestMethod] public void Test_37() => Run(37);
		[TestMethod] public void Test_38() => Run(38);
		[TestMethod] public void Test_39() => Run(39);
		[TestMethod] public void Test_40() => Run(40);
		[TestMethod] public void Test_41() => Run(41);
		[TestMethod] public void Test_42() => Run(42);
		[TestMethod] public void Test_43() => Run(43);
		[TestMethod] public void Test_44() => Run(44);
		[TestMethod] public void Test_45() => Run(45);
		[TestMethod] public void Test_46() => Run(46);
		[TestMethod] public void Test_47() => Run(47);

		private void Run(int testIndex)
		{
			var path = string.Format("ParserTests/ParserTest_{0}_in.txt", testIndex);
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
			var validOutput = File.ReadAllText(string.Format("ParserTests/ParserTest_{0}_out.txt", testIndex));
			Assert.IsTrue(output.Equals(validOutput));
		}
	}
}
