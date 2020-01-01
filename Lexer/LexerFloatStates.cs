using System;

namespace Lexer
{
	public partial class Lexer
	{
		private void BuildFloatLevels()
		{
			BuildFloatLevel();
			BuildFloatExponentLevel();
			BuildFloatExponentSignLevel();
		}

		private void BuildFloatLevel()
		{
			void CheckExponent()
			{
				var str = token.RawValue;
				if (str[str.Length - 1] != '.') {
					SetState(State.FloatExp, Token.Types.Float, updateLocation: false);
				} else {
					ActionErrorSymbol();
				}
			}
			Array.Copy(actions[(int)State.Int], actions[(int)State.Float], alphabetSize);
			var floatActions = actions[(int)State.Float];
			floatActions[0x2e /* . */] = ActionErrorSymbol;
			floatActions[0x58 /* X */] = ActionErrorSymbol;
			floatActions[0x78 /* x */] = ActionErrorSymbol;
			floatActions[0x65 /* e */] = CheckExponent;
			floatActions[0x45 /* E */] = CheckExponent;
		}

		private void BuildFloatExponentLevel()
		{
			void CheckSign()
			{
				var str = token.RawValue;
				if (str[str.Length - 1] == 'e' || str[str.Length - 1] == 'E') {
					SetState(State.FloatExpSign, Token.Types.Float, updateLocation: false);
				} else {
					ActionErrorSymbol();
				}
			}
			Array.Copy(actions[(int)State.Float], actions[(int)State.FloatExp], alphabetSize);
			var floatActions = actions[(int)State.FloatExp];
			floatActions[0x65 /* E */] = ActionErrorSymbol;
			floatActions[0x45 /* e */] = ActionErrorSymbol;
			floatActions[0x2d /* - */] = CheckSign;
			floatActions[0x2b /* + */] = CheckSign;
		}

		private void BuildFloatExponentSignLevel()
		{
			Array.Copy(actions[(int)State.FloatExp], actions[(int)State.FloatExpSign], alphabetSize);
			var floatActions = actions[(int)State.FloatExpSign];
			floatActions[0x2d /* - */] = ActionErrorSymbol;
			floatActions[0x2b /* + */] = ActionErrorSymbol;
		}
	}
}
