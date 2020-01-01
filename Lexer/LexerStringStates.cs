namespace Lexer
{
	public partial class Lexer
	{
		private void BuildStringLevels()
		{
			BuildStringLevel();
		}

		private void BuildStringLevel()
		{
			var constStringActions = actions[(int)State.ConstString];
			SetActionsRange(constStringActions, ActionAddCharToToken);
			constStringActions[0x00 /* \0 */] = ActionErrorSymbol;
			constStringActions[0x22 /* " */] = ActionAddCharToToken;
			constStringActions[0x22 /* " */] += ActionTokenCompleted;
		}
	}
}
