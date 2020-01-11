namespace Shared
{
	public enum Keyword : uint
	{
		Type		= 1 << 1,
		Void		= 1 << 2 | Type,
		Int			= 1 << 3 | Type,
		Float		= 1 << 4 | Type,
		Char		= 1 << 5 | Type,
		String		= 1 << 6 | Type,
		Bool		= 1 << 7 | Type,
		Class		= 1 << 8,
		Logic		= 1 << 9,
		If			= 1 << 10 | Logic,
		Else		= 1 << 11 | Logic,
		For			= 1 << 12 | Logic,
		While		= 1 << 13 | Logic,
		Return		= 1 << 14,
		Break		= 1 << 15,
		Modifier	= 1 << 16,
		Public		= 1 << 17 | Modifier,
		Static		= 1 << 18 | Modifier,
		New			= 1 << 19,
		True		= 1 << 20 | Bool,
		False		= 1 << 21 | Bool,
		Null		= 1 << 22
	}

	public enum Operator : ulong
	{
		/// <summary>Returns a type that supports arithmetic.</summary>
		ArithmeticalOperator	= 1ul << 0,
		Assignment				= 1ul << 1 | ArithmeticalOperator,
		/// <summary>This flag indicates that the operator can be unary.</summary>
		UnaryOperator			= 1ul << 2,
		/// <summary>The operator belong to the additive group.</summary>
		AdditiveOperator		= 1ul << 3 | ArithmeticalOperator | UnaryOperator,
		Add						= 1ul << 4 | AdditiveOperator,
		Subtract				= 1ul << 5 | AdditiveOperator,
		/// <summary>The operator belong to the multiplicative group.</summary>
		MultiplicativeOperator  = 1ul << 6 | ArithmeticalOperator,
		Multiply				= 1ul << 7 | MultiplicativeOperator,
		Divide					= 1ul << 8 | MultiplicativeOperator,
		Remainder				= 1ul << 9 | MultiplicativeOperator,
		/// <summary>The operator will return a boolean type.</summary>
		BoolOperator			= 1ul << 10,
		/// <summary>The operator will return a boolean type.</summary>
		LogicalNot				= 1ul << 11 | UnaryOperator | BoolOperator,
		BitwiseNot				= 1ul << 12 | UnaryOperator | ArithmeticalOperator,
		LogicalAnd				= 1ul << 13 | BoolOperator,
		BitwiseAnd				= 1ul << 14 | ArithmeticalOperator,
		LogicalOr				= 1ul << 15 | BoolOperator,
		BitwiseOr				= 1ul << 16 | ArithmeticalOperator,
		/// <summary>The operator belong to the comparison group.</summary>
		СomparisonOperator		= 1ul << 17,
		/// <summary> == or != </summary>
		EqualityOperator		= 1ul << 18 | BoolOperator | СomparisonOperator,
		EqualityTest			= 1ul << 19 | EqualityOperator,
		NotEqualityTest			= 1ul << 20 | EqualityOperator,
		/// <summary> less or more </summary>
		RelationalOperator		= 1ul << 21 | BoolOperator | СomparisonOperator,
		LessTest				= 1ul << 22 | RelationalOperator,
		MoreTest				= 1ul << 23 | RelationalOperator,
		/// <summary>The operator belongs to the group with the highest priority.</summary>
		Primary					= 1ul << 24,
		OpenParenthesis			= 1ul << 25 | Primary,
		CloseParenthesis		= 1ul << 26 | Primary,
		OpenCurlyBrace			= 1ul << 27,
		CloseCurlyBrace			= 1ul << 28,
		OpenSquareBracket		= 1ul << 29 | Primary,
		CloseSquareBracket		= 1ul << 30 | Primary,
		Dot						= 1ul << 31 | Primary,
		Comma					= 1ul << 32,
		SemiColon				= 1ul << 33,
		Colon					= 1ul << 34
	}
}
