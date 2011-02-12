using System;
using System.Collections.Generic;

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
            RangeYank, // yank edition, 'y'
            RepeatNumberInRangeYank, // repeat number in RangeYank, like "y5w"
            DoubleQuoteYankBegin, // symbol register yank after meet #"#
            DoubleQuoteYankSymbolSpecified, // after register symbol specified, such as #"a#
            RepeatNumberInDoubleQuoteYank, // repeat number like #"a2yw#
            SearchCharInLine, // char search in the line, like 'f'
            SearchWord, // search word in the file
            Motion_gg, // "gg"
            MotionOfLeftBracket, // "[{", no plans for "[["
            MotionOfRightBracket, // "]}", no plans for "]]"
            Motion_zz, // "zz"
        }

        public readonly static string Status_Char_Backspace = "\b";

        /// <summary>
        /// all simple motions plan to support
        /// </summary>
        private static List<char> Simple_Motion_Chars = new List<char>() { 'h', 'H', 'j', 'k', 'l', 'L', '0', '^', '$', 'G',
                                                        'w', 'W', 'e', 'E', 'b', 'B', '%', '*', '#', 'n', 'N' };
        /// <summary>
        /// all simple editions, and 'u' undo/'U' redo, 'p'/'P' for paste
        /// </summary>
        private static List<char> Simple_Edition_Chars = new List<char>() { '.', 'u', 'U', 'x', 'X', 'i', 'I', 'a', 'A', 's', 'S', 'o', 
                                                         'O', 'p', 'P', 'C', 'D', 'J' };
        // all range editions
        private static List<char> Range_Edition_Chars = new List<char>() { 'c', 'd', '=' };
        private static List<char> Search_Char_In_line_Chars = new List<char>() { 'f', 'F', 't', 'T' };
        private const char Double_Quote = '"';
        private const char Back_Slash = '/';
        // duplicated in Range_Edition_Chars
        private const char THE_y = 'y';
        private const char THE_Lowercase_p = 'p';
        private const char THE_Uppercase_P = 'P';
        private const char THE_g = 'g';
        private const char Left_Bracket = '[';
        private const char Left_Brace = '{';
        private const char Right_Bracket = ']';
        private const char Right_Brace = '}';
        private const char THE_z = 'z';

        private IVimHost _host = null;

        private ActionState _actionState = ActionState.None;

        private VimRegister _yankRegister = VimRegister.DefaultRegister;
        private RepeatNumberStore _repeatNumber = new RepeatNumberStore();
        private RepeatNumberStore _repeatNumberInRangeEdition = new RepeatNumberStore();
        private RepeatNumberStore _repeatNumberInRangeYank = new RepeatNumberStore();
        private RepeatNumberStore _repeatNumberInDoubleQuoteYank = new RepeatNumberStore();
        private char _firstRangeEditionChar = '\0';
        private char _inLineSearchChar = '\0';
        private string _wordToSearch = "";

        /// <summary>
        /// backup the state of RangeEdition,
        /// because _actionType may be flushed by state such as ActionType.RepeatNumberInRangeEdition
        private ActionState _rangeState = ActionState.None;

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
                    return new Motions.MotionGotoWordStar(_host, repeat.Value);
                case '#':
                    return new Motions.MotionGotoWordSharp(_host, repeat.Value);
                case 'n':
                    return new Motions.MotionGotoWordFindNext(_host, repeat.Value);
                case 'N':
                    return new Motions.MotionGotoWordFindPrevious(_host, repeat.Value);
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
                    return new Editions.EditionYankPaste(VimRegister.DefaultRegister, _host, repeat);
                case 'P':
                    return new Editions.EditionYankPasteBefore(VimRegister.DefaultRegister, _host, repeat);
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
                    return new Editions.EditionYankLine(VimRegister.DefaultRegister, _host, repeat);
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
                    return new Editions.EditionYankRange(VimRegister.DefaultRegister, motion, _host, repeat);
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
                    return new Motions.MotionGotoCharFindNext(toSearch, _host, repeat);
                case 'F':
                    return new Motions.MotionGotoCharFindPrevious(toSearch, _host, repeat);
                case 't':
                    return new Motions.MotionGotoBeforeCharFindNext(toSearch, _host, repeat);
                case 'T':
                    return new Motions.MotionGotoBeforeCharFindPrevious(toSearch, _host, repeat);
                default:
                    Debug.Assert(false);
                    break;
            }
            return null;
        }

        /// <summary>
        /// the output of statusChar controls the "Status Text" to display
        /// </summary>
        /// <param name="keyInput"></param>
        /// <param name="action"></param>
        /// <param name="statusChar"></param>
        /// <returns></returns>
        public KeyEvalState Evaluate(VimKeyInput keyInput, out IVimAction action, out string statusChar)
        {
            // DFA to interpret keyInput

            action = null;
            statusChar = "";

            IVimMotion motion = null;

            #region special key input...

            if (keyInput.Value.Length > 1) {
                // dealing with special key inputs
                if (string.Compare(keyInput.Value, VimKeyInput.Escape) == 0) {
                    // the great "Esc", having power to quit immediately
                    return KeyEvalState.Escape;
                }
                else {
                    if (_actionState == ActionState.SearchWord) {
                        // in search word state, only accepts Key.Enter, Key.Backspace as special key input, else just ignores
                        if (string.Compare(keyInput.Value, VimKeyInput.Enter) == 0) {
                            if (Util.StringHelper.IsNullOrWhiteSpace(_wordToSearch)) {
                                // dirty hacking! if return KeyEvalState.Error, 
                                // the Key.Enter will not be handled, but return to text editor
                                // TODO maybe need a better way to tell whether handling key input or not?
                                return KeyEvalState.Escape;
                            }
                            else {
                                action = new Motions.MotionGotoWordSearch(_wordToSearch, _host);
                                return KeyEvalState.Success;
                            }
                        }
                        else if (string.Compare(keyInput.Value, VimKeyInput.Backspace) == 0) {
                            if (_wordToSearch.Length == 0) {
                                return KeyEvalState.Error;
                            }
                            else {
                                _wordToSearch = _wordToSearch.Substring(0, _wordToSearch.Length - 1);

                                statusChar = Status_Char_Backspace;
                                return KeyEvalState.InProcess;
                            }
                        }
                        else {
                            return KeyEvalState.InProcess;
                        }
                    }
                    else {
                        return KeyEvalState.Error;
                    }
                }
            }

            #endregion

            #region single char key input...

            Debug.Assert(keyInput.Value.Length == 1);
            char key_input = keyInput.Value[0];

            statusChar = key_input.ToString();

            switch (_actionState) {
            case ActionState.SearchWord:
                _wordToSearch += key_input;
                return KeyEvalState.InProcess;
            case ActionState.SearchCharInLine:
                switch (_rangeState) {
                case ActionState.None:
                    action = this.GetSearchCharInLineMotion(_inLineSearchChar, key_input, _repeatNumber.Value);
                    return KeyEvalState.Success;
                case ActionState.RangeEdition:
                    motion = this.GetSearchCharInLineMotion(_inLineSearchChar, key_input, _repeatNumberInRangeEdition.Value);
                    action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                    return KeyEvalState.Success;
                case ActionState.RangeYank:
                    motion = this.GetSearchCharInLineMotion(_inLineSearchChar, key_input, _repeatNumberInRangeYank.Value);
                    action = new Editions.EditionYankRange(_yankRegister, motion, _host,
                        _repeatNumber.Value * _repeatNumberInDoubleQuoteYank.Value);
                    return KeyEvalState.Success;
                default:
                    Debug.Assert(false);
                    return KeyEvalState.Error;
                }
            case ActionState.DoubleQuoteYankBegin:
                _yankRegister = VimRegisterTable.Singleton.SymbolRegisters[key_input.ToString()];
                if (_yankRegister == null) {
                    _yankRegister = VimRegister.DefaultRegister;
                }
                _actionState = ActionState.DoubleQuoteYankSymbolSpecified;
                return KeyEvalState.InProcess;
            default: {

                    #region number char...

                    int num = 0;
                    if (int.TryParse(keyInput.Value, out num)) {
                        switch (_actionState) {
                        case ActionState.None:
                            if (num == 0) {
                                action = new Motions.MotionMoveToStartOfLine(_host, 1);
                                return KeyEvalState.Success;
                            }
                            else {
                                _actionState = ActionState.RepeatNumber;
                                _repeatNumber.Value = num;
                                return KeyEvalState.InProcess;
                            }
                        case ActionState.RepeatNumber:
                            _repeatNumber.Value = (_repeatNumber.Value * 10) + num;
                            return KeyEvalState.InProcess;
                        case ActionState.RangeEdition:
                            if (num == 0) {
                                motion = new Motions.MotionMoveToStartOfLine(_host, 1);
                                action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                                return KeyEvalState.Success;
                            }
                            else {
                                _actionState = ActionState.RepeatNumberInRangeEdition;
                                _repeatNumberInRangeEdition.Value = num;
                                return KeyEvalState.InProcess;
                            }
                        case ActionState.RepeatNumberInRangeEdition:
                            _repeatNumberInRangeEdition.Value = (_repeatNumberInRangeEdition.Value * 10) + num;
                            return KeyEvalState.InProcess;
                        case ActionState.RangeYank:
                            if (num == 0) {
                                motion = new Motions.MotionMoveToStartOfLine(_host, 1);
                                if (_yankRegister == null) {
                                    _yankRegister = VimRegister.DefaultRegister;
                                }
                                action = new Editions.EditionYankRange(_yankRegister, motion, _host, _repeatNumber.Value);
                                return KeyEvalState.Success;
                            }
                            else {
                                _actionState = ActionState.RepeatNumberInRangeYank;
                                _repeatNumberInRangeYank.Value = num;
                                return KeyEvalState.InProcess;
                            }
                        case ActionState.RepeatNumberInRangeYank:
                            _repeatNumberInRangeYank.Value = (_repeatNumberInRangeYank.Value * 10) + num;
                            return KeyEvalState.InProcess;
                        case ActionState.DoubleQuoteYankSymbolSpecified:
                            if (num == 0) {
                                return KeyEvalState.Error;
                            }
                            else {
                                _actionState = ActionState.RepeatNumberInDoubleQuoteYank;
                                _repeatNumberInDoubleQuoteYank.Value = num;
                            }
                            return KeyEvalState.InProcess;
                        case ActionState.RepeatNumberInDoubleQuoteYank:
                            _repeatNumberInDoubleQuoteYank.Value = (_repeatNumberInDoubleQuoteYank.Value * 10) + num;
                            return KeyEvalState.InProcess;
                        default:
                            return KeyEvalState.Error;
                        }
                    }

                    #endregion

                    if (_rangeState == ActionState.None) {
                        switch (_actionState) {
                        case ActionState.None: // fall through
                        case ActionState.RepeatNumber:
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
                                _rangeState = ActionState.RangeEdition;
                                _firstRangeEditionChar = key_input;
                                return KeyEvalState.InProcess;
                            }
                            else if (Search_Char_In_line_Chars.Contains(key_input)) {
                                _actionState = ActionState.SearchCharInLine;
                                _inLineSearchChar = key_input;
                                return KeyEvalState.InProcess;
                            }
                            else {
                                switch (key_input) {
                                case Back_Slash:
                                    if (_actionState != ActionState.None) {
                                        // can only enter ActionState.SearchWord from ActionState.None
                                        return KeyEvalState.Error;
                                    }
                                    else {
                                        _actionState = ActionState.SearchWord;
                                        return KeyEvalState.InProcess;
                                    }
                                case THE_y:
                                    _actionState = ActionState.RangeYank;
                                    _rangeState = ActionState.RangeYank;
                                    return KeyEvalState.InProcess;
                                case Double_Quote:
                                    _actionState = ActionState.DoubleQuoteYankBegin;
                                    return KeyEvalState.InProcess;
                                case THE_g:
                                    _actionState = ActionState.Motion_gg;
                                    return KeyEvalState.InProcess;
                                case Left_Bracket:
                                    _actionState = ActionState.MotionOfLeftBracket;
                                    return KeyEvalState.InProcess;
                                case Right_Bracket:
                                    _actionState = ActionState.MotionOfRightBracket;
                                    return KeyEvalState.InProcess;
                                case THE_z:
                                    _actionState = ActionState.Motion_zz;
                                    return KeyEvalState.InProcess;
                                default:
                                    return KeyEvalState.Error;
                                }
                            }
                        case ActionState.DoubleQuoteYankBegin:
                            _yankRegister = VimRegisterTable.Singleton.SymbolRegisters[key_input.ToString()];
                            if (_yankRegister == null) {
                                _yankRegister = VimRegister.DefaultRegister;
                            }
                            return KeyEvalState.InProcess;
                        case ActionState.DoubleQuoteYankSymbolSpecified: // fall through
                        case ActionState.RepeatNumberInDoubleQuoteYank:
                            if (key_input == THE_y) {
                                _actionState = ActionState.RangeYank;
                                _rangeState = ActionState.RangeYank;
                                return KeyEvalState.InProcess;
                            }
                            else if (key_input == THE_Lowercase_p) {
                                action = new Editions.EditionYankPaste(_yankRegister, _host,
                                    _repeatNumber.Value * _repeatNumberInDoubleQuoteYank.Value);
                                return KeyEvalState.Success;
                            }
                            else if (key_input == THE_Uppercase_P) {
                                action = new Editions.EditionYankPasteBefore(_yankRegister, _host,
                                    _repeatNumber.Value * _repeatNumberInDoubleQuoteYank.Value);
                                return KeyEvalState.Success;
                            }
                            else {
                                return KeyEvalState.Error;
                            }
                        case ActionState.Motion_gg:
                            if (key_input == THE_g) {
                                action = new Motions.MotionGotoLine(_host, _repeatNumber.Value);
                                return KeyEvalState.Success;
                            }
                            else {
                                return KeyEvalState.Error;
                            }
                        case ActionState.MotionOfLeftBracket:
                            if (key_input == Left_Brace) {
                                action = new Motions.MotionGotoLeftBrace(_host, _repeatNumber.Value);
                                return KeyEvalState.Success;
                            }
                            else {
                                return KeyEvalState.Error;
                            }
                        case ActionState.MotionOfRightBracket:
                            if (key_input == Right_Brace) {
                                action = new Motions.MotionGotoRightBrace(_host, _repeatNumber.Value);
                                return KeyEvalState.Success;
                            }
                            else {
                                return KeyEvalState.Error;
                            }
                        case ActionState.Motion_zz:
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
                        default:
                            return KeyEvalState.Error;
                        }
                    }
                    else {
                        Debug.Assert(_rangeState != ActionState.None);

                        if ((_actionState == ActionState.RangeEdition) ||
                            (_actionState == ActionState.RepeatNumberInRangeEdition) ||
                            (_actionState == ActionState.RangeYank) ||
                            (_actionState == ActionState.RepeatNumberInRangeYank)) {
                            if (Simple_Motion_Chars.Contains(key_input)) {
                                _actionState = ActionState.SimpleMotion;
                                if (_rangeState == ActionState.RangeEdition) {
                                    motion = this.GetSimpleMotion(key_input, _repeatNumberInRangeEdition);
                                    action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                                }
                                else if (_rangeState == ActionState.RangeYank) {
                                    motion = this.GetSimpleMotion(key_input, _repeatNumberInRangeYank);
                                    action = new Editions.EditionYankRange(_yankRegister, motion, _host,
                                        _repeatNumber.Value * _repeatNumberInDoubleQuoteYank.Value);
                                }
                                else {
                                    Debug.Assert(false);
                                    return KeyEvalState.Error;
                                }
                                return KeyEvalState.Success;
                            }
                            else if (Range_Edition_Chars.Contains(key_input)) {
                                if (_rangeState != ActionState.RangeEdition) {
                                    return KeyEvalState.Error;
                                }
                                else {
                                    if (key_input != _firstRangeEditionChar) {
                                        return KeyEvalState.Error;
                                    }

                                    action = this.GetDoubleRangeEdition(key_input,
                                        _repeatNumber.Value * _repeatNumberInRangeEdition.Value);
                                    return KeyEvalState.Success;
                                }
                            }
                            else if (Search_Char_In_line_Chars.Contains(key_input)) {
                                _actionState = ActionState.SearchCharInLine;
                                _inLineSearchChar = key_input;
                                return KeyEvalState.InProcess;
                            }
                            else if (key_input == THE_y) {
                                if (_rangeState != ActionState.RangeYank) {
                                    return KeyEvalState.Error;
                                }
                                else {
                                    // expecting no man will really want doing such complex yank operation like #2"5y3w#, but who knows
                                    action = new Editions.EditionYankLine(_yankRegister, _host,
                                        _repeatNumber.Value * _repeatNumberInDoubleQuoteYank.Value * _repeatNumberInRangeYank.Value);
                                    return KeyEvalState.Success;
                                }
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
                        else {
                            motion = null;

                            if (_actionState == ActionState.Motion_gg) {
                                if (key_input == THE_g) {
                                    motion = new Motions.MotionMoveToStartOfDocument(_host, _repeatNumberInRangeEdition.Value);
                                }
                                else {
                                    return KeyEvalState.Error;
                                }
                            }
                            else if (_actionState == ActionState.MotionOfLeftBracket) {
                                if (key_input == Left_Brace) {
                                    motion = new Motions.MotionGotoLeftBrace(_host, _repeatNumberInRangeEdition.Value);
                                }
                                else {
                                    return KeyEvalState.Error;
                                }
                            }
                            else if (_actionState == ActionState.MotionOfRightBracket) {
                                if (key_input == Right_Brace) {
                                    motion = new Motions.MotionGotoRightBrace(_host, _repeatNumberInRangeEdition.Value);
                                }
                                else {
                                    return KeyEvalState.Error;
                                }
                            }
                            else {
                                return KeyEvalState.Error;
                            }

                            if (_rangeState == ActionState.RangeEdition) {
                                action = this.GetRangeEdition(_firstRangeEditionChar, motion, _repeatNumber.Value);
                            }
                            else if (_rangeState == ActionState.RangeYank) {
                                action = new Editions.EditionYankRange(_yankRegister, motion, _host,
                                    _repeatNumber.Value * _repeatNumberInDoubleQuoteYank.Value);
                            }
                            else {
                                Debug.Assert(false);
                                return KeyEvalState.Error;
                            }

                            return KeyEvalState.Success;
                        }
                    }
                }
            }

            #endregion
        }
    }
}
