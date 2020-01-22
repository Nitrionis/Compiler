﻿using Lexer;
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
			public class ParamsInfo : IVariable
			{
				public Type Type { get; }
				public string Name { get; }

				public ParamsInfo(Type type, string name)
				{
					Type = type;
					Name = name;
				}
			}

			public readonly bool IsStatic;
			public readonly Type OutputType;
			public readonly string Name;
			public readonly List<ParamsInfo> Prams;

			string IVariable.Name => Name;
			Type ITypeProvider.Type => OutputType;

			public MethodInfo(Type outputType, string name, bool isStatic, List<ParamsInfo> prams = null)
			{
				OutputType = outputType;
				Name = name;
				IsStatic = isStatic;
				Prams = prams ?? new List<ParamsInfo>();
			}
		}

		public readonly string Name;
		public readonly TypeInfo Parent;
		public readonly Dictionary<string, IVariable> Fields;
		public readonly Dictionary<string, MethodInfo> Methods;
		public readonly bool IsArithmetical;
		public readonly bool IsBoolean;
		public readonly object DefaultValue;

		public bool IsReference => DefaultValue == null;

		public TypeInfo(string name, bool isArithmetical, bool isBoolean, TypeInfo parent = null, object defaultValue = null)
		{
			Name = name;
			Parent = parent;
			if (Parent != null) {
				Fields = new Dictionary<string, IVariable>(Parent.Fields);
				Methods = new Dictionary<string, MethodInfo>(Parent.Methods);
			} else {
				Fields = new Dictionary<string, IVariable>();
				Methods = new Dictionary<string, MethodInfo>();
			}
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

		public static readonly TypeInfo NullTypeInfo	= new TypeInfo("null", false, false, null, null);
		public static readonly TypeInfo VoidTypeInfo	= new TypeInfo("void", false, false, null, null);
		public static readonly TypeInfo BoolTypeInfo	= new TypeInfo("bool", false, true, null, false);
		public static readonly TypeInfo CharTypeInfo	= new TypeInfo("char", false, false, null, (char)0);
		public static readonly TypeInfo IntTypeInfo		= new TypeInfo("int", true, false, null, (int)0);
		public static readonly TypeInfo FloatTypeInfo	= new TypeInfo("float", true, false, null, (float)0.0f);
		public static readonly TypeInfo StringTypeInfo	= new TypeInfo("string", false, false, null, null);

		public readonly TypeInfo Info;
		public readonly uint ArrayRang;

		public Type(TypeInfo info, uint arrayRang = 0)
		{
			Info = info;
			ArrayRang = arrayRang;
		}

		public static bool Equals(Type t1, TypeInfo typeInfo) => 
			t1.ArrayRang == 0 && ReferenceEquals(t1.Info, typeInfo);

		public static bool Equals(TypeInfo typeInfo, Type t2) => 
			t2.ArrayRang == 0 && ReferenceEquals(typeInfo, t2.Info);

		public static bool Equals(Type t1, Type t2) => 
			t1.ArrayRang == t2.ArrayRang && ReferenceEquals(t1.Info, t2.Info);

		public bool IsReference() => Info.IsReference || ArrayRang > 0;

		public object DefaultValue() => ArrayRang > 0 ? null : Info.DefaultValue;

		static Type()
		{
			Predefined = new Dictionary<string, TypeInfo>() {
				["null"]	= NullTypeInfo,
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
						case Keyword.Null: return new Type(NullTypeInfo);
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

	public class Assignment
	{
		public static bool Can(Type dst, Type src) => 
			Type.Equals(dst, src) || dst.IsReference() && Type.Equals(src, Type.NullTypeInfo);
	}
}
