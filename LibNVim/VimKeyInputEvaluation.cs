using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;
using System.Diagnostics;

namespace LibNVim
{
    public class VimKeyInputEvaluation
    {
        /// <summary>
        /// key input evaluation state
        /// </summary>
        public enum KeyEvalState
        {
            None = 0, Error, InProcess, Success, Escape,
        }

        /// <summary>
        /// some commands like 'G', have different behaviours between default repeat number and user given number
        /// </summary>
        private class RepeatNumberStore
        {
            private int _value = 1;
            private bool _isDefault = true;

            public int Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    _value = value;
                    _isDefault = false;
                }
            }
            public bool IsDefault
            {
                get { return _isDefault; } 
            }

            public RepeatNumberStore()
            {
            }
        }

        /// <summary>
        /// DFA state on key input
        /// </summary>
        private enum ActionState
        {
            None = 0, 
            RepeatNumber, // outmost repeat number, like "5j"
            SimpleMotion, // simple motions, like 'j'
            SimpleEdition, // simple editions, like 'i'
            RangeEdition, // range editions, like "cc"
            RepeatNumberInRangeEdition, // repeat number in RangeEdition, like "d5j"
            SearchCharInLine, // char search in the line, like 'f'
            Motion_gg, // "gg"
            MotionOfLeftBracket, // "[{", no plans for "[["
            MotionOfRightBracket, // "]}", no plans for "]]"
            Motion_zz, // "zz"
        }

        // currently only deal with default register yank/paste
        const string Default_Register = "";

        /// <summary>
        /// all simple motions plan to support
        /// </summary>
        private static char[] Simple_Motion_Chars = { 'h', 'H', 'j', 'k', 'l', 'L', '0', '^', '$', 'G',
                                                        'w', 'W', 'e', 'E', 'b', 'B', '%', '*' };
        /// <summary>
        /// all simple editions, and 'u' undo/'U' redo, 'p'/'P' for paste
        /// </summary>
        private static char[] Simple_Edition_Chars = { '.', 'u', 'U', 'x', 'X', 'i', 'I', 'a', 'A', 's', 'S', 'o', 
                                                         'O', 'p', 'P', 'C', 'D', 'J' };
        // all range editions, and 'y' for yank
        private static char[] Range_Edition_Chars = { 'c', 'd', 'y', '=' };
        private static char[] Search_Char_In_line_Chars = { 'f', 'F', 't', 'T' };
        private static char THE_g = 'g';
        private static char Left_Bracket = '[';
        private static char Left_Brace = '{';
        private static char Right_Bracket = ']';
        private static char Right_Brace = '}';
        private static char THE_z = 'z';

        private IVimHost _host = null;

        private ActionState _actionState = ActionState.None;

        private RepeatNumberStore _repeatNumber = new RepeatNumberStore();
        private RepeatNumberStore _repeatNumberInRangeEdition = new RepeatNumberStore();
        private char _firstRangeEditionChar = '\0';
        private char _inLineSearchChar = '\0';

        /// <summary>
        /// backup the state of RangeEdition,
        /// because _actionType may be flushed by state such as ActionType.RepeatNumberInRangeEdition
        /// </summary>
        private bool _inRangeEdition = false;

        public VimKeyInputEvaluation(IVimHost host)
        {
            _host = host;
        }

        private IVimMotion GetSimpleMotion(char keyInput, RepeatNumberStore repeat)
        {
            Debug.Assert(Simple_Motion_Chars.Contains(keyInput));

            switch (keyInput) {
                case 'H':
                    return new Motions.MotionMoveToStartOfDocument(_host, repeat.Value);
                case 'h':
                    return new Motions.MotionCaretLeft(_host, repeat.Value);
                case 'j':
                    return new Motions.MotionCaretDown(_host, repeat.Value);
                case 'k':
                    return new Motions.MotionCaretUp(_host, repeat.Value);
                case 'l':
                    return new Motions.MotionCaretRight(_host, repeat.Value);
                case 'L':
                    return new Motions.MotionMoveToEndOfDocument(_host, repeat.Value);
                case '0':
                    return new Motions.MotionMoveToStartOfLine(_host, repeat.Value);
                case '^':
                    return new Motions.MotionMoveToStartOfLineText(_host, repeat.Value);
                case '$':
                    return new Motions.MotionMoveToEndOfLine(_host, repeat.Value);
                case 'G':
                    if (repeat.IsDefault) {
                        return new Motions.MotionMoveToEndOfDocument(_host, 1);
                    }
                    else {
                        return new Motions.MotionGotoLine(_host, repeat.Value);
                    }
                case 'w':
                    return new Motions.MotionMoveToNextWord(_host, repeat.Value);
                case 'W':
                    return new Motions.MotionMoveToNextWord(_host, repeat.Value);
                case 'e':
                    return new Motions.MotionMoveToEndOfWord(_host, repeat.Value);
                case 'E':
                    return new Motions.MotionMoveToEndOfWord(_host, repeat.Value);
                case 'b':
                    return new Motions.MotionMoveToPreviousWord(_host, repeat.Value);
                case 'B':
                    return new Motions.MotionMoveToPreviousWord(_host, repeat.Value);
                case '%':
                    return new Motions.MotionGoToMatch(_host, repeat.Value);
                case '*':
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            Debug.Assert(false);
            return null;
        }

        private IVimEdititon GetSimpleEdition(char keyInput, int repeat)
        {
            Debug.Assert(Simple_Edition_Chars.Contains(keyInput));

            switch (keyInput) {
                case '.':
                    return new Editions.EditionDot(_host, repeat);
                case 'u':
                    return new Editions.EditionUndo(_host, repeat);
                case 'U':
                    return new Editions.EditionRedo(_host, repeat);
                case 'x':
                    return new Editions.EditionDeleteChar(_host, repeat);
                case 'X':
                    return new Editions.EditionBackspace(_host, repeat);
                case 'i':
                    return new Editions.EditionInsert(_host, repeat);
                case 'I':
                    return new Editions.EditionInsertToLineStart(_host, repeat);
                case 'a':
                    return new Editions.EditionAppend(_host, repeat);
                case 'A':
                    return new Editions.EditionAppendToLineEnd(_host, repeat);
                case 's':
                    return new Editions.EditionChangeChar(_host, repeat);
                case 'S':
                    return new Editions.EditionChangeLine(_host, repeat);
                case 'o':
                    return new Editions.EditionOpenLineBlow(_host, repeat);
                case 'O':
                    return new Editions.EditionOpenLineAbove(_host, repeat);
                case 'p':
                    return new Editions.EditionYankPaste(_host.DefaultRegister, _host, repeat);
                case 'P':
                    return new Editions.EditionYankPasteBefore(_host.DefaultRegister, _host, repeat);
                case 'C':
                    return new Editions.EditionChangeRange(_host, 1, new Motions.MotionMoveToEndOfLine(_host, 1));
                case 'D':
                    return new Editions.EditionDeleteRange(_host, 1, new Motions.MotionMoveToEndOfLine(_host, 1));
                case 'J':
                    return new Editions.EditionJoinLine(_host, repeat);
                default:
                    Debug.Assert(false);
                    break;
            }

            Debug.Assert(false);
            return null;
        }

        /// <summary>
        /// "cc" "dd", "yy", "=="
        /// </summary>
        /// <param name="keyInput"></param>
        /// <param name="repeat"></param>
        /// <returns></returns>
        private IVimEdititon GetDoubleRangeEdition(char keyInput, int repeat)
        {
            switch (keyInput) {
                case 'c':
                    return new Editions.EditionChangeLine(_host, repeat);
                case 'd':
                    return new Editions.EditionDeleteLine(_host, repeat);
                case 'y':
                    return new Editions.EditionYankLine(_host.DefaultRegister, _host, repeat);
                case '=':
                    return new Editions.EditionFormatLine(_host, repeat);
                default:
                    Debug.Assert(false);
                    break;
            }

            Debug.Assert(false);
            return null;
        }

        private IVimEdititon GetRangeEdition(char keyInput, IVimMotion motion, int repeat)
        {
            switch (keyInput) {
                case 'c':
                    return new Editions.EditionChangeRange(_host, repeat, motion);
                case 'd':
                    return new Editions.EditionDeleteRange(_host, repeat, motion);
                case 'y':
                    return new Editions.EditionYankRange(_host.DefaultRegister, motion, _host, repeat);
                case '=':
                    return new Editions.EditionFormatRange(_host, repeat, motion);
                default:
                    Debug.Assert(false);
                    break;
            }

            Debug.Assert(false);
            return null;
        }

        private IVimMotion GetSearchCharInLineMotion(char searchCommand, char toSearch, int repeat)
        {
            switch (searchCommand) {
                case 'f':
                    return new Motions.MotionGotoCharSearch(toSearch, _host, repeat);
                case 'F':
                    return new Motions.MotionGotoCharSearchBack(toSearch, _host, repeat);
                case 't':
                    return new Motions.MotionGotoBeforeCharSearch(toSearch, _host, repeat);
                case 'T':
                    return new Motions.MotionGotoBeforeCharSearchBack(toSearch, _host, repeat);
                default:
                    Debug.Assert(false);
                    break;
            }
            return null;
        }

        public KeyEvalState Evaluate(VimKeyInput keyInput, out IVimAction action)
        {
            // DFA to interpret keyInput

            action = null;

            if (keyInput.Value.Length > 1) {
                // the great "Esc", having power to quit immediately
                if (string.Compare(keyInput.Value, VimKeyInput.Escape) == 0) {
                    return KeyEvalState.Escape;
                }
                else {
                    return KeyEvalState.Error;
                }
            }

            Debug.Assert(keyInput.Value.Length == 1);
            char key_input = keyInput.Value[0];

            int num = 0;
            if (int.TryParse(keyInput.Value, out num)) {
                if (_actionState == ActionState.None) {
                    if (num == 0) {
                        action = new Motions.MotionMoveToStartOfLine(_host, 1);
                        return KeyEvalState.Success;
                    }
                    else {
                        _actionState = ActionState.RepeatNumber;
                        _repeatNumber.Value = num;
                    }
                }
                else if (_actionState == ActionState.RepeatNumber) {
                    _repeatNumber.Value = (_repeatNumber.Value * 10) + num;
                }
                else if (_actionState == ActionState.RangeEdition) {
                    if (num == 0) {
                        IVimMotion motion = new Motions.MotionMoveToStartOfLine(_host, 1);
                        action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                        return KeyEvalState.Success;
                    }
                    else {
                        _actionState = ActionState.RepeatNumberInRangeEdition;
                        _repeatNumberInRangeEdition.Value = num;
                    }
                }
                else if (_actionState == ActionState.RepeatNumberInRangeEdition) {
                    _repeatNumberInRangeEdition.Value = (_repeatNumberInRangeEdition.Value * 10) + num;
                }
                else if (_actionState == ActionState.SearchCharInLine) {
                    if (!_inRangeEdition) {
                        action = this.GetSearchCharInLineMotion(_inLineSearchChar, key_input, _repeatNumber.Value);
                        return KeyEvalState.Success;
                    }
                    else {
                        IVimMotion motion = this.GetSearchCharInLineMotion(_inLineSearchChar, key_input, 
                            _repeatNumberInRangeEdition.Value);
                        action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                        return KeyEvalState.Success;
                    }
                }
                else {
                    return KeyEvalState.Error;
                }

                return KeyEvalState.InProcess;
            }

            if (!_inRangeEdition) {
                if ((_actionState == ActionState.None) || (_actionState == ActionState.RepeatNumber)) {
                    if (Simple_Motion_Chars.Contains(key_input)) {
                        _actionState = ActionState.SimpleMotion;
                        action = this.GetSimpleMotion(key_input, _repeatNumber);
                        return KeyEvalState.Success;
                    }
                    else if (Simple_Edition_Chars.Contains(key_input)) {
                        _actionState = ActionState.SimpleEdition;
                        action = this.GetSimpleEdition(key_input, _repeatNumber.Value);
                        return KeyEvalState.Success;
                    }
                    else if (Range_Edition_Chars.Contains(key_input)) {
                        _actionState = ActionState.RangeEdition;
                        _firstRangeEditionChar = key_input;
                        _inRangeEdition = true;
                        return KeyEvalState.InProcess;
                    }
                    else if (Search_Char_In_line_Chars.Contains(key_input)) {
                        _actionState = ActionState.SearchCharInLine;
                        _inLineSearchChar = key_input;
                        return KeyEvalState.InProcess;
                    }
                    else if (key_input == THE_g) {
                        _actionState = ActionState.Motion_gg;
                        return KeyEvalState.InProcess;
                    }
                    else if (key_input == Left_Bracket) {
                        _actionState = ActionState.MotionOfLeftBracket;
                        return KeyEvalState.InProcess;
                    }
                    else if (key_input == Right_Bracket) {
                        _actionState = ActionState.MotionOfRightBracket;
                        return KeyEvalState.InProcess;
                    }
                    else if (key_input == THE_z) {
                        _actionState = ActionState.Motion_zz;
                        return KeyEvalState.InProcess;
                    }
                    else {
                        return KeyEvalState.Error;
                    }
                }
                else if (_actionState == ActionState.SearchCharInLine) {
                    action = this.GetSearchCharInLineMotion(_inLineSearchChar, key_input, _repeatNumber.Value);
                    return KeyEvalState.Success;
                }
                else if (_actionState == ActionState.Motion_gg) {
                    if (key_input == THE_g) {
                        action = new Motions.MotionGotoLine(_host, _repeatNumber.Value);
                        return KeyEvalState.Success;
                    }
                    else {
                        return KeyEvalState.Error;
                    }
                }
                else if (_actionState == ActionState.MotionOfLeftBracket) {
                    if (key_input == Left_Brace) {
                        action = new Motions.MotionGotoPreviousBrace(_host, _repeatNumber.Value);
                        return KeyEvalState.Success;
                    }
                    else {
                        return KeyEvalState.Error;
                    }
                }
                else if (_actionState == ActionState.MotionOfRightBracket) {
                    if (key_input == Right_Brace) {
                        action = new Motions.MotionGotoNextBrace(_host, _repeatNumber.Value);
                        return KeyEvalState.Success;
                    }
                    else {
                        return KeyEvalState.Error;
                    }
                }
                else if (_actionState == ActionState.Motion_zz) {
                    if (key_input == THE_z) {
                        if (_repeatNumber.IsDefault) {
                            int current_line_number = _host.CurrentPosition.X + 1;
                            action = new Motions.MotionScrollLineCenter(_host, current_line_number);
                        }
                        else {
                            action = new Motions.MotionScrollLineCenter(_host, _repeatNumber.Value);
                        }
                        return KeyEvalState.Success;
                    }
                    else {
                        return KeyEvalState.Error;
                    }
                }
                else {
                    return KeyEvalState.Error;
                }
            }

            Debug.Assert(_inRangeEdition);

            if ((_actionState == ActionState.RangeEdition) ||
                (_actionState == ActionState.RepeatNumberInRangeEdition)) {
                if (Simple_Motion_Chars.Contains(key_input)) {
                    _actionState = ActionState.SimpleMotion;
                    IVimMotion motion = this.GetSimpleMotion(key_input, _repeatNumberInRangeEdition);
                    action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                    return KeyEvalState.Success;
                }
                else if (Range_Edition_Chars.Contains(key_input)) {
                    if (key_input != _firstRangeEditionChar) {
                        return KeyEvalState.Error;
                    }

                    action = this.GetDoubleRangeEdition(key_input, _repeatNumber.Value * _repeatNumberInRangeEdition.Value);
                    return KeyEvalState.Success;
                }
                else if (Search_Char_In_line_Chars.Contains(key_input)) {
                    _actionState = ActionState.SearchCharInLine;
                    _inLineSearchChar = key_input;
                    return KeyEvalState.InProcess;
                }
                else if (key_input == THE_g) {
                    _actionState = ActionState.Motion_gg;
                    return KeyEvalState.InProcess;
                }
                else if (key_input == Left_Bracket) {
                    _actionState = ActionState.MotionOfLeftBracket;
                    return KeyEvalState.InProcess;
                }
                else if (key_input == Right_Bracket) {
                    _actionState = ActionState.MotionOfRightBracket;
                    return KeyEvalState.InProcess;
                }
                else {
                    return KeyEvalState.Error;
                }
            }
            else if (_actionState == ActionState.SearchCharInLine) {
                IVimMotion motion = this.GetSearchCharInLineMotion(_inLineSearchChar, key_input, 
                    _repeatNumberInRangeEdition.Value);
                action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                return KeyEvalState.Success;
            }
            else if (_actionState == ActionState.Motion_gg) {
                if (key_input == THE_g) {
                    IVimMotion motion = new Motions.MotionMoveToStartOfDocument(_host, _repeatNumberInRangeEdition.Value);
                    action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                    return KeyEvalState.Success;
                }
                else {
                    return KeyEvalState.Error;
                }
            }
            else if (_actionState == ActionState.MotionOfLeftBracket) {
                if (key_input == Left_Brace) {
                    IVimMotion motion = new Motions.MotionGotoPreviousBrace(_host, _repeatNumberInRangeEdition.Value);
                    action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                    return KeyEvalState.Success;
                }
                else {
                    return KeyEvalState.Error;
                }
            }
            else if (_actionState == ActionState.MotionOfRightBracket) {
                if (key_input == Right_Brace) {
                    IVimMotion motion = new Motions.MotionGotoNextBrace(_host, _repeatNumberInRangeEdition.Value);
                    action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                    return KeyEvalState.Success;
                }
                else {
                    return KeyEvalState.Error;
                }
            }
            else {
                return KeyEvalState.Error;
            }
        }
    }
}
