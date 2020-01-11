using System;

namespace Lexer
{
	public partial class Lexer
	{
		private void BuildIntLevels()
		{
			BuildIntLevel();
			BuildInt0xLevel();
		}

		private void BuildIntLevel()
		{
			var intActions = actions[(int)State.Int];

			void CheckIntHexState()
			{
				if (token.RawValue.Length == 1 && token.RawValue[0] == '0') {
					SetState(State.Int0X, Token.Types.Int, updateLocation: false);
				}
			}

			void ActionCompleted()
			{
				ActionNoRequestNextSymbol();
				ActionTokenCompleted();
			}

			SetActionsRange(intActions, ActionCompleted, 0x00, (uint)alphabetSize);

			SetActionsRange(intActions, ActionAddCharToToken, 0x30, 10); // 0 1 2 ...
			SetActionsRange(intActions, ActionErrorSymbol, 0x41, 26); // A B C ...
			SetActionsRange(intActions, ActionErrorSymbol, 0x61, 26); // a b c ...
			SetActionsRange(intActions, ActionErrorSymbol, 0x21, 4); // ! " # $
			SetActionsRange(intActions, ActionErrorSymbol, 0x27, 2); // ' (
			//intActions[0x2c /* , */] = ActionErrorSymbol;
			intActions[0x2e /* . */] = () => SetState(State.Float, Token.Types.Float, updateLocation: false);
			intActions[0x3a /* : */] = ActionErrorSymbol;
			SetActionsRange(intActions, ActionErrorSymbol, 0x3e, 3); // > ? @
			intActions[0x58 /* X */] = CheckIntHexState;
			SetActionsRange(intActions, ActionErrorSymbol, 0x5b, 2); // [ \
			SetActionsRange(intActions, ActionErrorSymbol, 0x5e, 3); // ^ _ `
			intActions[0x78 /* x */] = CheckIntHexState;
			intActions[0x7b /* { */] = ActionErrorSymbol;
			intActions[0x7e /* ~ */] = ActionErrorSymbol;
		}

		private void BuildInt0xLevel()
		{
			Array.Copy(actions[(int)State.Int], actions[(int)State.Int0X], alphabetSize);
			var intActions = actions[(int)State.Int0X];
			intActions[0x2e /* . */] = ActionErrorSymbol;
			intActions[0x58 /* X */] = ActionErrorSymbol;
			intActions[0x78 /* x */] = ActionErrorSymbol;
			SetActionsRange(intActions, ActionAddCharToToken, 0x41, 6); // A B C D E F
			SetActionsRange(intActions, ActionAddCharToToken, 0x61, 6); // a b c d e f
		}
	}
}
