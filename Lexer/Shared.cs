namespace Shared
{
	public enum Keyword
	{
		Type		= 512,
		Void		= 1 | Type,
		Int			= 2 | Type,
		Float		= 3 | Type,
		Char		= 4 | Type,
		String		= 5 | Type,
		Class		= 6,
		Logic		= 1024,
		If			= 7 | Logic,
		For			= 8 | Logic,
		While		= 9 | Logic,
		Return		= 10 | Logic,
		Break		= 11 | Logic,
		Modifier	= 2048,
		Public		= 12 | Modifier,
		Static		= 13 | Modifier,
		New			= 14,
		Bool		= 4096,
		True		= 15 | Bool,
		False		= 16 | Bool,
		Null		= 17
	}

	public enum Operator
	{
		Assignment				= 1,
		AdditiveOperator			= 128,
		UnaryOperator				= 256,
		Add						= 2 | AdditiveOperator | UnaryOperator,
		Subtract				= 3 | AdditiveOperator | UnaryOperator,
		MultiplicativeOperator		= 512,
		Multiply				= 4 | MultiplicativeOperator,
		Divide					= 5 | MultiplicativeOperator,
		Remainder				= 6 | MultiplicativeOperator,
		LogicalNot				= 7 | UnaryOperator,
		BitwiseNot				= 8 | UnaryOperator,
		LogicalAnd				= 9,
		BitwiseAnd				= 10,
		LogicalOr				= 11,
		BitwiseOr				= 12,
		EqualityOperator			= 1024,
		EqualityTest			= 13 | EqualityOperator,
		NotEqualityTest			= 14 | EqualityOperator,
		RelationalOperator			= 2048,
		LessTest				= 15 | RelationalOperator,
		MoreTest				= 16 | RelationalOperator,
		Primary						= 4096,
		OpenParenthesis			= 17 | Primary,
		CloseParenthesis		= 18 | Primary,
		OpenCurlyBrace			= 19,
		CloseCurlyBrace			= 20,
		OpenSquareBracket		= 21 | Primary,
		CloseSquareBracket		= 22 | Primary,
		Dot						= 23 | Primary,
		Comma					= 24,
		SemiColon				= 25
	}
}
