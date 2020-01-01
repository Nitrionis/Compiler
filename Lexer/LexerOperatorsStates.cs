using System;
using Shared;

namespace Lexer
{
    public partial class Lexer
    {
		private void BuildOtherOperatorsLevels()
		{
			BuildDivisionLevel();
			BuildDoubleOperatorLevels();
		}

		private void BuildDivisionLevel()
		{
			var divisionActions = actions[(int)State.Division];
			var action = new Action(ActionNoRequestNextSymbol);
			action += () => {
				token.Type = Token.Types.Operator;
				token.Value = Operator.Divide;
				ActionTokenCompleted();
			};
			SetActionsRange(divisionActions, action);
			divisionActions[0x2f /* / */] = () => SetState(State.Comment, Token.Types.Undefined, updateLocation: false);
		}

		private void BuildDoubleOperatorLevels()
		{
			void BuildDoubleOperatorLevel(State level, int secondChar, Operator op1, Operator op2)
			{
				var operatorActions = actions[(int)level];
				var action = new Action(ActionTokenCompleted);
				action += ActionNoRequestNextSymbol;
				action += () => { token.Value = op1; };
				SetActionsRange(operatorActions, action);
				operatorActions[secondChar] = ActionAddCharToToken;
				operatorActions[secondChar] += ActionTokenCompleted;
				operatorActions[secondChar] += () => { token.Value = op2; };
			}
			BuildDoubleOperatorLevel(State.Ampersand, secondChar: 0x26 /* & */, Operator.BitwiseAnd, Operator.LogicalAnd); // &&
			BuildDoubleOperatorLevel(State.Pipe, secondChar: 0x7c /* | */, Operator.BitwiseOr, Operator.LogicalOr); // ||
			BuildDoubleOperatorLevel(State.Equals, secondChar: 0x3d /* = */, Operator.Assignment, Operator.EqualityTest); // ==
			BuildDoubleOperatorLevel(State.NotEquals, secondChar: 0x3d /* = */, Operator.LogicalNot, Operator.NotEqualityTest); // !=
		}
	}
}
