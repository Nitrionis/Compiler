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

		public static string TypeToString(Type type) => type == null ? "null" :
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

	public interface IExecutable
	{
		/// <summary>Performs a subtree.</summary>
		/// <returns>Returns null if the node does not return a value.</returns>
		TypeInstance Execute();
	}

	public interface IStatement : IExecutable
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
		public Dictionary<string, IVariable> Variables => TypeInfo.Fields;

		public TypeDefinition(TypeInfo typeInfo) => TypeInfo = typeInfo;

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
			if (!Assignment.Can(FieldInfo.Type, initializer.Type)) {
				throw new ParserException(string.Format(
					"FieldDefinition wrong initializer type: field {0}, initializer {1}",
					TypeToString(FieldInfo.Type), TypeToString(initializer.Type)));
			}
			fieldInfo.Initializer = initializer;
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

		public MethodDefinition(TypeInfo.MethodInfo methodInfo, Block block)
		{
			MethodInfo = methodInfo;
			Children.Capacity = 1;
			Children = block.Children;
			CheckReturns();
			methodInfo.Body = block;
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
			bool hasReturn = Type.Equals(MethodInfo.OutputType, Type.VoidTypeInfo);
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

	public abstract class Expression : Node, ITypeProvider, IExecutable
	{
		public abstract Type Type { get; }

		public Expression() { }

		public abstract TypeInstance Execute();
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
			if (Type.Predefined.ContainsKey(typeInfo.Name)) {
				throw new ParserException(string.Format(
					"The type {0} has no constructor", typeInfo.Name));
			}
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

		public override TypeInstance Execute() => new TypeInstance(Type);
	}

	public class MethodReference : Expression, IValue
	{
		public readonly TypeInfo.MethodInfo Method;
		public override Type Type => Method.OutputType;
		public bool IsVariable => false;

		public MethodReference(TypeInfo.MethodInfo method)
		{
			Method = method;
			Children.Capacity = 0;
		}

		public override string ToString(string indent, bool last) =>
			GetLogDecoration(indent, last).Prefix + string.Format(
				" Method {0} {1}\n", TypeToString(Type), Method.Name);

		public override TypeInstance Execute() => throw new InvalidOperationException();
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

		public override TypeInstance Execute() => Context.Current.Get(variable);
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

		public override TypeInstance Execute() => new TypeInstance(Value);
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
		public override Type Type => type;
		public Expression Left { get => (Expression)Children[0]; set => Children[0] = value; }
		public Expression Right { get => (Expression)Children[1]; set => Children[1] = value; }

		public BinaryOperation(Token token, Expression left, Expression right)
			: base(token.RawValue, (Operator)token.Value)
		{
			Children.Capacity = 2;
			Children.Add(left);
			Children.Add(right);
			var leftType = Left.Type;
			var rightType = Right.Type;
			if (!Type.Equals(leftType, rightType)) {
				throw new ParserException(token, string.Format(
					"Binary operation different types: '{0}' and '{1}'",
					TypeToString(leftType), TypeToString(rightType)));
			}
			if (Operator.HasFlag(Operator.BoolOperator)) {
				if (!Operator.HasFlag(Operator.СomparisonOperator) && !leftType.Info.IsBoolean) {
					throw new ParserException(token, string.Format(
						"Operation '{0}' not supported on type '{1}'",
						StringRepresentation, TypeToString(leftType)));
				}
				type = new Type(Type.BoolTypeInfo);
			}
			if (Operator.HasFlag(Operator.ArithmeticalOperator)) {
				type = leftType;
				if (Operator != Operator.Assignment && !type.Info.IsArithmetical) {
					throw new ParserException(token, string.Format(
						"Operation '{0}' not supported on type '{1}'",
						StringRepresentation, TypeToString(type)));
				}
			}
			if (ReferenceEquals(type.Info, Type.VoidTypeInfo)) {
				throw new ParserException(token, string.Format(
					"Operation '{0}' void type arguments", StringRepresentation));
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

		public override TypeInstance Execute()
		{
			var parametersType = Left.Type;
			var leftRes = Left.Execute();
			var rightRes = Right.Execute();
			switch (Operator) {
				case Operator.Assignment:
					leftRes.Value = rightRes.Value;
					return leftRes;
				case Operator.EqualityTest:
					if (parametersType.ArrayRang == 0) {
						return new TypeInstance(parametersType.Info.EqualityTest(leftRes.Value, rightRes.Value));
					} else {
						return new TypeInstance(ReferenceEquals(leftRes.Value, rightRes.Value));
					}
				case Operator.NotEqualityTest:
					if (parametersType.ArrayRang == 0) {
						return new TypeInstance(!parametersType.Info.EqualityTest(leftRes.Value, rightRes.Value));
					} else {
						return new TypeInstance(!ReferenceEquals(leftRes.Value, rightRes.Value));
					}
				case Operator.LogicalAnd: return new TypeInstance((bool)leftRes.Value && (bool)rightRes.Value);
				case Operator.LogicalOr: return new TypeInstance((bool)leftRes.Value || (bool)rightRes.Value);
				case Operator.LessTest: return Type.Equals(parametersType, Type.IntTypeInfo) ?
					new TypeInstance((int)leftRes.Value < (int)rightRes.Value) :
					new TypeInstance((float)leftRes.Value < (float)rightRes.Value);
				case Operator.MoreTest: return Type.Equals(parametersType, Type.IntTypeInfo) ?
					new TypeInstance((int)leftRes.Value > (int)rightRes.Value) :
					new TypeInstance((float)leftRes.Value > (float)rightRes.Value);
				case Operator.Add: return Type.Equals(parametersType, Type.IntTypeInfo) ?
					new TypeInstance((int)leftRes.Value + (int)rightRes.Value) : 
					new TypeInstance((float)leftRes.Value + (float)rightRes.Value);
				case Operator.Subtract: return Type.Equals(parametersType, Type.IntTypeInfo) ?
					new TypeInstance((int)leftRes.Value - (int)rightRes.Value) :
					new TypeInstance((float)leftRes.Value - (float)rightRes.Value);
				case Operator.Multiply: return Type.Equals(parametersType, Type.IntTypeInfo) ?
					new TypeInstance((int)leftRes.Value * (int)rightRes.Value) :
					new TypeInstance((float)leftRes.Value * (float)rightRes.Value);
				case Operator.Divide: return Type.Equals(parametersType, Type.IntTypeInfo) ?
					new TypeInstance((int)leftRes.Value / (int)rightRes.Value) :
					new TypeInstance((float)leftRes.Value / (float)rightRes.Value);
				case Operator.Remainder: return Type.Equals(parametersType, Type.IntTypeInfo) ?
					new TypeInstance((int)leftRes.Value % (int)rightRes.Value) :
					new TypeInstance((float)leftRes.Value % (float)rightRes.Value);
				case Operator.BitwiseAnd: return new TypeInstance((int)leftRes.Value & (int)rightRes.Value);
				case Operator.BitwiseOr: return new TypeInstance((int)leftRes.Value | (int)rightRes.Value);
			}
			throw new InvalidOperationException();
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
			if (!Type.Info.IsArithmetical) {
				throw new ParserException(token, string.Format(
					"Operation '{0}' invalid argument type '{1}'", 
					StringRepresentation, TypeToString(Type)));
			}
			if (ReferenceEquals(Type.Info, Type.VoidTypeInfo)) {
				throw new ParserException(token, string.Format(
					"Operation '{0}' void type arguments", StringRepresentation));
			}
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

		public override TypeInstance Execute()
		{
			switch (Operator) {
				case Operator.Add: return new TypeInstance(Child.Execute().Value);
				case Operator.Subtract: return new TypeInstance(Type.Equals(Type, Type.IntTypeInfo) ? 
						-(int)Child.Execute().Value : -(float)Child.Execute().Value);
				case Operator.LogicalNot: return new TypeInstance(!(bool)Child.Execute().Value);
				case Operator.BitwiseNot: return new TypeInstance(~(int)Child.Execute().Value);
			}
			throw new InvalidOperationException();
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

		public override TypeInstance Execute() => new TypeInstance(Type, Children);
	}

	public class ArrayAccess : Expression, IValue
	{
		public override Type Type => new Type(Child.Type.Info, Child.Type.ArrayRang - 1);
		public Expression Index { get => (Expression)Children[0]; set => Children[0] = value; }
		public Expression Child { get => (Expression)Children[1]; set => Children[1] = value; }
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

		public override TypeInstance Execute() => Child.Execute().AsArray[(int)Index.Execute().Value];
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
			IVariable fieldInfo;
			if (type.Info.Fields.TryGetValue(MemberName, out fieldInfo)) {
				Info = fieldInfo;
				isStatic = ((TypeInfo.FieldInfo)fieldInfo).IsStatic;
			}
			TypeInfo.MethodInfo methodInfo;
			if (type.Info.Methods.TryGetValue(MemberName, out methodInfo)) {
				Info = methodInfo;
				isStatic = methodInfo.IsStatic;
			}
			if (Info == null) {
				throw new ParserException(string.Format(
					"Type {0} has no member {1}", type.Info.Name, MemberName));
			}
			if (child is TypeReference && !isStatic) {
				throw new ParserException(string.Format(
					"Member {0} of type {1} is not static", MemberName, type.Info.Name));
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

		public override TypeInstance Execute() => Child.Execute().AsClass.Get(Info);
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

		public override TypeInstance Execute() => Child.Execute();
	}

	public class Invocation : Expression, IStatement
	{
		public static readonly Invocation Constructor = new Invocation();

		public readonly List<Expression> Parameters;
		public Expression Child { get => (Expression)Children[0]; set => Children[0] = value; }
		public override Type Type => Child.Type;
		public bool IsStatement => true;
		public readonly TypeInfo.MethodInfo MethodInfo;

		private Invocation() { }

		public Invocation(List<Expression> parameters, Expression child, TypeInfo.MethodInfo currentMethod, Token token)
		{
			Parameters = parameters;
			Children.Capacity = 1;
			Children.Add(child);
			if (child != Constructor) {
				if (child is MethodReference methodReference) {
					if (!methodReference.Method.IsStatic && (currentMethod?.IsStatic ?? false)) {
						throw new ParserException(string.Format("Method '{0}' is not static in method '{1}'",
							methodReference.Method.Name, currentMethod.Name));
					}
					MethodInfo = methodReference.Method;
				}
				if (child is MemberAccess member && !member.IsVariable) {
					MethodInfo = (TypeInfo.MethodInfo)member.Info;
				}
				if (MethodInfo == null) {
					throw new ParserException(token, "Invalid invocation target!\n" + Child.ToString());
				}
				if (MethodInfo.Prams.Count != parameters.Count) {
					throw new ParserException(token, "Wrong invocation parameters count!");
				}
				for (int i = 0; i < parameters.Count; i++) {
					if (!Type.Equals(MethodInfo.Prams[i].Type, parameters[i].Type)) {
						throw new ParserException(token, "Invocation wrong parameter type!");
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

		public override TypeInstance Execute()
		{
			var parameters = new Dictionary<IVariable, TypeInstance>(Parameters.Count);
			for (int i = 0; i < Parameters.Count; i++) {
				parameters.Add(MethodInfo.Prams[i], Parameters[i].Execute());
			}
			if (Child is MemberAccess) {
				var owner = (Expression)Child.Children[0];
				Context.Push(new Context(owner.Execute().AsClass, parameters));
			} else {
				Context.Push(new Context(Child.Execute().AsClass, parameters));
			}
			var res = MethodInfo.Body.Execute();
			Context.Pop();
			return res;
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

		public override TypeInstance Execute() => new TypeInstance(new Context(Type.Info));
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
			if (!Type.Equals(childType, intType) && !Type.Equals(childType,floatType)) {
				throw new ParserException(string.Format(
					"Wrong typecast: bad source type {0}", childType.Info.Name));
			}
			if (!Type.Equals(Type, intType) && !Type.Equals(Type, floatType)) {
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

		public override TypeInstance Execute() => Child.Execute();
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
			if (!Assignment.Can(Type, value.Type)) {
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

		public TypeInstance Execute()
		{
			Block.Stack.Peek().LocalVariables.Add(this);
			Context.Current.LocalVariables.Add(this, new TypeInstance(Value.Execute().Value));
			return null;
		}
	}

	public class Block : Node, IStatement, IScope
	{
		public static readonly Stack<Block> Stack = new Stack<Block>();

		public readonly List<IVariable> LocalVariables = new List<IVariable>();
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

		public TypeInstance Execute()
		{
			Stack.Push(this);
			foreach (var s in Children) {
				((IExecutable)s).Execute();
			}
			Stack.Pop();
			var context = Context.Current;
			foreach (var v in LocalVariables) {
				context.LocalVariables.Remove(v);
			}
			return null;
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
			Children.Capacity = 2 + (elseStatement != null ? 1 : 0);
			Children.Add(condition);
			Children.Add(blockStatement);
			if (elseStatement != null) {
				Children.Add(elseStatement);
			}
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

		public TypeInstance Execute()
		{
			if ((bool)Condition.Execute().Value) {
				((IStatement)Children[1]).Execute();
			} else if (Children.Capacity == 3) {
				((IStatement)Children[2]).Execute();
			}
			return null;
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

		public TypeInstance Execute()
		{
			Initializer?.Execute();
			while ((bool)Condition.Execute().Value) {
				BlockStatement.Execute();
				((IExecutable)Children[2]).Execute();
			}
			return null;
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

		public TypeInstance Execute()
		{
			while ((bool)Condition.Execute().Value) {
				BlockStatement.Execute();
			}
			return null;
		}
	}

	public class Break : Node, IStatement
	{
		public bool IsStatement => true;

		public TypeInstance Execute() => throw new NotImplementedException();

		public override string ToString(string indent, bool last) => 
			GetLogDecoration(indent, true).Prefix + "break\n";
	}

	public class SemiColon : Node, IStatement
	{
		public bool IsStatement => true;

		public TypeInstance Execute() => null;

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
			if (Type.Equals(currentMethod.OutputType, Type.VoidTypeInfo)) {
				if (Child != null) {
					throw new ParserException(returnToken, string.Format(
						"Method {0} return {1}, but expected nothing",
						currentMethod.Name, Child.ToString()));
				}
			} else if (!Assignment.Can(currentMethod.OutputType, Type)) {
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

		public TypeInstance Execute()
		{
			throw new NotImplementedException();
		}
	}
}
