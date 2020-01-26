using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Parser
{
	public class Context
	{
		private static Stack<Context> stack = new Stack<Context>();

		public static Context Current => stack.Peek();

		/// <summary>
		/// Non-static class fields.
		/// </summary>
		public readonly Dictionary<IVariable, TypeInstance> Fields;

		/// <summary>
		/// Static class fields.
		/// </summary>
		public readonly Dictionary<IVariable, TypeInstance> StaticFields;

		/// <summary>
		/// Local variables and method parameters.
		/// </summary>
		public readonly Dictionary<IVariable, TypeInstance> LocalVariables;

		/// <summary>
		/// Creates a context for a new object.
		/// </summary>
		public Context(TypeInfo typeInfo, Dictionary<IVariable, TypeInstance> fields = null)
		{
			Fields = fields ?? null;
			StaticFields = typeInfo.StaticFields;
			LocalVariables = new Dictionary<IVariable, TypeInstance>();
		}

		/// <summary>
		/// Creates a copy of the context without local variables.
		/// </summary>
		public Context(Context context)
		{
			Fields = context.Fields;
			StaticFields = context.StaticFields;
			LocalVariables = new Dictionary<IVariable, TypeInstance>();
		}

		public static void Push(Context context) => stack.Push(context);
		public static void Pop() => stack.Pop();

		public TypeInstance Get(IVariable variable)
		{
			if (variable is TypeInfo.FieldInfo field) {
				if (field.IsStatic) {
					return StaticFields[variable];
				} else {
					return Fields[variable];
				}
			} else {
				return LocalVariables[variable];
			}
		}
	}

	/// <summary>
	/// This is reference on a object.
	/// </summary>
	public class TypeInstance
	{
		public object Value;

		/// <summary>
		/// Valid if this is a class instance.
		/// </summary>
		public Context AsClass => (Context)Value;

		/// <summary>
		/// Valid if this is an array of something.
		/// </summary>
		public TypeInstance[] AsArray => (TypeInstance[])Value;

		/// <summary>
		/// Constructor for predefined types.
		/// </summary>
		public TypeInstance(object value) => Value = value;

		/// <summary>
		/// Constructor for single object instance.
		/// </summary>
		/// <exception cref="InvalidOperationException">TypeInfo is predefined.</exception>
		public TypeInstance(TypeInfo typeInfo) => Value = SingleObjectConstructor(typeInfo);

		/// <summary>
		/// Constructor for single object instance and for array.
		/// </summary>
		/// <param name="size">Array size at [0] and [1], [2], [3], ... is array elements.</param>
		public TypeInstance(Type type, List<Node> data = null)
		{
			if (type.ArrayRang > 0) {
				var arr = new object[(int)((IExecutable)data[0]).Execute().Value];
				int index = 0;
				foreach (var item in data.Skip(1)) {
					arr[index++] = ((IExecutable)item).Execute().Value;
				}
				Value = arr;
			} else {
				Value = SingleObjectConstructor(type.Info);
			}
		}

		private static object SingleObjectConstructor(TypeInfo typeInfo)
		{
#if DEBUG
			if (Type.Predefined.ContainsKey(typeInfo.Name)) {
				throw new InvalidOperationException("Wrong constructor");
			}
#endif
			var fields = new Dictionary<IVariable, object>(typeInfo.Fields.Count);
			foreach (var f in typeInfo.StaticFields) {
				fields.Add(f.Key, f.Value);
			}
			foreach (var f in typeInfo.Fields) {
				var field = (TypeInfo.FieldInfo)f.Value;
				if (!field.IsStatic) {
					fields.Add(field, field.Initializer.Execute());
				}
			}
			return fields;
		}
	}

	public class Runtime
	{
		private readonly Parser parser;

		protected Runtime(Parser parser)
		{
			this.parser = parser;
		}

		public Runtime(string path) : this(new Parser(path)) { }
		public Runtime(Stream stream) : this(new Parser(stream)) { }

		public void Execute()
		{
			var program = parser.ParseProgram();
			TypeInfo typeInfo = null;
			TypeInfo.MethodInfo enterPoint = null;
			foreach (var t in parser.Types) {
				TypeInfo.MethodInfo method = null;
				if (t.Value.Methods.TryGetValue("Main", out method)) {
					if (enterPoint != null) {
						throw new ParserException("Multiple program entry points");
					}
					typeInfo = t.Value;
					enterPoint = method;
				}
				foreach (var f in t.Value.Fields) {
					var field = (TypeInfo.FieldInfo)f.Value;
					if (field.Name == "Main") {
						throw new ParserException("Identifier 'Main' not available for class field");
					}
					// initialize static class variables
					if (field.IsStatic && !t.Value.StaticFields.ContainsKey(field)) {
						t.Value.StaticFields.Add(field, ((IExecutable)field.Initializer).Execute());
					}
				}
			}
			if (!Type.Equals(enterPoint.OutputType, Type.VoidTypeInfo)) {
				throw new ParserException("Entry point invalid type");
			}
			if (enterPoint.Prams.Count != 0) {
				throw new ParserException("Entry point invalid params count");
			}
			Context.Push(new Context(typeInfo.StaticFields));
			enterPoint.Body.Execute();
		}
	}
}
