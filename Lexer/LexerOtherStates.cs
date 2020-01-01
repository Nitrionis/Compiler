namespace Lexer
{
	public partial class Lexer
	{
		private void BuildOtherLevels()
		{
			BuildCharLevel();
			BuildCommentLevel();
			BuildWordLevel();
		}

		private void BuildCharLevel()
		{
			var quotationMarkActions = actions[(int)State.Char];
			SetActionsRange(quotationMarkActions, () => {
				ActionAddCharToToken();
				if (token.RawValue.Length > 2) {
					isError = true;
					tokenCompleted = true;
				}
			});
			quotationMarkActions[0x00 /* \0 */] = ActionErrorSymbol;
			quotationMarkActions[0x27 /* ' */] = ActionAddCharToToken;
			quotationMarkActions[0x27 /* ' */] += ActionTokenCompleted;
		}

		private void BuildCommentLevel()
		{
			var commentActions = actions[(int)State.Comment];
			SetActionsRange(commentActions, ActionAddCharToToken);
			commentActions[0x0a /* \n */] = ActionSetStartState;
			commentActions[0x0a /* \n */] += ActionClear;
		}

		private void BuildWordLevel()
		{
			var wordActions = actions[(int)State.Word];
			for (int i = 0; i < wordActions.Length; i++) {
				if (IsDigit(i) || IsLatin(i)) {
					wordActions[i] = ActionAddCharToToken;
				} else {
					wordActions[i] = ActionNoRequestNextSymbol;
					wordActions[i] += ActionTokenCompleted;
				}
			}
		}
	}
}
