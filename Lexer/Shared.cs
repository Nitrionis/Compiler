namespace Shared
{
	public enum Keyword
	{
		Type		= 1 << 1,
		Void		= 1 << 2 | Type,
		Int			= 1 << 3 | Type,
		Float		= 1 << 4 | Type,
		Char		= 1 << 5 | Type,
		String		= 1 << 6 | Type,
		Class		= 1 << 7,
		Logic		= 1 << 8,
		If			= 1 << 9  | Logic,
		For			= 1 << 10 | Logic,
		While		= 1 << 11 | Logic,
		Return		= 1 << 12 | Logic,
		Break		= 1 << 13 | Logic,
		Modifier	= 1 << 14,
		Public		= 1 << 15 | Modifier,
		Static		= 1 << 16 | Modifier,
		New			= 1 << 17,
		Bool		= 1 << 18,
		True		= 1 << 19 | Bool,
		False		= 1 << 20 | Bool,
		Null		= 1 << 21
	}

	public enum Operator
	{
		Assignment				= 1 << 0,
		AdditiveOperator		= 1 << 1,
		UnaryOperator			= 1 << 2,
		Add						= 1 << 3 | AdditiveOperator | UnaryOperator,
		Subtract				= 1 << 4 | AdditiveOperator | UnaryOperator,
		MultiplicativeOperator	= 1 << 5,
		Multiply				= 1 << 6 | MultiplicativeOperator,
		Divide					= 1 << 7 | MultiplicativeOperator,
		Remainder				= 1 << 8 | MultiplicativeOperator,
		LogicalNot				= 1 << 9  | UnaryOperator,
		BitwiseNot				= 1 << 10 | UnaryOperator,
		LogicalAnd				= 1 << 11,
		BitwiseAnd				= 1 << 12,
		LogicalOr				= 1 << 13,
		BitwiseOr				= 1 << 14,
		EqualityOperator		= 1 << 15,
		EqualityTest			= 1 << 16 | EqualityOperator,
		NotEqualityTest			= 1 << 17 | EqualityOperator,
		RelationalOperator		= 1 << 18,
		LessTest				= 1 << 19 | RelationalOperator,
		MoreTest				= 1 << 20 | RelationalOperator,
		Primary					= 1 << 21,
		OpenParenthesis			= 1 << 22 | Primary,
		CloseParenthesis		= 1 << 23 | Primary,
		OpenCurlyBrace			= 1 << 24,
		CloseCurlyBrace			= 1 << 25,
		OpenSquareBracket		= 1 << 26 | Primary,
		CloseSquareBracket		= 1 << 27 | Primary,
		Dot						= 1 << 28 | Primary,
		Comma					= 1 << 29,
		SemiColon				= 1 << 30
	}
}
