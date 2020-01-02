using System;
using Shared;

namespace Lexer
{
	public partial class Lexer
    {
		private void SetActionsRange(Action[] actions, Action action, uint startIndex = 0, uint size = uint.MaxValue)
		{
			if (size == uint.MaxValue) {
				size = (uint)actions.Length;
			}
			for (uint i = startIndex, end = startIndex + size; i < end; i++) {
				actions[i] = action;
			}
		}

		private void ActionSkip() { }

		private void ActionClear() => token.RawValue = "";

		private void ActionOneSymbolOperator(Operator op)
		{
			token.Type = Token.Types.Operator;
			token.Value = op;
			token.RawValue = ((char)stream.Symbol).ToString();
			UpdateTokenLocation();
			ActionTokenCompleted();
		}

		private void ActionSetStartState() => activeState = State.Start;

		/// <summary>
		/// Also add char to token.
		/// </summary>
		private void SetState(State state, Token.Types type, bool updateLocation)
		{
			activeState = state;
			token.Type = type;
			ActionAddCharToToken();
			if (updateLocation) {
				UpdateTokenLocation();
			}
		}

		private void ActionErrorSymbol()
		{
			isError = true;
			tokenCompleted = true;
			ActionAddCharToToken();
		}

		private void ActionNoRequestNextSymbol() => stream.NeedNextSymbol = false;

		private void ActionAddCharToToken() => token.RawValue += (char)stream.Symbol;

		private void ActionTokenCompleted()
		{
			tokenCompleted = true;
			ActionSetStartState();
		}

		private void UpdateTokenLocation()
		{
			token.RowIndex = stream.RowIndex;
			token.ColIndex = stream.ColIndex;
		}
	}
}
