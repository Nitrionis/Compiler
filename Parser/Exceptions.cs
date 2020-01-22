using System;
using Lexer;

namespace Parser
{
	public class ParserException : InvalidOperationException
	{
		public ParserException() { }
		public ParserException(string message) : base(message) { }
		public ParserException(string message, Exception inner) : base(message, inner) { }
		public ParserException(Token token, string message) : base(GetPrefix(token) + message) { }

		private static string GetPrefix(Token token) => 
			string.Format("(r:{0}, c:{1}) ", token.RowIndex, token.ColIndex);
	}

	public class WrongTokenFound : ParserException
	{
		public WrongTokenFound(Token token, string rawValue) : base(CreateMessage(token, rawValue)) { }

		private static string CreateMessage(Token token, string rawValue) =>
			string.Format("(r:{0}, c:{1}) Syntax error: '{2}' expected, but '{3}' found",
				token.RowIndex, token.ColIndex, rawValue, token.RawValue);
	}

	public class TypeNotFound : ParserException
	{
		public TypeNotFound(Token token) : base(CreateMessage(token)) { }

		private static string CreateMessage(Token token) =>
			string.Format("(r:{0}, c:{1}) Syntax error: type '{2}' not found",
				token.RowIndex, token.ColIndex, token.RawValue);
	}

	public class InvalidExpressionsCombination : ParserException
	{
		public InvalidExpressionsCombination(string op1, string op2) : base(CreateMessage(op1, op2)) { }

		private static string CreateMessage(string op1, string op2) =>
			string.Format("Syntax error: invalid expressions combination {0} and {1}", op1, op2);
	}

	public class InvalidExpressionType : ParserException
	{
		public InvalidExpressionType(Token token, string msg) : base(CreateMessage(token, msg)) { }

		private static string CreateMessage(Token token, string msg) =>
			string.Format("(r:{0}, c:{1}) Syntax error: Invalid expression type. Expected {2}.",
				token.RowIndex, token.ColIndex, msg);
	}
}
