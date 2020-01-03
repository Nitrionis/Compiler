using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ParserTests
{
	using Parser = Parser.Parser;

	[TestClass]
	public class ParserTests
	{
		[TestMethod]
		public void Test_1()
		{
			new Parser(GenerateStreamFromString("a = b && c || d * (int)(float)(char)(string)-e.a.b.c(a+b, c-d);"));
		}

		[TestMethod]
		public void Test_2()
		{
			new Parser(GenerateStreamFromString("a = new int[3][][][] {new int[1][][]{ 0 },new int[1][][]{},new int[1][][]{}};"));
		}

		private static MemoryStream GenerateStreamFromString(string value) => 
			new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
	}
}
