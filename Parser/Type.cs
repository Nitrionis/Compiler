using Lexer;
using Shared;
using System;
using System.Collections.Generic;

namespace Parser
{
	public class TypeInfo
	{
		public class FieldInfo
		{
			public readonly Type Type;
			public readonly string Name;
		}

		public class MethodInfo
		{
			public class PramsInfo
			{
				public readonly Type Type;
				public readonly string Name;

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

			public MethodInfo(Type outputType, string name, List<PramsInfo> prams = null)
			{
				OutputType = outputType;
				Name = name;
				Prams = prams ?? new List<PramsInfo>();
			}
		}

		public readonly string Name;
		public readonly Dictionary<string, FieldInfo> Fields;
		public readonly Dictionary<string, MethodInfo> Methods;

		public TypeInfo(string name)
		{
			Name = name;
			Fields = new Dictionary<string, FieldInfo>();
			Methods = new Dictionary<string, MethodInfo>();
		}

		public bool ContainsMember(string identifier) =>
			Fields.ContainsKey(identifier) || Methods.ContainsKey(identifier);
	}

	public class Type
	{
		public static readonly Dictionary<string, TypeInfo> Predefined;

		public static readonly TypeInfo VoidTypeInfo	= new TypeInfo("void");
		public static readonly TypeInfo BoolTypeInfo	= new TypeInfo("bool");
		public static readonly TypeInfo CharTypeInfo	= new TypeInfo("char");
		public static readonly TypeInfo IntTypeInfo		= new TypeInfo("int");
		public static readonly TypeInfo FloatTypeInfo	= new TypeInfo("float");
		public static readonly TypeInfo StringTypeInfo	= new TypeInfo("string");

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
			return ReferenceEquals(Info, other.Info) && ArrayRang == other.ArrayRang;
		}

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

	public class VariableInfo
	{
		public readonly Type Type;
		public readonly string Name;
		public object Value;

		public VariableInfo(Type type, string name)
		{
			Type = type;
			Name = name;
		}
	}
}
