namespace Lexer
{
	public partial class Lexer
	{
		private void BuildStringLevels()
		{
			BuildStringLevel();
			BuildStringLineFeedLevel();
		}

		private void BuildStringLevel()
		{
			var constStringActions = actions[(int)State.ConstString];
			SetActionsRange(constStringActions, ActionAddCharToToken);
			constStringActions[0x00 /* \0 */] = ActionErrorSymbol;
			constStringActions[0x22 /* " */] = ActionAddCharToToken;
			constStringActions[0x22 /* " */] += ActionTokenCompleted;
		}

		private void BuildStringLineFeedLevel()
		{
			var escapeSequenceActions = actions[(int)State.EscapeSequence];
			SetActionsRange(escapeSequenceActions, () => 
				SetState(State.ConstString, Token.Types.String, updateLocation: false));
			escapeSequenceActions[0x6e /* n */] += () => {
				var str = token.RawValue;
				str.Remove(str.Length - 2, 2);
				token.RawValue = str + "\n";
			};
			escapeSequenceActions[0x00 /* \0 */] = ActionErrorSymbol;
			escapeSequenceActions[0x22 /* " */] = ActionAddCharToToken;
			escapeSequenceActions[0x22 /* " */] += ActionTokenCompleted;
		}
	}
}
