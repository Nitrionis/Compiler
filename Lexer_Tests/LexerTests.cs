using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lexer;
using Shared;

namespace LexerTests
{
	[TestClass]
	public class LexerTests
	{
		private static Lexer.Lexer lexer;

		static LexerTests()
		{
			lexer = new Lexer.Lexer(GenerateStreamFromString(""));
		}

		[TestMethod]
		public void Test_1()
		{
			Assert.IsTrue(new Test("", new Token[0]).Execute().IsDone);
		}

		[TestMethod]
		public void Test_2()
		{
			Assert.IsTrue(new Test("a", new[] { new Token(Token.Types.Identifier, "a", "a", 0, 0) }).Execute().IsDone);
		}

		[TestMethod]
		public void Test_3()
		{
			Assert.IsTrue(new Test("1", new[] { new Token(Token.Types.Int, 1, "1", 0, 0) }).Execute().IsDone);
		}

		[TestMethod]
		public void Test_4()
		{
			Assert.IsTrue(new Test("/", new[] { new Token(Token.Types.Operator, Operator.Divide, "/", 0, 0) }).Execute().IsDone);
		}

		[TestMethod]
		public void Test_5()
		{
			Assert.IsTrue(new Test("//", new Token[0]).Execute().IsDone);
		}

		[TestMethod]
		public void Test_6()
		{
			Assert.IsTrue(new Test("//a", new Token[0]).Execute().IsDone);
		}

		[TestMethod]
		public void Test_7()
		{
			Assert.IsTrue(new Test(
				"//`1234567890-=~!@#$%^&*()_+qwertyuiop[]QWERTYUIOP{}asdfghjkl;'ASDFGHJKL:|\"\\zxcvbnm,./ZXCVBNM<>?",
				new Token[0]
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_8()
		{
			Assert.IsTrue(new Test("&&&", new[]{
				new Token(Token.Types.Operator, Operator.LogicalAnd, "&&", 0, 0),
				new Token(Token.Types.Operator, Operator.BitwiseAnd, "&", 0, 2) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_9()
		{
			Assert.IsTrue(new Test("& &&", new[]{
				new Token(Token.Types.Operator, Operator.BitwiseAnd, "&", 0, 0),
				new Token(Token.Types.Operator, Operator.LogicalAnd, "&&", 0, 2) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_10()
		{
			Assert.IsTrue(new Test("+-*/=&&&!~()[]{}%<>!=|||", new[]{
				new Token(Token.Types.Operator, Operator.Add, "+", 0, 0),
				new Token(Token.Types.Operator, Operator.Subtract, "-", 0, 1),
				new Token(Token.Types.Operator, Operator.Multiply, "*", 0, 2),
				new Token(Token.Types.Operator, Operator.Divide, "/", 0, 3),
				new Token(Token.Types.Operator, Operator.Assignment, "=", 0, 4),
				new Token(Token.Types.Operator, Operator.LogicalAnd,"&&", 0, 5),
				new Token(Token.Types.Operator, Operator.BitwiseAnd, "&", 0, 7),
				new Token(Token.Types.Operator, Operator.LogicalNot, "!", 0, 8),
				new Token(Token.Types.Operator, Operator.BitwiseNot, "~", 0, 9),
				new Token(Token.Types.Operator, Operator.OpenParenthesis, "(", 0, 10),
				new Token(Token.Types.Operator, Operator.CloseParenthesis, ")", 0, 11),
				new Token(Token.Types.Operator, Operator.OpenSquareBracket, "[", 0, 12),
				new Token(Token.Types.Operator, Operator.CloseSquareBracket, "]", 0, 13),
				new Token(Token.Types.Operator, Operator.OpenCurlyBrace, "{", 0, 14),
				new Token(Token.Types.Operator, Operator.CloseCurlyBrace, "}", 0, 15),
				new Token(Token.Types.Operator, Operator.Remainder, "%", 0, 16),
				new Token(Token.Types.Operator, Operator.LessTest, "<", 0, 17),
				new Token(Token.Types.Operator, Operator.MoreTest, ">", 0, 18),
				new Token(Token.Types.Operator, Operator.NotEqualityTest, "!=", 0, 19),
				new Token(Token.Types.Operator, Operator.LogicalOr, "||", 0, 21),
				new Token(Token.Types.Operator, Operator.BitwiseOr, "|", 0, 23),}
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_11()
		{
			Assert.IsTrue(new Test(
				"1234567890", 
				new[] { new Token(Token.Types.Int, 1234567890, "1234567890", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_12()
		{
			Assert.IsTrue(new Test(
				"0xff", new[] { new Token(Token.Types.Int, 255, "0xff", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_13()
		{
			Assert.IsTrue(new Test(
				"001", new[] { new Token(Token.Types.Int, 1, "001", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_14()
		{
			Assert.IsTrue(new Test(
				"99999999999999999999", 
				new[] { new Token(Token.Types.Int, null, "99999999999999999999", 0, 0) { Message = "OverflowException" } }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_15()
		{
			Assert.IsTrue(new Test(
				"0xffffffff", 
				new[] { new Token(Token.Types.Int, -1, "0xffffffff", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_16()
		{
			Assert.IsTrue(new Test(
				"0xfffffffff", 
				new[] { new Token(Token.Types.Int, null, "0xfffffffff", 0, 0) { Message = "OverflowException" } }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_17()
		{
			Assert.IsTrue(new Test(
				"0XFF", new[] { new Token(Token.Types.Int, 255, "0XFF", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_18()
		{
			Assert.IsTrue(new Test(
				"1.0", new[] { new Token(Token.Types.Float, 1.0f, "1.0", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_19()
		{
			Assert.IsTrue(new Test(
				"1.", new[] { new Token(Token.Types.Float, null, "1.", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_20()
		{
			Assert.IsTrue(new Test(
				"1234567890.1234567890",
				new[]{ new Token(Token.Types.Float, 1234567890.1234567890f, "1234567890.1234567890", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_21()
		{
			Assert.IsTrue(new Test(
				"3.4E+39", 
				new[] { new Token(Token.Types.Float, null, "3.4E+39", 0, 0) { Message = "OverflowException" } }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_22()
		{
			Assert.IsTrue(new Test(
				"1.5E-46", new[] { new Token(Token.Types.Float, 0f, "1.5E-46", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_23()
		{
			Assert.IsTrue(new Test(
				"1.0e1", new[] { new Token(Token.Types.Float, 10.0f, "1.0e1", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_24()
		{
			Assert.IsTrue(new Test(
				"1.0e+1", new[] { new Token(Token.Types.Float, 10.0f, "1.0e+1", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_25()
		{
			Assert.IsTrue(new Test(
				"1.0e-1", new[] { new Token(Token.Types.Float, 0.1f, "1.0e-1", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_26()
		{
			Assert.IsTrue(new Test(
				"1.0e1.", new[] { new Token(Token.Types.Float, null, "1.0e1.", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_27()
		{
			Assert.IsTrue(new Test(
				"1.0e+1.", new[] { new Token(Token.Types.Float, null, "1.0e+1.", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_28()
		{
			Assert.IsTrue(new Test(
				"1.0e-1.", new[] { new Token(Token.Types.Float, null, "1.0e-1.", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_29()
		{
			Assert.IsTrue(new Test(
				"'a'", new[] { new Token(Token.Types.Char, 'a', "'a'", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_30()
		{
			Assert.IsTrue(new Test(
				"'ab'", new[] { new Token(Token.Types.Char, null, "'ab", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_31()
		{
			Assert.IsTrue(new Test(
				"\"a\"", new[] { new Token(Token.Types.String, "a", "\"a\"", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_32()
		{
			Assert.IsTrue(new Test(
				"\"`1234567890-=~!@#$%^&*()_+qwertyuiop[]QWERTYUIOP{}asdfghjkl;'ASDFGHJKL:|\\zxcvbnm,./ZXCVBNM<>?\"",
				new[]{ new Token(Token.Types.String,
					"`1234567890-=~!@#$%^&*()_+qwertyuiop[]QWERTYUIOP{}asdfghjkl;'ASDFGHJKL:|\\zxcvbnm,./ZXCVBNM<>?",
					"\"`1234567890-=~!@#$%^&*()_+qwertyuiop[]QWERTYUIOP{}asdfghjkl;'ASDFGHJKL:|\\zxcvbnm,./ZXCVBNM<>?\"", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_33()
		{
			Assert.IsTrue(new Test(
				"a a", new[] {
					new Token(Token.Types.Identifier, "a", "a", 0, 0),
					new Token(Token.Types.Identifier, "a", "a", 0, 2) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_34()
		{
			Assert.IsTrue(new Test(
				"123*123", new[] {
					new Token(Token.Types.Int, 123, "123", 0, 0),
					new Token(Token.Types.Operator, Operator.Multiply, "*", 0, 3),
					new Token(Token.Types.Int, 123, "123", 0, 4) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_35()
		{
			Assert.IsTrue(new Test(
				"123 * 123", new[]{
					new Token(Token.Types.Int, 123, "123", 0, 0),
					new Token(Token.Types.Operator, Operator.Multiply, "*", 0, 4),
					new Token(Token.Types.Int, 123, "123", 0, 6) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_36()
		{
			Assert.IsTrue(new Test(
				"123.0*123.0", new[]{
					new Token(Token.Types.Float, 123.0f, "123.0", 0, 0),
					new Token(Token.Types.Operator, Operator.Multiply, "*", 0, 5),
					new Token(Token.Types.Float, 123.0f, "123.0", 0, 6) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_37()
		{
			Assert.IsTrue(new Test(
				"123.0 * 123.0", new[]{
					new Token(Token.Types.Float, 123.0f, "123.0", 0, 0),
					new Token(Token.Types.Operator, Operator.Multiply, "*", 0, 6),
					new Token(Token.Types.Float, 123.0f, "123.0", 0, 8) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_38()
		{
			Assert.IsTrue(new Test(
				"a.", new[]{
					new Token(Token.Types.Identifier, "a", "a", 0, 0),
					new Token(Token.Types.Operator, Operator.Dot, ".", 0, 1) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_39()
		{
			Assert.IsTrue(new Test(
				"a*b", new[]{
					new Token(Token.Types.Identifier, "a", "a", 0, 0),
					new Token(Token.Types.Operator, Operator.Multiply, "*", 0, 1),
					new Token(Token.Types.Identifier, "b", "b", 0, 2) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_40()
		{
			Assert.IsTrue(new Test(
				"-1.0e-1", new[]{
					new Token(Token.Types.Operator, Operator.Subtract, "-", 0, 0),
					new Token(Token.Types.Float, 0.1f, "1.0e-1", 0, 1)}
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_41()
		{
			Assert.IsTrue(new Test(
				"-1", new[]{
					new Token(Token.Types.Operator, Operator.Subtract, "-", 0, 0),
					new Token(Token.Types.Int, 1, "1", 0, 1)}
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_42()
		{
			Assert.IsTrue(new Test(
				"void", new[] { new Token(Token.Types.Keyword, Keyword.Void, "void", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_43()
		{
			Assert.IsTrue(new Test(
				"int", new[] { new Token(Token.Types.Keyword, Keyword.Int, "int", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_44()
		{
			Assert.IsTrue(new Test(
				"float", new[] { new Token(Token.Types.Keyword, Keyword.Float, "float", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_45()
		{
			Assert.IsTrue(new Test(
				"char", new[] { new Token(Token.Types.Keyword, Keyword.Char, "char", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_46()
		{
			Assert.IsTrue(new Test(
				"string", new[] { new Token(Token.Types.Keyword, Keyword.String, "string", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_47()
		{
			Assert.IsTrue(new Test(
				"class", new[] { new Token(Token.Types.Keyword, Keyword.Class, "class", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_48()
		{
			Assert.IsTrue(new Test(
				"if", new[] { new Token(Token.Types.Keyword, Keyword.If, "if", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_49()
		{
			Assert.IsTrue(new Test(
				"for", new[] { new Token(Token.Types.Keyword, Keyword.For, "for", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_50()
		{
			Assert.IsTrue(new Test(
				"while", new[] { new Token(Token.Types.Keyword, Keyword.While, "while", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_51()
		{
			Assert.IsTrue(new Test(
				" while", new[] { new Token(Token.Types.Keyword, Keyword.While, "while", 0, 1) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_52()
		{
			Assert.IsTrue(new Test(
				"*while+", new[]{
					new Token(Token.Types.Operator, Operator.Multiply, "*", 0, 0),
					new Token(Token.Types.Keyword, Keyword.While, "while", 0, 1),
					new Token(Token.Types.Operator, Operator.Add, "+", 0, 6),}
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_53()
		{
			Assert.IsTrue(new Test(
				"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890", new[]{
					new Token(Token.Types.Identifier, "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890",
					"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890", 0, 0) }
			).Execute().IsDone);
		}

		[TestMethod]
		public void Test_54()
		{
			Assert.IsTrue(new Test(
				"*a+", new[]{
					new Token(Token.Types.Operator, Operator.Multiply, "*", 0, 0),
					new Token(Token.Types.Identifier, "a", "a", 0, 1),
					new Token(Token.Types.Operator, Operator.Add, "+", 0, 2),}
			).Execute().IsDone);
		}

		private struct Test
		{
			public struct Result
			{
				public bool IsDone;
				public List<Token> Tokens;
			}

			public readonly string Input;
			public readonly Token[] Output;

			public Test(string input, Token[] output)
			{
				Input = input; Output = output;
			}

			public Result Execute()
			{
				var tokens = GetTokens();
				bool equals = true;
				equals &= Output.Length == tokens.Count;
				if (equals) {
					for (int i = 0; i < Output.Length; i++) {
						equals &= Token.Equals(Output[i], tokens[i]);
					}
				}
				return new Result { IsDone = equals, Tokens = tokens };
			}

			private List<Token> GetTokens()
			{
				lexer.SetSource(GenerateStreamFromString(Input));
				var tokens = new List<Token>();
				Token token;
				while (null != (token = lexer.Next())) {
					tokens.Add(token);
				}
				return tokens;
			}
		}

		private static MemoryStream GenerateStreamFromString(string value)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
		}
	}
}
