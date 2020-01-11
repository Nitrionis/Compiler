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
			var token = PeekToken();
			var left = ParseConditionalOrExpression();
			if (left == null) return null;
			token = PeekToken();
			if (token == null ||
				!((token.Type == Token.Types.Operator) &&
				 ((Operator)token.Value == Operator.Assignment))) 
			{
				return left;
			}
			if (!(left is IValue value && value.IsVariable)) {
				throw new ParserException(token, "Wrong assignment target");
			}
			NextTokenThrowIfFailed();
			var right = ParseAssignment();
			left = new BinaryOperation(token, left, right);
			return left;
		}

		private static bool IsOperator(Token token, Operator @operator) =>
			token.Type == Token.Types.Operator && ((Operator)token.Value).HasFlag(@operator);

		private Expression ParseBinaryLeftAssociativeOperation(
			Operator @operator, ParseExpressionHandle ParseMorePriorityExpression)
		{
			var left = ParseMorePriorityExpression();
			while (left != null) {
				var token = PeekToken();
				if (!IsOperator(token, @operator)) {
					break;
				}
				NextTokenThrowIfFailed();
				var right = ParseMorePriorityExpression();
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
			if (IsOperator(token, Operator.UnaryOperator)) {
				NextTokenThrowIfFailed();
				return new UnaryOperation(token, ParseUnaryExpression());
			}
			if (IsOperator(token, Operator.OpenParenthesis)) {
				var openParenthesis = token;
				var typeName = NextTokenThrowIfFailed();
				TypeInfo typeInfo = null;
				if (IsTypeName(typeName, ref typeInfo)) {
					var closeParenthesis = NextTokenThrowIfFailed();
					if (IsOperator(closeParenthesis, Operator.CloseParenthesis)) {
						NextTokenThrowIfFailed();
						return new TypeCast(new Type(typeInfo), ParseUnaryExpression());
					}
					foreseeableFuture.Push(typeName);
				}
				foreseeableFuture.Push(openParenthesis);
			}
			return ParseArrayCreationExpression();
		}

		private bool IsTypeName(Token token, ref TypeInfo typeInfo) => (
				token.Type == Token.Types.Identifier || 
				token.Type == Token.Types.Keyword && ((Keyword)token.Value).HasFlag(Keyword.Type)
			) && Types.TryGetValue(token.RawValue, out typeInfo);

		private Expression ParseArrayCreationExpression()
		{
			var token = PeekToken();
			if (token.Type == Token.Types.Keyword && (Keyword)token.Value == Keyword.New) {
				var typeNameToken = NextTokenThrowIfFailed();
				TypeInfo typeInfo = null;
				if (IsTypeName(typeNameToken, ref typeInfo)) {
					var openSquareBracket = NextTokenThrowIfFailed();
					if (IsOperator(openSquareBracket, Operator.OpenSquareBracket)) {
						token = NextTokenThrowIfFailed();
						var sizeExpression = ParseExpression();
						if (sizeExpression == null) {
							throw new ParserException(string.Format(
								"(r:{0}, c:{1}) Syntax error: array size not set",
								token.RowIndex, token.ColIndex));
						}
						token = PeekToken();
						if (!IsOperator(token, Operator.CloseSquareBracket)) {
							throw new WrongTokenFound(token, "]");
						}
						NextTokenThrowIfFailed();
						uint arrayRang = 1 + ParseArrayRang();
						if (!IsOperator(PeekToken(), Operator.OpenCurlyBrace)) {
							throw new WrongTokenFound(token, "{");
						}
						return new ArrayCreation(
							type: new Type(typeInfo, arrayRang),
							size: sizeExpression,
							data: ParseArrayData());
					}
					foreseeableFuture.Push(typeNameToken);
				}
				foreseeableFuture.Push(token);
			}
			return ParsePrimaryExpression();
		}

		private uint ParseArrayRang()
		{
			for (uint rang = 0; true; rang++) {
				Token token = PeekToken();
				if (!IsOperator(token, Operator.OpenSquareBracket)) {
					return rang;
				}
				token = NextTokenThrowIfFailed();
				if (!IsOperator(token, Operator.CloseSquareBracket)) {
					throw new WrongTokenFound(token, "]");
				}
				NextTokenThrowIfFailed();
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
				case Token.Types.String: NextTokenThrowIfFailed(); return new Literal(token);
				case Token.Types.Keyword:
					switch ((Keyword)token.Value) {
						case Keyword.Int:    NextTokenThrowIfFailed(); return new TypeReference(Type.IntTypeInfo);
						case Keyword.Float:  NextTokenThrowIfFailed(); return new TypeReference(Type.FloatTypeInfo);
						case Keyword.Char:   NextTokenThrowIfFailed(); return new TypeReference(Type.CharTypeInfo);
						case Keyword.String: NextTokenThrowIfFailed(); return new TypeReference(Type.StringTypeInfo);
						case Keyword.Bool:	 NextTokenThrowIfFailed(); return new TypeReference(Type.BoolTypeInfo);
						case Keyword.True:   NextTokenThrowIfFailed(); return new Literal(new Type(Type.BoolTypeInfo), true);
						case Keyword.False:  NextTokenThrowIfFailed(); return new Literal(new Type(Type.BoolTypeInfo), false);
						case Keyword.Null:   NextTokenThrowIfFailed(); return new Literal(null, null);
						case Keyword.New:	 return ParseObjectCreationExpression(previousExpression);
						case Keyword.Return: return null;
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
			NextTokenThrowIfFailed();
			return new Parenthesis(expression);
		}

		private Expression ParseInvocation(Expression previousExpression)
		{
			Token token;
			var parameters = new List<Expression>();
			while (true) {
				NextTokenThrowIfFailed();
				var expression = ParseExpression();
				if (expression != null) {
					parameters.Add(expression);
				}
				token = PeekToken();
				if (!IsOperator(token, Operator.Comma)) {
					break;
				}
			}
			if (!IsOperator(token, Operator.CloseParenthesis)) {
				throw new WrongTokenFound(token, ")");
			}
			NextTokenThrowIfFailed();
			return new Invocation(parameters, previousExpression, CurrentMethod);
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
			if (token.Type == Token.Types.Keyword && ((Keyword)token.Value).HasFlag(Keyword.Type)) {
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
			NextTokenThrowIfFailed();
			var identifier = (string)token.Value;
			TypeInfo typeInfo;
			if (Types.TryGetValue(identifier, out typeInfo)) {
				return new TypeReference(typeInfo);
			}
			foreach (var scope in scopes) {
				IVariable variableInfo;
				if (scope.Variables.TryGetValue(identifier, out variableInfo)) {
					if (scope is TypeDefinition && !((TypeInfo.FieldInfo)variableInfo).IsStatic) {
						if (CurrentField != null && CurrentField.IsStatic) {
							throw new ParserException(token, string.Format(
								"Attempting to access a non-static field '{0}' from static field '{1}' initializer",
								variableInfo.Name, CurrentField.Name));
						}
						if (CurrentMethod != null && CurrentMethod.IsStatic) {
							throw new ParserException(token, string.Format(
								"Attempting to access a non-static field '{0}' from a static method '{1}'",
								variableInfo.Name, CurrentMethod.Name));
						}
					}
					return new VariableReference(variableInfo);
				}
			}
			TypeInfo.MethodInfo method;
			if (CurrentType.Methods.TryGetValue(identifier, out method)) {
				return new MethodReference(method);
			}
			throw new ParserException(string.Format("Unknown identifier: {0}", identifier ?? "Null"));
		}
	}
}
