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
			return token == null ? null : !token.IsError ? token :
				throw new LexerException(string.Format("Lexical analysis failed token {0}", token.ToString()));
		}

		/// <summary>
		/// Our whole program consists of several classes.
		/// </summary>
		public List<Node> ParseProgram()
		{
			var nodes = new List<Node>();
			while (NextToken()) {
				nodes.Add(ParseClass());
			}
			return nodes;
		}
	}
}
