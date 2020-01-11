using System;
using System.Collections.Generic;
using System.Text;
using Shared;

namespace Parser
{
	using Token = Lexer.Token;

	public abstract class Node
	{
		public List<Node> Children = new List<Node>();

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

		protected string TypeToString(Type type) => type == null ? "null" :
			type.Info.Name + new StringBuilder().Insert(0, "[]", (int)type.ArrayRang);
	}

	public interface IScope
	{
		Dictionary<string, IVariable> Variables { get; }
	}

	public class Scope : IScope
	{
		public Dictionary<string, IVariable> Variables { get; } = new Dictionary<string, IVariable>();
	}

	public interface IStatement
	{
		bool IsStatement { get; }
	}

	public interface IValue
	{
		bool IsVariable { get; }
	}

	public class TypeDefinition : Node, IScope
	{
		public readonly TypeInfo TypeInfo;
		public Dictionary<string, IVariable> Variables { get; }

		public TypeDefinition(TypeInfo typeInfo)
		{
			TypeInfo = typeInfo;
			Variables = new Dictionary<string, IVariable>();
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format("public class {0}\n", TypeInfo.Name);
			for (int i = 0; i < Children.Count; i++) {
				if (Children[i] != null) {
					res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
				}
			}
			return res;
		}
	}

	public class FieldDefinition : Node
	{
		public readonly TypeInfo.FieldInfo FieldInfo;
		public Expression Initializer { get => (Expression)Children[0]; set => Children[0] = value; }

		public FieldDefinition(TypeInfo.FieldInfo fieldInfo, Expression initializer)
		{
			FieldInfo = fieldInfo;
			Children.Capacity = 1;
			Children.Add(initializer);
			if (!initializer.Type.Equals(FieldInfo.Type)) {
				throw new ParserException(string.Format(
					"FieldDefinition wrong initializer type: field {0}, initializer {1}",
					TypeToString(FieldInfo.Type), TypeToString(initializer.Type)));
			}
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format("public {0}{1} {2}\n",
				FieldInfo.IsStatic ? "static " : "", TypeToString(FieldInfo.Type), FieldInfo.Name);
			for (int i = 0; i < Children.Count; i++) {
				if (Children[i] != null) {
					res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
				}
			}
			return res;
		}
	}

	public class MethodDefinition : Node
	{
		public readonly TypeInfo.MethodInfo MethodInfo;

		public MethodDefinition(TypeInfo.MethodInfo methodInfo, List<Node> children)
		{
			MethodInfo = methodInfo;
			Children.Capacity = 1;
			Children = children;
			CheckReturns();
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format("public {0}{1} {2}\n", 
				MethodInfo.IsStatic ? "static " : "", TypeToString(MethodInfo.OutputType), MethodInfo.Name);
			for (int i = 0; i < Children.Count; i++) {
				if (Children[i] != null) {
					res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
				}
			}
			return res;
		}

		private void CheckReturns()
		{
			bool hasReturn = MethodInfo.OutputType.Equals(new Type(Type.VoidTypeInfo));
			if (!hasReturn) {
				foreach (var node in Children) {
					hasReturn |= node is Return;
				}
			}
			if (!hasReturn) {
				throw new ParserException(string.Format(
					"Method {0} without return in the end", MethodInfo.Name));
			}
		}
	}

	public abstract class Expression : Node, ITypeProvider
	{
		public abstract Type Type { get; }

		public Expression() { }
	}

	public class ObjectCreation : Expression, IStatement, IValue
	{
		public override Type Type { get; }
		public bool IsVariable => false;
		public bool IsStatement => true;

		public ObjectCreation(TypeInfo typeInfo, Invocation invocation)
		{
			Type = new Type(typeInfo);
			Children = new List<Node>(invocation.Parameters);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format(" new {0}(...)\n", Type.Info.Name);
			for (int i = 0; i < Children.Count; i++) {
				if (Children[i] != null) {
					res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
				}
			}
			return res;
		}
	}

	public class MethodReference : Expression, IValue
	{
		public readonly TypeInfo.MethodInfo Method;
		public override Type Type => Method.OutputType;
		public bool IsVariable => false;

		public MethodReference(TypeInfo.MethodInfo method)
		{
			this.Method = method;
			Children.Capacity = 0;
		}

		public override string ToString(string indent, bool last) =>
			GetLogDecoration(indent, last).Prefix + string.Format(
				" Method {0} {1}\n", TypeToString(Type), Method.Name);
	}

	public class VariableReference : Expression, IValue
	{
		public readonly IVariable variable;
		public override Type Type => variable.Type;
		public bool IsVariable => true;

		public VariableReference(IVariable variable)
		{
			this.variable = variable;
			Children.Capacity = 0;
		}

		public bool IsBoolean => throw new NotImplementedException();

		public override string ToString(string indent, bool last) => 
			GetLogDecoration(indent, last).Prefix + string.Format(
				" Variable {0} {1}\n", TypeToString(Type), variable.Name);
	}

	public class Literal : Expression, IValue
	{
		public override Type Type { get; }
		public object Value { get; private set; }
		public bool IsVariable => false;

		public Literal(Token token)
		{
			Type = Type.Of(token);
			Value = token.Value;
		}

		public Literal(Type type, object value)
		{
			Type = type;
			Value = value;
		}

		public override string ToString(string indent, bool last) => 
			GetLogDecoration(indent, last).Prefix + string.Format(
				" Literal {0} type {1}\n", Value?.ToString() ?? "null", TypeToString(Type));
	}

	public abstract class Operation : Expression, IValue
	{
		public readonly string StringRepresentation;
		public readonly Operator Operator;
		public bool IsVariable => false;

		public Operation(string stringRepresentation, Operator @operator)
		{
			StringRepresentation = stringRepresentation;
			Operator = @operator;
		}

		public static bool IsArithmeticSupported(Type type) =>
			type.Info == Type.IntTypeInfo || type.Info == Type.FloatTypeInfo;
	}

	public class BinaryOperation : Operation, IStatement
	{
		private Type type;
		public bool IsStatement => Operator.HasFlag(Operator.Assignment);
		public Expression Left { get => (Expression)Children[0]; set => Children[0] = value; }
		public Expression Right { get => (Expression)Children[1]; set => Children[1] = value; }

		public BinaryOperation(Token token, Expression left, Expression right)
			: base(token.RawValue, (Operator)token.Value)
		{
			Children.Capacity = 2;
			Children.Add(left);
			Children.Add(right);
		}

		public override Type Type
		{
			get
			{
				if (type == null) {
					var leftType = Left.Type;
					var rightType = Right.Type;
					if (!leftType.Equals(rightType)) {
						throw new ParserException(string.Format(
							"Binary operation different types: {0} and {1}", 
							TypeToString(leftType), TypeToString(rightType)));
					}
					if (Operator.HasFlag(Operator.BoolOperator)) {
						if (!Operator.HasFlag(Operator.СomparisonOperator) && !leftType.Info.IsBoolean) {
							throw new ParserException(string.Format(
								"Operation {0} not supported on type {1}",
								StringRepresentation, TypeToString(leftType)));
						}
						type = new Type(Type.BoolTypeInfo);
					}
					if (Operator.HasFlag(Operator.ArithmeticalOperator)) {
						if (!leftType.Info.IsArithmetical) {
							throw new ParserException(string.Format(
								"Operation {0} not supported on type {1}",
								StringRepresentation, TypeToString(leftType)));
						}
						type = leftType;
					}
				}
				return type;
			}
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			string res = "";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return decoration.Prefix + string.Format(" Binary {0} type {1}\n", 
				StringRepresentation, TypeToString(Type)) + res;
		}
	}

	public class UnaryOperation : Operation
	{
		public override Type Type => Child.Type;
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
			string res = "";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return decoration.Prefix + string.Format(" Unary {0} type {1}\n", 
				StringRepresentation, TypeToString(Type)) + res;
		}
	}

	public class ArrayCreation : Expression, IValue
	{
		public override Type Type { get; }
		public Expression ArraySize { get => (Expression)Children[0]; set => Children[0] = value; }
		public bool IsVariable => false;

		public ArrayCreation(Type type, Expression size, List<Expression> data)
		{
			Type = type;
			Children = new List<Node>(data.Count + 1);
			Children.Add(size);
			Children.AddRange(data);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format(" new {0}[...]{1}\n",
				Type.Info.Name, new StringBuilder().Insert(0, "[]", (int)Type.ArrayRang - 1));
			res += ArraySize.ToString(decoration.Indent, false);
			for (int i = 1; i < Children.Count; i++) {
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

	public class ArrayAccess : Expression, IValue
	{
		public override Type Type => new Type(Child.Type.Info);
		public Expression Index { get => (Expression)Children[1]; set => Children[1] = value; }
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }
		public bool IsVariable => true;

		public ArrayAccess(Expression index, Expression child)
		{
			Children.Capacity = 2;
			Children.Add(index);
			Children.Add(child);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			string res = "";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return decoration.Prefix + string.Format(
				"...[...] type {0}\n", TypeToString(Type)) + res;
		}
	}

	public class MemberAccess : Expression, IValue
	{
		public readonly string MemberName;
		/// <summary><see cref="TypeInfo.FieldInfo"/> or <see cref="TypeInfo.MethodInfo"/></summary>
		public IVariable Info { get; private set; }
		/// <summary>Field type or method out type.</summary>
		public override Type Type => Info.Type;
		public bool IsVariable => Info is TypeInfo.FieldInfo;
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }

		public MemberAccess(string name, Expression child)
		{
			MemberName = name;
			Children.Capacity = 1;
			Children.Add(child);
			var type = Child.Type;
			if (type.ArrayRang > 0) {
				throw new ParserException("Arrays have no fields and methods");
			}
			bool isStatic = false;
			TypeInfo.FieldInfo fieldInfo;
			if (type.Info.Fields.TryGetValue(MemberName, out fieldInfo)) {
				Info = fieldInfo;
				isStatic = fieldInfo.IsStatic;
			}
			TypeInfo.MethodInfo methodInfo;
			if (type.Info.Methods.TryGetValue(MemberName, out methodInfo)) {
				Info = methodInfo;
				isStatic = methodInfo.IsStatic;
			}
			if (Info == null) {
				throw new ParserException(string.Format(
					"Type {0} has no member {1}",
					type.Info.Name, MemberName));
			}
			if (child is TypeReference && !isStatic) {
				throw new ParserException(string.Format(
					"Member {0} of type {1} is not static",
					MemberName, type.Info.Name));
			}
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = string.Format("{0} ... .{1} type {2}\n", 
				decoration.Prefix, MemberName, TypeToString(Type));
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class Parenthesis : Expression
	{
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }
		public override Type Type => Child.Type;

		public Parenthesis(Expression child)
		{
			Children.Capacity = 1;
			Children.Add(child);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			string res = "";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return decoration.Prefix + string.Format("() Parenthesis type {0}\n", TypeToString(Type)) + res;
		}
	}

	public class Invocation : Expression, IStatement
	{
		public static readonly Invocation Constructor = new Invocation();

		public readonly List<Expression> Parameters;
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }
		public override Type Type => Child.Type;
		public bool IsStatement => true;

		private Invocation() { }

		public Invocation(List<Expression> parameters, Expression child, TypeInfo.MethodInfo currentMethod)
		{
			Parameters = parameters;
			Children.Capacity = 1;
			Children.Add(child);
			if (child != Constructor) {
				TypeInfo.MethodInfo methodInfo = null;
				if (child is MethodReference methodReference) {
					if (!methodReference.Method.IsStatic && (currentMethod?.IsStatic ?? false)) {
						throw new ParserException(string.Format("Method '{0}' is not static in method '{1}'",
							methodReference.Method.Name, currentMethod.Name));
					}
					methodInfo = methodReference.Method;
				}
				if (child is MemberAccess member && !member.IsVariable) {
					methodInfo = (TypeInfo.MethodInfo)member.Info;
				}
				if (methodInfo == null) {
					throw new ParserException("Invalid invocation target!\n" + Child.ToString());
				}
				if (methodInfo.Prams.Count != parameters.Count) {
					throw new ParserException("Wrong invocation parameters count!\n");
				}
				for (int i = 0; i < parameters.Count; i++) {
					if (!methodInfo.Prams[i].Type.Equals(parameters[i].Type)) {
						throw new ParserException("Invocation wrong parameter type!\n");
					}
				}
			}
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			string res = "";
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
			return decoration.Prefix + string.Format(" () Invocation type {0}\n", TypeToString(Type)) + res;
		}
	}

	public class TypeReference : Expression
	{
		public override Type Type { get; }

		public TypeReference(TypeInfo typeInfo)
		{
			Type = new Type(typeInfo);
			Children.Capacity = 0;
		}

		public override string ToString(string indent, bool last) => 
			GetLogDecoration(indent, last).Prefix + " " + TypeToString(Type) + " TypeReference\n";
	}

	public class TypeCast : Expression
	{
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }
		public override Type Type { get; }

		public TypeCast(Type type, Expression child)
		{
			var childType = child.Type;
			Type = type;
			Children.Capacity = 1;
			Children.Add(child);
			var intType = new Type(Type.IntTypeInfo);
			var floatType = new Type(Type.FloatTypeInfo);
			if (!childType.Equals(intType) && !childType.Equals(floatType)) {
				throw new ParserException(string.Format(
					"Wrong typecast: bad source type {0}", childType.Info.Name));
			}
			if (!Type.Equals(intType) && !Type.Equals(floatType)) {
				throw new ParserException(string.Format(
					"Wrong typecast: bad destination type {0}", Type.Info.Name));
			}
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			string res = "";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return decoration.Prefix + string.Format(" ({0}) TypeCast\n", TypeToString(Type)) + res;
		}
	}

	public class VariableDefinition : Node, IStatement, IVariable
	{
		public Type Type { get; private set; }
		public string Name { get; private set; }
		public Expression Value { get => (Expression)Children[0]; }
		public bool IsStatement => true;

		public VariableDefinition(Type type, string name, Expression value)
		{
			Type = type;
			Name = name;
			Children.Capacity = 1;
			Children.Add(value);
			if (!value.Type.Equals(Type)) {
				throw new ParserException(string.Format(
					"VariableDefinition wrong value type: variable {0}, value {1}",
					TypeToString(Type), TypeToString(value.Type)));
			}
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + string.Format("{0} {1}\n", TypeToString(Type), Name);
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i]?.ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class Block : Node, IStatement, IScope
	{
		public Dictionary<string, IVariable> Variables { get; }
		public bool IsStatement => true;

		public Block() => Variables = new Dictionary<string, IVariable>();

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + "{...}\n";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i].ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class If : Node, IStatement
	{
		public Expression Condition { get => (Expression)Children[0]; set => Children[0] = value; }
		public bool IsStatement => true;

		/// <summary>
		/// Represents <see cref="Block"/> or <see cref="If"/>>
		/// </summary>
		public Node BlockStatement { get => Children[1]; set => Children[1] = value; }

		/// <summary>
		/// Represents <see cref="Block"/> or <see cref="If"/>>
		/// </summary>
		public Node ElseStatement { get => Children[2]; set => Children[2] = value; }

		public If(Expression condition, Node blockStatement, Node elseStatement = null)
		{
			Children.Capacity = 3;
			Children.Add(condition);
			Children.Add(blockStatement);
			Children.Add(elseStatement);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + "if\n";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i]?.ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class For : Node, IStatement
	{
		public bool IsStatement => true;

		/// <summary>
		/// Can be null! 
		/// </summary>
		public VariableDefinition Initializer 
		{ 
			get => (VariableDefinition)Children[0];
			set => Children[0] = value;
		}

		public Expression Condition { get => (Expression)Children[1]; set => Children[1] = value; }

		/// <summary>
		/// Can be null! 
		/// </summary>
		public Expression Iterator { get => (Expression)Children[2]; set => Children[2] = value; }

		public Block BlockStatement { get => (Block)Children[3]; set => Children[3] = value; }

		public For(Node initializer, Expression condition, Expression iterator, Block blockStatement)
		{
			Children.Capacity = 4;
			Children.Add(initializer);
			Children.Add(condition);
			Children.Add(iterator);
			Children.Add(blockStatement);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + "for\n";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i]?.ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class While : Node, IStatement
	{
		public Expression Condition { get => (Expression)Children[0]; set => Children[0] = value; }
		public Block BlockStatement { get => (Block)Children[1]; set => Children[1] = value; }
		public bool IsStatement => true;

		public While(Expression condition, Block blockStatement)
		{
			Children.Capacity = 2;
			Children.Add(condition);
			Children.Add(blockStatement);
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			var res = decoration.Prefix + "while\n";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i]?.ToString(decoration.Indent, i == Children.Count - 1);
			}
			return res;
		}
	}

	public class Break : Node, IStatement
	{
		public bool IsStatement => true;

		public override string ToString(string indent, bool last) => 
			GetLogDecoration(indent, true).Prefix + "break\n";
	}

	public class SemiColon : Node, IStatement
	{
		public bool IsStatement => true;

		public override string ToString(string indent, bool last) => 
			GetLogDecoration(indent, true).Prefix + ";\n";
	}

	public class Return : Node, IStatement, ITypeProvider
	{
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }
		public Type Type => Child?.Type;
		public bool IsStatement => true;

		public Return(Expression child, TypeInfo.MethodInfo currentMethod, Token returnToken)
		{
			Children.Capacity = 1;
			Children.Add(child);
			if (currentMethod.OutputType.Equals(new Type(Type.VoidTypeInfo))) {
				if (Child != null) {
					throw new ParserException(returnToken, string.Format(
						"Method {0} return {1}, but expected nothing",
						currentMethod.Name, Child.ToString()));
				}
			} else if (
				(Type == null && !currentMethod.OutputType.IsReference()) ||
				(Type != null && !currentMethod.OutputType.Equals(Type))) 
			{
				throw new ParserException(returnToken, string.Format(
					"Method {0} return wrong type: {1}, but expected {2}",
					currentMethod.Name, TypeToString(Type), TypeToString(currentMethod.OutputType)));
			}
		}

		public override string ToString(string indent, bool last)
		{
			var decoration = GetLogDecoration(indent, last);
			string res = "";
			for (int i = 0; i < Children.Count; i++) {
				res += Children[i]?.ToString(decoration.Indent, i == Children.Count - 1);
			}
			return decoration.Prefix + string.Format("return ... type {0}\n", TypeToString(Type)) + res;
		}
	}
}
