using System;
using System.Collections.Generic;
using Shared;
using Lexer;

namespace Parser
{
	public partial class Parser
	{
		private TypeInfo CurrentType;
		private TypeInfo.FieldInfo CurrentField;
		private TypeInfo.MethodInfo CurrentMethod;

		private Node ParseClass()
		{
			var token = PeekToken();
			if (!IsKeyword(token, Keyword.Public)) {
				throw new WrongTokenFound(token, "public");
			}
			token = NextTokenThrowIfFailed();
			if (!IsKeyword(token, Keyword.Class)) {
				throw new WrongTokenFound(token, "class");
			}
			token = NextTokenThrowIfFailed();
			if (token.Type != Token.Types.Identifier) {
				throw new WrongTokenFound(token, "identifier");
			}
			TypeInfo typeInfo = null;
			if (IsTypeName(token, ref typeInfo)) {
				throw new ParserException(token, "Type name is not unique");
			}
			CurrentType = new TypeInfo((string)token.Value, false, false);
			Types.Add((string)token.Value, CurrentType);
			var typeNode = new TypeDefinition(CurrentType);
			scopes.Push(typeNode);

			token = NextTokenThrowIfFailed();
			if (!IsOperator(token, Operator.OpenCurlyBrace)) {
				throw new WrongTokenFound(token, "{");
			}
			NextTokenThrowIfFailed();
			while (true) {
				var node = ParseMember();
				if (node == null) {
					break;
				}
				typeNode.Children.Add(node);
			}
			token = PeekToken();
			if (!IsOperator(token, Operator.CloseCurlyBrace)) {
				throw new WrongTokenFound(token, "}");
			}
			CurrentType = null;
			scopes.Pop();
			return typeNode;
		}

		private Node ParseMember()
		{
			var token = PeekToken();
			if (IsOperator(token, Operator.CloseCurlyBrace)) return null;
			if (!IsKeyword(token, Keyword.Public)) {
				throw new WrongTokenFound(token, "public");
			}
			token = NextTokenThrowIfFailed();
			bool isStatic = false;
			if (IsKeyword(token, Keyword.Static)) {
				isStatic = true;
				NextTokenThrowIfFailed();
			}
			var type = ParseType();
			token = PeekToken();
			if (token.Type != Token.Types.Identifier) {
				throw new WrongTokenFound(token, "identifier");
			}
			var identifier = (string)token.Value;
			if (!IsIdentifierUnique(identifier)) {
				throw new ParserException(token, string.Format("identifier {0} not unique", identifier));
			}
			token = NextTokenThrowIfFailed();
			if (IsOperator(token, Operator.OpenParenthesis)) {
				NextTokenThrowIfFailed();
				return ParseMethod(type, identifier, isStatic);
			} else if (IsOperator(token, Operator.Assignment)) {
				NextTokenThrowIfFailed();
				return ParseField(type, identifier, isStatic);
			} else if (IsOperator(token, Operator.SemiColon)) {
				return ParseField(type, identifier, isStatic);
			}
			else {
				throw new WrongTokenFound(token, "( or = or ;");
			}
		}

		private Node ParseField(Type type , string identifier, bool isStatic)
		{
			if (type.Info == Type.VoidTypeInfo) {
				throw new ParserException("Cannot create a field of type void");
			}
			var fieldInfo = new TypeInfo.FieldInfo(type, identifier, isStatic);
			CurrentField = fieldInfo;
			var initializer = ParseExpression();
			CurrentField = null;
			if (initializer == null) {
				initializer = new Literal(type, type.DefaultValue());
			}
			if (initializer.Type == null && !type.IsReference()) {
				throw new ParserException(string.Format("Variable {0} is not a reference type", identifier));
			}
			var token = PeekToken();
			if (!IsOperator(token, Operator.SemiColon)) {
				throw new WrongTokenFound(token, ";");
			}
			NextTokenThrowIfFailed();
			CurrentType.Fields.Add(identifier, fieldInfo);
			return new FieldDefinition(fieldInfo, initializer);
		}

		private Node ParseMethod(Type type, string identifier, bool isStatic)
		{
			var token = PeekToken();
			var parameters = new List<TypeInfo.MethodInfo.PramsInfo>();
			var scope = new Scope();
			scopes.Push(scope);
			if (!IsOperator(token, Operator.CloseParenthesis)) {
				while (true) {
					var paramType = ParseType();
					// parse param identifier
					token = PeekToken();
					if (token.Type != Token.Types.Identifier) {
						throw new WrongTokenFound(token, "identifier");
					}
					var paramName = (string)token.Value;
					if (!IsIdentifierUnique(paramName)) {
						throw new ParserException(token, string.Format("identifier {0} not unique", paramName));
					}
					// add param to method
					var pramsInfo = new TypeInfo.MethodInfo.PramsInfo(paramType, paramName);
					scope.Variables.Add(paramName, pramsInfo);
					parameters.Add(pramsInfo);

					token = NextTokenThrowIfFailed();
					if (!IsOperator(token, Operator.Comma)) {
						break;
					}
					token = NextTokenThrowIfFailed();
				}
				if (!IsOperator(token, Operator.CloseParenthesis)) {
					throw new WrongTokenFound(token, ")");
				}
			}
			token = NextTokenThrowIfFailed();
			if (!IsOperator(token, Operator.OpenCurlyBrace)) {
				throw new WrongTokenFound(token, "{");
			}
			NextTokenThrowIfFailed();
			var methodInfo = new TypeInfo.MethodInfo(type, identifier, isStatic, parameters);
			CurrentType.Methods.Add(identifier, methodInfo);
			CurrentMethod = methodInfo;
			var blockStatement = (Block)ParseBlock();
			CurrentMethod = null;
			scopes.Pop();
			return new MethodDefinition(methodInfo, blockStatement.Children);
		}

		private Type ParseType()
		{
			var token = PeekToken();
			TypeInfo typeInfo = null;
			if (!IsTypeName(token, ref typeInfo)) {
				throw new TypeNotFound(token);
			}
			NextTokenThrowIfFailed();
			return new Type(typeInfo, ParseArrayRang());
		}

		private bool IsIdentifierUnique(string identifier)
		{
			foreach (var scope in scopes) {
				if (scope.Variables.ContainsKey(identifier)) {
					return false;
				}
			}
			return true;
		}
	}
}
