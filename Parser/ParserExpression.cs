using System;
using System.Collections.Generic;
using Shared;
using Lexer;

namespace Parser
{
	public partial class Parser
	{
		private delegate Expression ParseExpressionHandle();

		private Expression ParseExpression() => ParseAssignment();

		private Expression ParseAssignment() // _ = _
		{
			var left = ParseConditionalOrExpression();
			if (left == null) return null;
			var token = PeekToken();
			if (token == null ||
				!((token.Type == Token.Types.Operator) &&
				 ((Operator)token.Value == Operator.Assignment))) {
				return left;
			}
			NextTokenThrowIfFailed();
			var right = ParseAssignment();
			left = new BinaryOperation(token, left, right);
			return left;
		}

		private static bool IsOperator(Token token, Operator @operator) =>
			token.Type == Token.Types.Operator && ((Operator)token.Value & @operator) == @operator;

		private Expression ParseBinaryLeftAssociativeOperation(
			Operator @operator, ParseExpressionHandle ParseDeeperExpression)
		{
			var left = ParseDeeperExpression();
			while (left != null) {
				var token = PeekToken();
				if (token == null || !IsOperator(token, @operator)) {
					break;
				}
				NextTokenThrowIfFailed();
				var right = ParseDeeperExpression();
				left = new BinaryOperation(token, left, right);
			}
			return left;
		}

		private Expression ParseConditionalOrExpression() =>
			ParseBinaryLeftAssociativeOperation(Operator.LogicalOr, () =>
				ParseBinaryLeftAssociativeOperation(Operator.LogicalAnd, () =>
					ParseBinaryLeftAssociativeOperation(Operator.BitwiseOr, () =>
						ParseBinaryLeftAssociativeOperation(Operator.BitwiseAnd, () =>
							ParseBinaryLeftAssociativeOperation(Operator.EqualityOperator, () =>
								ParseBinaryLeftAssociativeOperation(Operator.RelationalOperator, () =>
									ParseBinaryLeftAssociativeOperation(Operator.AdditiveOperator, () =>
										ParseBinaryLeftAssociativeOperation(Operator.MultiplicativeOperator, () =>
											ParseUnaryExpression()))))))));

		private Expression ParseUnaryExpression()  // +_ -_ !_ ~_ (Type)_
		{
			var token = PeekToken();
			if (token == null) return null;
			if (IsOperator(token, Operator.UnaryOperator)) {
				NextTokenThrowIfFailed();
				return new UnaryOperation(token, ParseUnaryExpression());
			}
			if (IsOperator(token, Operator.OpenParenthesis)) {
				var expression = ParsePrimaryExpression();
				if (expression is Parenthesis parenthesis) {
					var typeReference = parenthesis.Children[0] as TypeReference;
					if (typeReference != null) {
						NextTokenThrowIfFailed();
						return new TypeCast(typeReference.TypeInfo, ParseUnaryExpression());
					}
				}
				return expression;
			}
			return ParseArrayCreationExpression();
		}

		private bool IsTypeName(Token token) =>
			token.Type == Token.Types.Identifier && Types.ContainsKey((string)token.Value) ||
			token.Type == Token.Types.Keyword && ((Keyword)token.Value & Keyword.Type) != 0;

		private Expression ParseArrayCreationExpression()
		{
			var token = PeekToken();
			if (token != null && token.Type == Token.Types.Keyword && (Keyword)token.Value == Keyword.New) {
				var typeName = NextTokenThrowIfFailed();
				if (IsTypeName(typeName)) {
					var openSquareBracket = NextTokenThrowIfFailed();
					if (IsOperator(openSquareBracket, Operator.OpenSquareBracket)) {
						NextToken();
						var size = ParseExpression();
						token = PeekToken();
						if (size == null) {
							throw new ParserException(string.Format(
								"(r:{0}, c:{1}) Syntax error: array size not set",
								token.RowIndex, token.ColIndex));
						}
						if (!IsOperator(token, Operator.CloseSquareBracket)) {
							throw new WrongTokenFound(token, "]");
						}
						uint rang = ParseArrayRang();
						var data = ParseArrayData();
						var type = typeName.Type == Token.Types.Keyword ?
							Type.Of(typeName) : new Type(Types[(string)typeName.Value], rang);
						return new ArrayCreation(type, size, data);
					}
					foreseeableFuture.Push(typeName);
				}
				foreseeableFuture.Push(token);
			}
			return ParsePrimaryExpression();
		}

		private uint ParseArrayRang()
		{
			for (uint rang = 1; true; rang++) {
				NextTokenThrowIfFailed();
				Token token = PeekToken();
				if (IsOperator(token, Operator.OpenCurlyBrace)) {
					return rang;
				}
				if (!IsOperator(token, Operator.OpenSquareBracket)) {
					throw new WrongTokenFound(token, "[");
				}
				NextTokenThrowIfFailed();
				token = PeekToken();
				if (!IsOperator(token, Operator.CloseSquareBracket)) {
					throw new WrongTokenFound(token, "]");
				}
			}
		}

		private List<Expression> ParseArrayData()
		{
			var parameters = new List<Expression>();
			var token = PeekToken();
			if (IsOperator(token, Operator.OpenCurlyBrace)) {
				while (true) {
					NextTokenThrowIfFailed();
					parameters.Add(ParseExpression());
					token = PeekToken();
					if (!IsOperator(token, Operator.Comma)) {
						break;
					}
				}
				if (!IsOperator(token, Operator.CloseCurlyBrace)) {
					throw new WrongTokenFound(token, "}");
				}
				NextTokenThrowIfFailed();
			}
			return parameters;
		}

		private Expression ParsePrimaryExpression()
		{
			var left = PrimaryRouter(null);
			while (left != null) {
				var right = PrimaryRouter(left);
				if (right == null) {
					break;
				}
				left = right;
			}
			return left;
		}

		private Expression PrimaryRouter(Expression previousExpression)
		{
			var token = PeekToken();
			if (token == null) return null;
			switch (token.Type) {
				case Token.Types.Int:
				case Token.Types.Float:
				case Token.Types.Char:
				case Token.Types.String: NextToken(); return new Literal(token);
				case Token.Types.Keyword:
					switch ((Keyword)token.Value) {
						case Keyword.Int:	 NextToken(); return new TypeReference(Type.IntTypeInfo);
						case Keyword.Float:  NextToken(); return new TypeReference(Type.FloatTypeInfo);
						case Keyword.Char:	 NextToken(); return new TypeReference(Type.CharTypeInfo);
						case Keyword.String: NextToken(); return new TypeReference(Type.StringTypeInfo);
						case Keyword.True:	 NextToken(); return new Literal(new Type(Type.BoolTypeInfo), true);
						case Keyword.False:  NextToken(); return new Literal(new Type(Type.BoolTypeInfo), false);
						case Keyword.Null:	 NextToken(); return new Literal(null, null);
						case Keyword.New:	 return ParseObjectCreationExpression(previousExpression);
						default:             throw new ParserException(string.Format(
												"(r:{0}, c:{1}) Syntax error: bad keyword '{2}'",
												token.RowIndex, token.ColIndex, token.RawValue));
					}
				case Token.Types.Identifier: return ParseReference(previousExpression);
				case Token.Types.Operator:
					switch ((Operator)PeekToken().Value) {
						case Operator.OpenParenthesis:   return ParseParenthesis(previousExpression);
						case Operator.OpenSquareBracket: return ParseArrayAccess(previousExpression);
						case Operator.Dot:               return ParseMemberAccess(previousExpression);
						default:                         return null;
					}
				default: throw new InvalidOperationException();
			}
		}

		private Expression ParseParenthesis(Expression previousExpression)
		{
			if (previousExpression != null) {
				return ParseInvocation(previousExpression);
			}
			NextTokenThrowIfFailed();
			var token = PeekToken();
			if (token == null) return null;
			if (IsOperator(token, Operator.CloseParenthesis)) {
				throw new ParserException(string.Format(
					"(r:{0}, c:{1}) syntax error: empty parenthesis expression",
					token.RowIndex, token.ColIndex));
			}
			var expression = ParseExpression();
			token = PeekToken();
			if (!IsOperator(token, Operator.CloseParenthesis)) {
				throw new WrongTokenFound(token, ")");
			}
			return new Parenthesis(expression);
		}

		private Expression ParseInvocation(Expression previousExpression)
		{
			Token token;
			var parameters = new List<Expression>();
			while (true) {
				NextTokenThrowIfFailed();
				parameters.Add(ParseExpression());
				token = PeekToken();
				if (!IsOperator(token, Operator.Comma)) {
					break;
				}
			}
			if (!IsOperator(token, Operator.CloseParenthesis)) {
				throw new WrongTokenFound(token, ")");
			}
			NextTokenThrowIfFailed();
			return new Invocation(parameters, previousExpression);
		}

		private Expression ParseArrayAccess(Expression previousExpression)
		{
			NextTokenThrowIfFailed();
			var expression = ParseExpression();
			var token = PeekToken();
			if (!IsOperator(token, Operator.CloseSquareBracket)) {
				throw new WrongTokenFound(token, "]");
			}
			NextTokenThrowIfFailed();
			return new ArrayAccess(expression, previousExpression);
		}

		private Expression ParseMemberAccess(Expression previousExpression)
		{
			var token = NextTokenThrowIfFailed();
			if (token.Type != Token.Types.Identifier) {
				throw new WrongTokenFound(token, "identifier");
			}
			NextTokenThrowIfFailed();
			return new MemberAccess((string)token.Value, previousExpression);
		}

		private Expression ParseObjectCreationExpression(Expression previousExpression)
		{
			var token = NextTokenThrowIfFailed();
			TypeInfo type;
			if (token.Type == Token.Types.Keyword && ((Keyword)token.Value & Keyword.Type) != 0) {
				type = Type.Of(token).Info;
			} else if (token.Type != Token.Types.Identifier) {
				throw new WrongTokenFound(token, "identifier");
			} else if (!Types.ContainsKey((string)token.Value)) {
				throw new TypeNotFound(token);
			} else {
				type = Types[(string)token.Value];
			}
			var parenthesis = NextTokenThrowIfFailed();
			if (!IsOperator(parenthesis, Operator.OpenParenthesis)) {
				throw new WrongTokenFound(token, "(");
			}
			return new ObjectCreation(type, (Invocation)ParseInvocation(Invocation.Constructor));
		}

		private Expression ParseReference(Expression previousExpression)
		{
			var token = PeekToken();
			NextToken();
			TypeInfo typeInfo;
			return Types.TryGetValue((string)token.Value, out typeInfo) ?
				(Expression)new TypeReference(typeInfo) :
				(Expression)new VariableOrMemberReference((string)token.Value);
		}
	}
}
