using System.Collections.Generic;
using System.IO;
using Lexer;

namespace Parser
{
	using Lexer = Lexer.Lexer;
	using Token = Lexer.Token;

	public partial class Parser
    {
		private readonly Lexer lexer;
		private readonly Stack<Token> foreseeableFuture;
		private readonly Dictionary<string, TypeInfo> Types;

		protected Parser(Lexer lexer)
		{
			this.lexer = lexer;
			foreseeableFuture = new Stack<Token>();
			Types = new Dictionary<string, TypeInfo>(Type.Predefined);

			// todo
			NextTokenThrowIfFailed();
			var exception = ParseExpression();
			if (exception != null) {
				System.Console.WriteLine(exception.ToString());
			}
		}

		public Parser(string path) : this(new Lexer(path)) {  }
		public Parser(Stream stream) : this(new Lexer(stream)) { }

		/// <summary>
		/// Gets a current token. Can be null.
		/// </summary>
		private Token PeekToken() => foreseeableFuture.Count > 0 ? foreseeableFuture.Peek() : lexer.Peek();

		private bool NextToken() => TryGetToken() != null;

		/// <summary>
		/// Used when the next token must exist.
		/// </summary>
		private Token NextTokenThrowIfFailed() => TryGetToken() ?? throw new ParserException("Unexpected end.");

		private Token TryGetToken()
		{
			Token token;
			if (foreseeableFuture.Count == 0) {
				token = lexer.Next();
			} else {
				foreseeableFuture.Pop();
				token = foreseeableFuture.Count == 0 ? lexer.Peek() : foreseeableFuture.Peek();
			}
			return token == null ? null : token.IsError ?
				throw new LexerException("Lexical analysis failed.") : token;
		}

		public Token PeekAndNext()
		{
			var t = PeekToken();
			NextToken();
			return t;
		}

		public Token NextAndPeek()
		{
			NextToken();
			return PeekToken();
		}
	}
}
