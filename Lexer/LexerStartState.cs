using Shared;

namespace Lexer
{
	public partial class Lexer
	{
		private void BuildStartLevel()
		{
			void ActionSetWordState() => SetState(State.Word, Token.Types.Identifier, updateLocation: true);
			void ActionSetIntState() => SetState(State.Int, Token.Types.Int, updateLocation: true);

			var startActions = actions[(int)State.Start];
			startActions[0x21 /* ! */] = () => SetState(State.NotEquals, Token.Types.Operator, updateLocation: true);
			startActions[0x22 /* " */] = () => SetState(State.ConstString, Token.Types.String, updateLocation: true);
			SetActionsRange(startActions, ActionErrorSymbol, 0x23, 2); // # $
			startActions[0x25 /* % */] = () => ActionOneSymbolOperator(Operator.Remainder);
			startActions[0x26 /* & */] = () => SetState(State.Ampersand, Token.Types.Operator, updateLocation: true);
			startActions[0x27 /* ' */] = () => SetState(State.Char, Token.Types.Char, updateLocation: true);
			startActions[0x28 /* ( */] = () => ActionOneSymbolOperator(Operator.OpenParenthesis);
			startActions[0x29 /* ) */] = () => ActionOneSymbolOperator(Operator.CloseParenthesis);
			startActions[0x2a /* * */] = () => ActionOneSymbolOperator(Operator.Multiply);
			startActions[0x2b /* + */] = () => ActionOneSymbolOperator(Operator.Add);
			startActions[0x2c /* , */] = () => ActionOneSymbolOperator(Operator.Comma);
			startActions[0x2d /* - */] = () => ActionOneSymbolOperator(Operator.Subtract);
			startActions[0x2e /* . */] = () => ActionOneSymbolOperator(Operator.Dot);
			startActions[0x2f /* / */] = () => SetState(State.Division, Token.Types.Operator, updateLocation: true);
			SetActionsRange(startActions, ActionSetIntState, 0x30, 10); // 0 1 2 ...
			startActions[0x3a /* : */] = ActionErrorSymbol;
			startActions[0x3b /* ; */] = () => ActionOneSymbolOperator(Operator.SemiColon);
			startActions[0x3c /* < */] = () => ActionOneSymbolOperator(Operator.LessTest);
			startActions[0x3d /* = */] = () => SetState(State.Equals, Token.Types.Operator, updateLocation: true);
			startActions[0x3e /* > */] = () => ActionOneSymbolOperator(Operator.MoreTest);
			SetActionsRange(startActions, ActionErrorSymbol, 0x3f, 2); // ? @
			SetActionsRange(startActions, ActionSetWordState, 0x41, 26); // A B C ...
			startActions[0x5b /* [ */] = () => ActionOneSymbolOperator(Operator.OpenSquareBracket);
			startActions[0x5c /* \ */] = ActionErrorSymbol;
			startActions[0x5d /* ] */] = () => ActionOneSymbolOperator(Operator.CloseSquareBracket);
			SetActionsRange(startActions, ActionErrorSymbol, 0x5e, 3); // ^ _ `
			SetActionsRange(startActions, ActionSetWordState, 0x61, 26); // a b c ...
			startActions[0x7b /* { */] = () => ActionOneSymbolOperator(Operator.OpenCurlyBrace);
			startActions[0x7c /* | */] = () => SetState(State.Pipe, Token.Types.Operator, updateLocation: true);
			startActions[0x7d /* } */] = () => ActionOneSymbolOperator(Operator.CloseCurlyBrace);
			startActions[0x7e /* } */] = () => ActionOneSymbolOperator(Operator.BitwiseNot);
		}
	}
}
