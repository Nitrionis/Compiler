using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Shared;

namespace Lexer
{
	public class LexerException : InvalidOperationException
	{
		public LexerException() { }
		public LexerException(string message) : base(message) { }
		public LexerException(string message, Exception inner) : base(message, inner) { }
	}

	public partial class Lexer
    {
		private static readonly Dictionary<string, Keyword> keywords;

		static Lexer()
		{
			keywords = new Dictionary<string, Keyword>() {
				["void"]	= Keyword.Void,
				["int"]		= Keyword.Int,
				["float"]	= Keyword.Float,
				["char"]	= Keyword.Char,
				["string"]	= Keyword.String,
				["bool"]	= Keyword.Bool,
				["true"]	= Keyword.True,
				["false"]	= Keyword.False,
				["if"]		= Keyword.If,
				["for"]		= Keyword.For,
				["while"]	= Keyword.While,
				["class"]	= Keyword.Class,
				["return"]	= Keyword.Return,
				["break"]	= Keyword.Break,
				["public"]	= Keyword.Public,
				["new"]		= Keyword.New,
				["null"]	= Keyword.Null
			};
		}

		private enum State : uint
		{
			Start,
			Division,
			Comment,
			ConstString,
			Ampersand,
			Pipe,
			Minus,
			Equals,
			NotEquals,
			Word,
			Int,           // [0-9]+
			Int0X,         // (0[xX])[0-9]*
			Float,         // ([0-9]+.)[0-9]*
			FloatExp,      // ([0-9]+.[0-9]+)[eE][0-9]*
			FloatExpSign,  // ([0-9]+.[0-9]+)[eE][-+][0-9]*
			Char
		}

		private readonly Input stream;
		private readonly Action[][] actions;
		private readonly int statesCount;
		private readonly int alphabetSize = 128;

		private State activeState = State.Start;
		private Token token;
		private bool tokenCompleted;
		private bool isError;

		public static bool IsKeyword(string word) => keywords.ContainsKey(word);
		public static Keyword KeywordToEnum(string word) => keywords[word];

		public static bool IsDigit(int symbol) => (symbol >= 0x30 && symbol <= 0x39);
		public static bool IsLatin(int symbol) => (symbol >= 0x41 && symbol <= 0x5a) || (symbol >= 0x61 && symbol <= 0x7a);

		public Lexer()
		{
			statesCount = Enum.GetValues(typeof(State)).Length;
			actions = new Action[statesCount][];
			for (int i = 0; i < statesCount; i++) {
				actions[i] = new Action[alphabetSize];
				SetActionsRange(actions[i], ActionSkip);
			}

			BuildStartLevel();
			BuildOtherOperatorsLevels();
			BuildStringLevels();
			BuildIntLevels();
			BuildFloatLevels();
			BuildOtherLevels();
		}

		public Lexer(string path) : this() => stream = new Input(path);
		public Lexer(Stream stream) : this() => this.stream = new Input(stream);

		public void SetSource(string path)
		{
			stream.SetSource(path);
			isError = false;
		}

		public void SetSource(Stream stream)
		{
			this.stream.SetSource(stream);
			isError = false;
		}

		public Token Peek() => token;

		public Token Next()
		{
			if (isError) {
				return null;
			}
			token = new Token();
			tokenCompleted = false;
			while (!tokenCompleted && stream.Next() > -1) {
				if (stream.Symbol > alphabetSize) {
					throw new LexerException("Lexer fatal: unsupported character '" + stream.Symbol + "'");
				}
				actions[(int)activeState][stream.Symbol]();
			}
			UpdateTokenValue();
			ActionTokenCompleted();
			token = token.Type != Token.Types.Undefined ? token : null;
			return token;
		}

		private void UpdateTokenValue()
		{
			if (isError) return;
			try {
				switch (token.Type) {
					case Token.Types.Char: token.Value = token.RawValue[1]; break;
					case Token.Types.String: token.Value = token.RawValue.Substring(1, token.RawValue.Length - 2); break;
					case Token.Types.Float:
						if (token.RawValue.Length > 2) {
							token.Value = float.Parse(token.RawValue, CultureInfo.InvariantCulture);
							if (float.IsInfinity((float)token.Value)) {
								token.Value = null;
								token.Message = "OverflowException";
							}
						}
						break;
					case Token.Types.Int:
						var raw = token.RawValue;
						if (raw.Length > 1 && (raw[1] == 'x' || raw[1] == 'X')) {
							token.Value = int.Parse(token.RawValue.Substring(2), NumberStyles.HexNumber);
						} else {
							token.Value = int.Parse(token.RawValue);
						}
						break;
					case Token.Types.Keyword: token.Value = KeywordToEnum(token.RawValue); break;
					case Token.Types.Identifier:
						token.Type = IsKeyword(token.RawValue) ? Token.Types.Keyword : Token.Types.Identifier;
						if (token.Type == Token.Types.Keyword) {
							token.Value = KeywordToEnum(token.RawValue);
						} else {
							token.Value = token.RawValue;
						}
						break;
					case Token.Types.Operator:
						if (token.Value == null) {
							switch (activeState) {
								case State.Ampersand: token.Value = Operator.BitwiseAnd; break;
								case State.Pipe: token.Value = Operator.BitwiseOr; break;
								case State.NotEquals: token.Value = Operator.LogicalNot; break;
								case State.Division: token.Value = Operator.Divide; break;
								case State.Equals: token.Value = Operator.EqualityTest; break;
								default: throw new InvalidOperationException();
							}
						}
						break;
				}
			} catch (OverflowException) {
				token.Message = "OverflowException";
			} catch (FormatException) {
				token.Message = "FormatException";
			}
		}
	}
}
