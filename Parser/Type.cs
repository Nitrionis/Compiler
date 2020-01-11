using Lexer;
using Shared;
using System;
using System.Collections.Generic;

namespace Parser
{
	public interface ITypeProvider
	{
		Type Type { get; }
	}

	public interface IVariable : ITypeProvider
	{
		string Name { get; }
	}

	public class TypeInfo
	{
		public class FieldInfo : IVariable
		{
			public Type Type { get; }
			public string Name { get; }
			public bool IsStatic { get; }

			public FieldInfo(Type type, string name, bool isStatic)
			{
				Type = type;
				Name = name;
				IsStatic = isStatic;
			}
		}

		public class MethodInfo : IVariable
		{
			public class PramsInfo : IVariable
			{
				public Type Type { get; }
				public string Name { get; }

				public PramsInfo(Type type, string name)
				{
					Type = type;
					Name = name;
				}
			}

			public readonly bool IsStatic;
			public readonly Type OutputType;
			public readonly string Name;
			public readonly List<PramsInfo> Prams;

			string IVariable.Name => Name;
			Type ITypeProvider.Type => OutputType;

			public MethodInfo(Type outputType, string name, bool isStatic, List<PramsInfo> prams = null)
			{
				OutputType = outputType;
				Name = name;
				IsStatic = isStatic;
				Prams = prams ?? new List<PramsInfo>();
			}
		}

		public readonly string Name;
		public readonly Dictionary<string, FieldInfo> Fields;
		public readonly Dictionary<string, MethodInfo> Methods;
		public readonly bool IsArithmetical;
		public readonly bool IsBoolean;
		public readonly object DefaultValue;

		public bool IsReference => DefaultValue == null;

		public TypeInfo(string name, bool isArithmetical, bool isBoolean, object defaultValue = null)
		{
			Name = name;
			Fields = new Dictionary<string, FieldInfo>();
			Methods = new Dictionary<string, MethodInfo>();
			IsArithmetical = isArithmetical;
			IsBoolean = isBoolean;
			DefaultValue = defaultValue;
		}

		public bool ContainsMember(string identifier) =>
			Fields.ContainsKey(identifier) || Methods.ContainsKey(identifier);
	}

	public class Type
	{
		public static readonly Dictionary<string, TypeInfo> Predefined;

		public static readonly TypeInfo VoidTypeInfo	= new TypeInfo("void", false, false, null);
		public static readonly TypeInfo BoolTypeInfo	= new TypeInfo("bool", false, true, false);
		public static readonly TypeInfo CharTypeInfo	= new TypeInfo("char", false, false, (char)0);
		public static readonly TypeInfo IntTypeInfo		= new TypeInfo("int", true, false, (int)0);
		public static readonly TypeInfo FloatTypeInfo	= new TypeInfo("float", true, false, (float)0.0f);
		public static readonly TypeInfo StringTypeInfo	= new TypeInfo("string", false, false, null);

		public readonly TypeInfo Info;
		public readonly uint ArrayRang;

		public Type(TypeInfo info, uint arrayRang = 0)
		{
			Info = info;
			ArrayRang = arrayRang;
		}

		public override bool Equals(object obj)
		{
			var other = (Type)obj;
			return obj == null ? false : ReferenceEquals(Info, other.Info) && ArrayRang == other.ArrayRang;
		}

		public bool IsReference() => Info.IsReference || ArrayRang > 0;

		public object DefaultValue() => ArrayRang > 0 ? null : Info.DefaultValue;

		static Type()
		{
			Predefined = new Dictionary<string, TypeInfo>() {
				["void"]	= VoidTypeInfo,
				["bool"]	= BoolTypeInfo,
				["char"]	= CharTypeInfo,
				["int"]		= IntTypeInfo,
				["float"]	= FloatTypeInfo,
				["string"]	= StringTypeInfo,
			};
		}

		public static Type Of(Token token)
		{
			switch (token.Type) {
				case Token.Types.Keyword:
					switch ((Keyword)token.Value) {
						case Keyword.Void: return new Type(VoidTypeInfo);
						case Keyword.True: return new Type(BoolTypeInfo);
						case Keyword.False: return new Type(BoolTypeInfo);
						case Keyword.Int: return new Type(IntTypeInfo);
						case Keyword.Float: return new Type(FloatTypeInfo);
						case Keyword.Char: return new Type(CharTypeInfo);
						case Keyword.String: return new Type(StringTypeInfo);
						default: throw new InvalidOperationException();
					}
				case Token.Types.Int: return new Type(IntTypeInfo);
				case Token.Types.Char: return new Type(CharTypeInfo);
				case Token.Types.Float: return new Type(FloatTypeInfo);
				case Token.Types.String: return new Type(StringTypeInfo);
				default: throw new InvalidOperationException();
			}
		}
	}
}
