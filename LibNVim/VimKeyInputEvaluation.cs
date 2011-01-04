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
        public enum KeyEvalState
        {
            None = 0, Error, InProcess, Success, Escape,
        }

        /// <summary>
        /// 有的命令, 如 'G' 需要区分是默认的数字还是输入的数字
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

        private enum ActionState
        {
            None = 0, 
            RepeatNumber, // 命令最初的数字, 比如 5j
            SimpleMotion, // 简单的移动命令, 比如 j
            SimpleEdition, // 简单的编辑命令, 比如 i
            RangeEdition, // 识别 cc, dd, yy, ==
            RepeatNumberInRangeEdition, // 识别 RangeEdition 如 d5w 中的数字
            Motion_gg, // 识别 gg
            MotionOfLeftBracket, // 识别 [{, 暂时不支持 [[, 不怎么了解, 用的也不多
            MotionOfRightBracket, // 识别 ]}
        }

        /// <summary>
        /// 所有的简单移动字符 
        /// TODO '*' 是否需要独立出来, 有待考虑
        /// </summary>
        private static char[] Simple_Motion_Chars = { 'h', 'H', 'j', 'k', 'l', 'L', '0', '^', '$', 'G',
                                                        'w', 'W', 'e', 'E', 'b', 'B', '%', '*' };
        /// <summary>
        /// 'u' 撤销/'U' 重做
        /// </summary>
        private static char[] Simple_Edition_Chars = { '.', 'u', 'U', 'x', 'X', 'i', 'I', 'a', 'A', 's', 'S', 'o', 
                                                         'O', 'p', 'P', 'C', 'D', 'J' };
        private static char[] Range_Edition_Chars = { 'c', 'd', 'y', '=' };
        private static char THE_g = 'g';
        private static char Left_Bracket = '[';
        private static char Left_Brace = '{';
        private static char Right_Bracket = ']';
        private static char Right_Brace = '}';

        private IVimHost _host = null;

        private ActionState _actionState = ActionState.None;

        /// <summary>
        /// 默认重复次数为 1
        /// </summary>
        private RepeatNumberStore _repeatNumber = new RepeatNumberStore();
        private RepeatNumberStore _repeatNumberInRangeEdition = new RepeatNumberStore();
        private char _firstRangeEditionChar = '\0';

        /// <summary>
        /// 记录 RangeEdition 的状态, 因为 _actionType 可能会被 ActionType.RepeatNumberInRangeEdition 冲掉
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
                        return new Motions.MotionMoveToEndOfDocument(_host, repeat.Value);
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
                    return new Editions.EditionApplyLastEditionRedoable(_host, repeat);
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
                    break;
                case 'P':
                    break;
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
        /// 对 "cc" "dd", "yy", "==" 的触发
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
                    break;
                case '=':
                    return new Editions.EditionFormatLine(_host, repeat);
                default:
                    Debug.Assert(false);
                    break;
            }

            Debug.Assert(false);
            return null;
        }

        private IVimEdititon GetRangeEdition(char keyInput, int repeat, IVimMotion motion)
        {
            switch (keyInput) {
                case 'c':
                    return new Editions.EditionChangeRange(_host, repeat, motion);
                case 'd':
                    return new Editions.EditionDeleteRange(_host, repeat, motion);
                case 'y':
                    break;
                case '=':
                    return new Editions.EditionFormatRange(_host, repeat, motion);
                default:
                    Debug.Assert(false);
                    break;
            }

            Debug.Assert(false);
            return null;
        }

        public KeyEvalState Evaluate(VimKeyInput keyInput, out IVimAction action)
        {
            // 通过有限状态自动机解析输入

            action = null;

            // 只要遇到 "Esc" 就从解析过程完全退出, 不理会之前的输入
            // TODO 是否合理, 有待探讨
            if (string.Compare(keyInput.Value, VimKeyInput.Escape) == 0) {
                return KeyEvalState.Escape;
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
                    _actionState = ActionState.RepeatNumberInRangeEdition;
                    _repeatNumberInRangeEdition.Value = num;
                }
                else if (_actionState == ActionState.RepeatNumberInRangeEdition) {
                    _repeatNumberInRangeEdition.Value = (_repeatNumberInRangeEdition.Value * 10) + num;
                }
                else {
                    return KeyEvalState.Error;
                }

                return KeyEvalState.InProcess;
            }

            if (!_inRangeEdition) {
                if ((_actionState == ActionState.None) || (_actionState == ActionState.RepeatNumber)) {
                    // ActionType.None 和 ActionType.RepeatNumber 可以并为一个处理
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
                else if (_actionState == ActionState.Motion_gg) {
                    if (key_input == THE_g) {
                        return KeyEvalState.None;
                    }
                    else {
                        return KeyEvalState.Error;
                    }
                }
                else if (_actionState == ActionState.MotionOfLeftBracket) {
                    if (key_input == Left_Brace) {
                        return KeyEvalState.None;
                    }
                    else {
                        return KeyEvalState.Error;
                    }
                }
                else if (_actionState == ActionState.MotionOfRightBracket) {
                    if (key_input == Right_Brace) {
                        return KeyEvalState.None;
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
                    action = this.GetRangeEdition(_firstRangeEditionChar, _repeatNumber.Value, motion);
                    return KeyEvalState.Success;
                }
                else if (Range_Edition_Chars.Contains(key_input)) {
                    if (key_input != _firstRangeEditionChar) {
                        return KeyEvalState.Error;
                    }

                    action = this.GetDoubleRangeEdition(key_input, _repeatNumber.Value * _repeatNumberInRangeEdition.Value);
                    return KeyEvalState.Success;
                }
                else if (key_input == THE_g) {
                    _actionState = ActionState.Motion_gg;
                    return KeyEvalState.Success;
                }
                else if (key_input == Left_Bracket) {
                    _actionState = ActionState.MotionOfLeftBracket;
                    return KeyEvalState.Success;
                }
                else if (key_input == Right_Bracket) {
                    _actionState = ActionState.MotionOfRightBracket;
                    return KeyEvalState.Success;
                }
                else {
                    return KeyEvalState.Error;
                }
            }
            else if (_actionState == ActionState.Motion_gg) {
                if (key_input == THE_g) {
                    return KeyEvalState.Success;
                }
                else {
                    return KeyEvalState.Error;
                }
            }
            else if (_actionState == ActionState.MotionOfLeftBracket) {
                if (key_input == Left_Brace) {
                    return KeyEvalState.Success;
                }
                else {
                    return KeyEvalState.Error;
                }
            }
            else if (_actionState == ActionState.MotionOfRightBracket) {
                if (key_input == Right_Brace) {
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
