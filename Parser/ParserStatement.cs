using System;
using System.Collections.Generic;
using Shared;
using Lexer;

namespace Parser
{
	public partial class Parser
	{
		private Stack<IScope> scopes = new Stack<IScope>();
		private int loopDepth = 0;

		private List<Node> ParseStatements()
		{
			var nodes = new List<Node>();
			var token = PeekToken();
			while (!IsOperator(token, Operator.CloseCurlyBrace)) {
				Node node;
				TypeInfo typeInfo = null;
				if (IsTypeName(token, ref typeInfo)) {
					var nextToken = NextTokenThrowIfFailed();
					if (IsOperator(nextToken, Operator.Dot)) {
						foreseeableFuture.Push(token);
						node = ParseStatementExpression();
					} else {
						node = ParseVariableDeclaration(typeInfo);
					}
				} else if (IsKeyword(token, Keyword.Logic)) {
					node = ParseSwitchesAndLoops();
				} else if (IsOperator(token, Operator.OpenCurlyBrace)) {
					NextTokenThrowIfFailed();
					node = ParseBlock();
				} else if (IsOperator(token, Operator.SemiColon)) {
					NextTokenThrowIfFailed();
					node = new SemiColon();
				} else {
					node = ParseStatementExpression();
				}
				nodes.Add(node ?? throw new ParserException(string.Format("Invalid token {0}", token.RawValue)));
				token = PeekToken();
			}
			return nodes;
		}

		private bool IsKeyword(Token token, Keyword keyword) =>
			token.Type == Token.Types.Keyword && ((Keyword)token.Value).HasFlag(keyword);

		private bool IsBoolExpression(Expression expression) => Type.Equals(expression.Type, Type.BoolTypeInfo);

		private Node ParseSwitchesAndLoops()
		{
			var token = PeekToken();
			var keyword = (Keyword)token.Value;
			token = NextTokenThrowIfFailed();
			if (!IsOperator(token, Operator.OpenParenthesis)) {
				throw new WrongTokenFound(token, "(");
			}
			token = NextTokenThrowIfFailed();
			Node node = null;
			++loopDepth;
			if (keyword.HasFlag(Keyword.If)) {
				node = ParseIf(token);
			} else if (keyword.HasFlag(Keyword.For)) {
				node = ParseFor(token);
			} else if (keyword.HasFlag(Keyword.While)) {
				node = ParseWhile(token);
			}
			--loopDepth;
			return node ?? throw new WrongTokenFound(token, "something else");
		}

		private Node ParseReturn(Token returnToken)
		{
			var expression = ParseExpression();
			var token = PeekToken();
			if (!IsOperator(token, Operator.SemiColon)) {
				throw new WrongTokenFound(token, ";");
			}
			return new Return(expression, CurrentMethod, returnToken);
		}

		private Node ParseIf(Token token)
		{
			var boolExpression = ParseExpression();
			if (!IsBoolExpression(boolExpression)) {
				throw new InvalidExpressionType(token, "bool");
			}
			token = PeekToken();
			if (!IsOperator(token, Operator.CloseParenthesis)) {
				throw new WrongTokenFound(token, ")");
			}
			token = NextTokenThrowIfFailed();
			Node blockStatement;
			if (IsKeyword(token, Keyword.If)) {
				blockStatement = ParseIf(NextTokenThrowIfFailed());
			} else if (IsOperator(token, Operator.OpenCurlyBrace)) {
				NextTokenThrowIfFailed();
				blockStatement = ParseBlock();
			} else {
				throw new WrongTokenFound(token, "{");
			}
			Node elseStatement = null;
			token = PeekToken();
			if (IsKeyword(token, Keyword.Else)) {
				token = NextTokenThrowIfFailed();
				if (IsKeyword(token, Keyword.If)) {
					token = NextTokenThrowIfFailed();
					if (!IsOperator(token, Operator.OpenParenthesis)) {
						throw new WrongTokenFound(token, "(");
					}
					elseStatement = ParseIf(NextTokenThrowIfFailed());
				} else if (IsOperator(token, Operator.OpenCurlyBrace)) {
					NextTokenThrowIfFailed();
					elseStatement = ParseBlock();
				} else {
					throw new WrongTokenFound(token, "if' or '{");
				}
			}
			return new If(boolExpression, blockStatement, elseStatement);
		}

		private Node ParseFor(Token token)
		{
			Node initializer = null;
			TypeInfo typeInfo = null;
			if (IsTypeName(token, ref typeInfo)) {
				NextTokenThrowIfFailed();
				initializer = ParseVariableDeclaration(typeInfo);
				token = PeekToken();
			}
			if (initializer == null) {
				if (!IsOperator(token, Operator.SemiColon)) {
					throw new WrongTokenFound(token, ";");
				}
				token = NextTokenThrowIfFailed();
			}
			var condition = ParseExpression();
			if (condition != null && !IsBoolExpression(condition)) {
				throw new InvalidExpressionType(token, "bool");
			}
			token = PeekToken();
			if (!IsOperator(token, Operator.SemiColon)) {
				throw new WrongTokenFound(token, ";");
			}
			token = NextTokenThrowIfFailed();
			var iterator = ParseExpression();
			if (iterator != null && !(iterator is IStatement stmt && stmt.IsStatement)) {
				throw new InvalidExpressionType(token, "expression which statement");
			}
			token = PeekToken();
			if (!IsOperator(token, Operator.CloseParenthesis)) {
				throw new WrongTokenFound(token, ")");
			}
			token = NextTokenThrowIfFailed();
			if (!IsOperator(token, Operator.OpenCurlyBrace)) {
				throw new WrongTokenFound(token, "{");
			}
			NextTokenThrowIfFailed();
			var blockStatement = (Block)ParseBlock();
			return new For(initializer, condition, iterator, blockStatement);
		}

		private Node ParseWhile(Token token)
		{
			var condition = ParseExpression();
			if (!IsBoolExpression(condition)) {
				throw new InvalidExpressionType(token, "bool");
			}
			token = PeekToken();
			if (!IsOperator(token, Operator.CloseParenthesis)) {
				throw new WrongTokenFound(token, ")");
			}
			token = NextTokenThrowIfFailed();
			if (!IsOperator(token, Operator.OpenCurlyBrace)) {
				throw new WrongTokenFound(token, "{");
			}
			NextTokenThrowIfFailed();
			var blockStatement = (Block)ParseBlock();
			return new While(condition, blockStatement);
		}

		private Node ParseBlock()
		{
			var block = new Block();
			scopes.Push(block);
			block.Children = ParseStatements();
			scopes.Pop();
			var token = PeekToken();
			if (!IsOperator(token, Operator.CloseCurlyBrace)) {
				throw new WrongTokenFound(token, "}");
			}
			NextTokenThrowIfFailed();
			return block;
		}

		private Node ParseStatementExpression()
		{
			var token = PeekToken();
			if (IsKeyword(token, Keyword.Return)) {
				NextTokenThrowIfFailed();
				return ParseReturn(token);
			} else if (IsKeyword(token, Keyword.Break)) {
				if (loopDepth == 0) {
					throw new ParserException(token, "Break out of loops or ifs");
				}
				token = NextTokenThrowIfFailed();
				if (!IsOperator(token, Operator.SemiColon)) {
					throw new WrongTokenFound(token, ";");
				}
				return new Break();
			} else {
				var expression = ParseExpression();
				if (!(expression is IStatement stmt && stmt.IsStatement)) {
					throw new ParserException(token, "Expression is not a statement");
				}
				return expression;
			}
			throw new InvalidOperationException();
		}

		private Node ParseVariableDeclaration(TypeInfo typeInfo)
		{
			if (typeInfo == Type.VoidTypeInfo) {
				throw new ParserException("Cannot create a variable of type void");
			}
			uint rang = ParseArrayRang();
			var token = PeekToken();
			if (token.Type != Token.Types.Identifier) {
				throw new WrongTokenFound(token, "identifier");
			}
			var identifier = (string)token.Value;
			if (!IsIdentifierUnique(identifier)) {
				throw new ParserException(token, string.Format("identifier {0} not unique", identifier));
			}
			token = NextTokenThrowIfFailed();
			Expression value = null;
			if (IsOperator(token, Operator.Assignment)) {
				NextTokenThrowIfFailed();
				value = ParseExpression();
			}
			if (value != null && value.Type == null && !typeInfo.IsReference && rang == 0) {
				throw new ParserException(string.Format("Variable {0} is not a reference type", identifier));
			}
			var type = new Type(typeInfo, rang);
			if (value == null) {
				value = new Literal(type, type.DefaultValue());
			}
			token = PeekToken();
			if (!IsOperator(token, Operator.SemiColon)) {
				throw new WrongTokenFound(token, ";");
			}
			NextTokenThrowIfFailed();
			var node = new VariableDefinition(type, identifier, value);
			scopes.Peek().Variables.Add(identifier, node);
			return node;
		}
	}
}
