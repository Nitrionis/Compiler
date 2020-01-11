using System;

namespace Lexer
{
	public class Token
	{
		[Flags]
		public enum Types
		{
			Undefined	= 1 << 1,
			Identifier	= 1 << 2,
			Keyword		= 1 << 3,
			Operator	= 1 << 4,
			Literal		= 1 << 5,
			String		= 1 << 6 | Literal,
			Int			= 1 << 7 | Literal,
			Float		= 1 << 8 | Literal,
			Char		= 1 << 9 | Literal,
			Eof			= 1 << 10
		}

		public Types Type = Types.Undefined;
		public object Value;
		public string RawValue = "";
		public int RowIndex = -1;
		public int ColIndex = -1;
		public string Message = null;

		public Token() { }

		public Token(Types type, object value, string rawValue, int rowIndex, int colIndex)
		{
			Type = type;
			Value = value;
			RawValue = rawValue;
			RowIndex = rowIndex;
			ColIndex = colIndex;
		}

		public bool IsError => Value == null;

		public override string ToString() => string.Format(
			"r:{0,3} c:{1,3} {2,16} {3,-16} raw {4}",
			RowIndex, ColIndex, !IsError ? Type.ToString() : "Error-" + Type.ToString(), Value, RawValue);

		public static bool Equals(Token t1, Token t2) =>
			(t1 == null && t2 == null) ||
			(t1.Type == t2.Type &&
			object.Equals(t1.Value, t2.Value) &&
			t1.RawValue == t2.RawValue &&
			t1.RowIndex == t2.RowIndex &&
			t1.ColIndex == t2.ColIndex &&
			object.Equals(t1.Message, t2.Message));
	}
}
