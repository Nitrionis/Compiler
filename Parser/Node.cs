using System;
using System.Collections.Generic;
using Shared;

namespace Parser
{
	using Token = Lexer.Token;

	public abstract class Node
	{
		public Node Parent;
		public List<Node> Children;

		public Node() => Children = new List<Node>();

		public sealed override string ToString() => ToString("", true);

		public abstract string ToString(string indent, bool last);

		protected struct LogDecoration
		{
			public string Indent;
			public string Prefix;
		}

		protected LogDecoration GetLogDecoration(string indent, bool last)
		{
			return new LogDecoration {
				Indent = indent + (last ? "  " : "| "),
				Prefix = indent + (last ? "└─" : "├─")
			};
		}
	}

	public interface IScope
	{
		Dictionary<string, VariableInfo> Variables { get; set; }
	}

	public class GlobalScope : Node, IScope
	{
		public static GlobalScope Instance { get; private set; }

		public Dictionary<string, VariableInfo> Variables { get; set; }

		public GlobalScope()
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = this;
		}

		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public class TypeDefinition : Node
	{
		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public class FieldDefinition : Node
	{
		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public class MethodDefinition : Node
	{
		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public abstract class Expression : Node
	{
		public Expression() { }
	}

	public class ObjectCreation : Expression
	{
		public readonly Type Type;

		public ObjectCreation(TypeInfo typeInfo, Invocation invocation)
		{
			Type = new Type(typeInfo);
			Children = new List<Node>(invocation.Parameters);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format(" new {0}(... ObjectCreation\n", Type.Info.Name);
			for (int i = 0; i < Children.Count; i++) {
				if (Children[i] != null) {
					res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
				}
			}
			return res;
		}
	}

	public interface IVariable { }

	///<summary>
	/// VariableDefinition and MethodDefinition can be null!
	/// if VariableOrFieldReference != null then this is variable reference.
	/// if MethodDefinition != null then this is method reference.
	///</summary>
	public class VariableOrMemberReference : Expression // todo
	{
		public IVariable VariableOrFieldReference;
		public MethodDefinition MethodReference;
		public readonly string MemberName;

		public VariableOrMemberReference(string name) => MemberName = name;

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format(" Variable {0}\n", MemberName);
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class Literal : Expression
	{
		public VariableInfo Value;

		public Literal(Token token) => Value = new VariableInfo(Type.Of(token), null) { Value = token.Value };

		public Literal(Type type, object value) => Value = new VariableInfo(type, null) { Value = value };

		public override string ToString(string indent, bool last) => 
			GetLogDecoration(indent, last).Prefix + string.Format(" Literal {0}\n", Value.Value.ToString());
	}

	public abstract class Operation : Expression
	{
		public readonly string StringRepresentation;
		public readonly Operator Operator;

		public Operation(string stringRepresentation, Operator @operator)
		{
			StringRepresentation = stringRepresentation;
			Operator = @operator;
		}

		public static bool IsArithmeticSupported(Type type) =>
			type.Info == Type.IntTypeInfo || type.Info == Type.FloatTypeInfo;
	}

	public class BinaryOperation : Operation
	{
		public Expression Left { get => (Expression)Children[0]; set => Children[0] = value; }
		public Expression Right { get => (Expression)Children[1]; set => Children[1] = value; }

		public BinaryOperation(Token token, Expression left, Expression right)
			: base(token.RawValue, (Operator)token.Value)
		{
			Children.Capacity = 2;
			Children.Add(left);
			Children.Add(right);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format(" Binary {0}\n", StringRepresentation);
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class UnaryOperation : Operation
	{
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }

		public UnaryOperation(Token token, Expression child)
			: base(token.RawValue, (Operator)token.Value)
		{
			Children.Capacity = 1;
			Children.Add(child);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format(" Unary {0}\n", StringRepresentation);
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class ArrayCreation : Expression
	{
		public readonly Type Type;
		public readonly Expression ArraySize;

		public ArrayCreation(Type type, Expression size, List<Expression> data)
		{
			Type = type;
			ArraySize = size;
			Children = new List<Node>(data);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + 
				string.Format(" new array of {0}\n", Type.Info.Name) + 
				ArraySize.ToString(decoration.Indent, false);
			for (int i = 0; i < Children.Count; i++) {
				if (Children[i] != null) {
					res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
				} else {
					var nullDecoration = GetLogDecoration(decoration.Indent, i == Children.Count - 1);
					res += nullDecoration.Prefix + " null\n";
				}
			}
			return res;
		}
	}

	public class ArrayAccess : Expression
	{
		public Expression Index { get => (Expression)Children[0]; set => Children[0] = value; }
		public Expression Child { get => (Expression)Children[1]; set => Children[1] = value; }

		public ArrayAccess(Expression index, Expression child)
		{
			Children.Capacity = 2;
			Children.Add(index);
			Children.Add(child);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + "[] index child\n";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class MemberAccess : Expression
	{
		public readonly string MemberName;
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }

		public MemberAccess(string name, Expression child)
		{
			MemberName = name;
			Children.Capacity = 1;
			Children.Add(child);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + " ." + MemberName + "\n";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class Parenthesis : Expression
	{
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }

		public Parenthesis(Expression child)
		{
			Children.Capacity = 1;
			Children.Add(child);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + "() Parenthesis\n";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class Invocation : Expression
	{
		public static readonly Invocation Constructor = new Invocation(null, null);

		public readonly List<Expression> Parameters;
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }

		public Invocation(List<Expression> parameters, Expression child)
		{
			Parameters = parameters;
			Children.Capacity = 1;
			Children.Add(child);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + " () Invocation\n";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, Parameters.Count == 0);
			}
			for (int i = 0; i < Parameters.Count; i++) {
				if (Parameters[i] != null) {
					res += Parameters[i].ToString(decoration.Indent, i == Parameters.Count - 1);
				} else {
					res += decoration.Indent + (i == Parameters.Count - 1 ? "└─" : "├─") + " null";
				}
			}
			return res;
		}
	}

	public class TypeReference : Expression
	{
		public readonly TypeInfo TypeInfo;

		public TypeReference(TypeInfo typeInfo)
		{
			TypeInfo = typeInfo;
			Children.Capacity = 0;
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + " " + TypeInfo.Name + " TypeReference\n";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class TypeCast : Expression
	{
		public readonly TypeInfo Type;
		public readonly Expression Child;

		public TypeCast(TypeInfo type, Expression child)
		{
			Type = type;
			Children.Capacity = 1;
			Children.Add(child);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format(" ({0}) TypeCast\n", Type.Name);
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public interface IStatement { }

	public class VariableDefinition : Node, IStatement
	{
		public readonly Type Type;
		public readonly string Name;
		public object Value;

		public VariableDefinition(Type type, string name)
		{
			Type = type;
			Name = name;
		}

		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public class CallStatement : Node, IStatement
	{
		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public class Assignment : Node, IStatement
	{
		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public class Block : Node, IStatement, IScope
	{
		public Dictionary<string, VariableInfo> Variables { get; set; }

		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public class If : Node, IStatement
	{
		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public class While : Node, IStatement
	{
		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}

	public class Break : Node, IStatement
	{
		public override string ToString(string indent, bool last) => throw new NotImplementedException();
	}
}
