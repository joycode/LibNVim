using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using EnvDTE;
using System.Media;
using Microsoft.VisualStudio.Text.Formatting;
using System.Diagnostics;
using Microsoft.VisualStudio.Language.Intellisense;

namespace VsNVim
{
    public sealed class VsHost : AbstractVimHost
    {
        
        private ITextView _textView = null;
        private _DTE _dte = null;
        private IEditorOperations _editorOperations = null;
        private IBlockCaret _blockCaret = null;
        private ICompletionBroker _completionBroker = null;

        public override VimPoint CurrentPosition
        {
            get 
            {
                SnapshotPoint pos = _textView.Caret.Position.BufferPosition;

                ITextSnapshotLine line = pos.GetContainingLine();

                int x = line.LineNumber;
                int y = pos.Position - line.Start.Position;

                return new VimPoint(x, y);
            }
        }
        public override VimPoint CurrentLineEndPosition
        {
            get
            {
                ITextSnapshotLine line = _textView.Caret.Position.BufferPosition.GetContainingLine();

                int x = line.LineNumber;
                int y = line.End.Position - line.Start.Position;

                return new VimPoint(x, y);
            }
        }
        public override int TextLineCount
        {
            get { return _textView.TextSnapshot.LineCount; }
        }

        public VsHost(ITextView textView, _DTE dte, IEditorOperations editorOperations, IBlockCaret blockCaret,
            ICompletionBroker completionBroker)
        {
            _textView = textView;
            _dte = dte;
            _editorOperations = editorOperations;
            _blockCaret = blockCaret;
            _completionBroker = completionBroker;
        }

        private bool SafeExecuteCommand(string command, string args = "")
        {
            try {
                _dte.ExecuteCommand(command, args);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public override void Beep()
        {
            SystemSounds.Beep.Play();
        }

        public override void DismissDisplayWindows()
        {
            if (_completionBroker.IsCompletionActive(_textView))
            {
                _completionBroker.DismissAllSessions(_textView);
            }
        }

        public override void KeyDown(VimKeyEventArgs args)
        {
            base.KeyDown(args);

            if (this.CurrentMode.CaretShape != _blockCaret.CaretShape) {
                _blockCaret.CaretShape = this.CurrentMode.CaretShape;
            }
        }

        public override void Undo()
        {
            SafeExecuteCommand("Edit.Undo");
            this.Select(this.CurrentPosition, this.CurrentPosition);
            if (this.IsCurrentPositionAtEndOfLine()) {
                this.CaretLeft();
            }
        }

        public override void Redo()
        {
            SafeExecuteCommand("Edit.Redo");
            this.Select(this.CurrentPosition, this.CurrentPosition);
            if (this.IsCurrentPositionAtEndOfLine()) {
                this.CaretLeft();
            }
        }

        public override char GetCharAtCurrentPosition()
        {
            if (this.IsCurrentPositionAtEndOfDocument()) {
                return '\0';
            }

            return _textView.Caret.Position.BufferPosition.GetChar();
        }

        public bool IsCurrentPositionAtEndOfDocument()
        {
            SnapshotPoint pos = _textView.Caret.Position.BufferPosition;
            ITextSnapshotLine line = pos.GetContainingLine();
            if (line.LineNumber != (_textView.TextSnapshot.LineCount - 1))
            {
                return false;
            }

            return this.IsCurrentPositionAtEndOfLine();
        }

        public override bool IsCurrentPositionAtStartOfLine()
        {
            SnapshotPoint pos = _textView.Caret.Position.BufferPosition;
            return (pos.Position == pos.GetContainingLine().Start.Position);
        }

        public override bool IsCurrentPositionAtStartOfLineText()
        {
            SnapshotPoint bak = _textView.Caret.Position.BufferPosition;
            this.MoveToStartOfLine();
            this.MoveToStartOfLineText();
            SnapshotPoint test_pos = _textView.Caret.Position.BufferPosition;
            _textView.Caret.MoveTo(bak);

            return (bak == test_pos);
        }

        public override bool IsCurrentPositionAtEndOfLine()
        {
            SnapshotPoint pos = _textView.Caret.Position.BufferPosition;
            return (pos.Position == pos.GetContainingLine().End.Position);
        }

        public override bool IsCurrentPositionAtFirstLine()
        {
            SnapshotPoint pos = _textView.Caret.Position.BufferPosition;
            return (pos.GetContainingLine().LineNumber == 0);
        }

        public override bool IsCurrentPositionAtLastLine()
        {
            if (_textView.TextSnapshot.LineCount == 0) {
                return true;
            }

            SnapshotPoint pos = _textView.Caret.Position.BufferPosition;
            int end_line = _textView.TextSnapshot.LineCount - 1;

            return (pos.GetContainingLine().LineNumber == end_line);
        }

        private SnapshotPoint TranslatePoint(VimPoint pos)
        {
            Debug.Assert(pos.X < _textView.TextSnapshot.LineCount);
            ITextSnapshotLine line = _textView.TextSnapshot.GetLineFromLineNumber(pos.X);

            Debug.Assert(pos.Y < (line.Length + 1));
            int dst_pos = line.Start.Position + pos.Y;

            return new SnapshotPoint(_textView.TextSnapshot, dst_pos);
        }

        public override void MoveCursor(VimPoint pos)
        {
            _textView.Caret.MoveTo(this.TranslatePoint(pos));
        }

        public override void Select(VimPoint from, VimPoint to)
        {
            SnapshotPoint editor_from = this.TranslatePoint(from);
            SnapshotPoint editor_to = this.TranslatePoint(to);

            _editorOperations.SelectAndMoveCaret(new VirtualSnapshotPoint(editor_from), 
                new VirtualSnapshotPoint(editor_to));
        }

        public override void CaretLeft()
        {
            if (this.IsCurrentPositionAtStartOfLine()) {
                return;
            }

            _textView.Caret.MoveTo(new SnapshotPoint(_textView.TextSnapshot, 
                _textView.Caret.Position.BufferPosition.Position - 1));
        }

        public override void CaretRight()
        {
            if (this.IsCurrentPositionAtEndOfLine()) {
                return;
            }

            _textView.Caret.MoveTo(new SnapshotPoint(_textView.TextSnapshot,
                _textView.Caret.Position.BufferPosition.Position + 1));
        }

        public override void CaretUp()
        {
            if (this.IsCurrentPositionAtFirstLine()) {
                return;
            }

            _editorOperations.MoveLineUp(false);
        }

        public override void CaretDown()
        {
            if (this.IsCurrentPositionAtLastLine()) {
                return;
            }

            _editorOperations.MoveLineDown(false);
        }

        public override void MoveToStartOfDocument()
        {
            _editorOperations.MoveToStartOfDocument(false);
        }

        public override void MoveToEndOfDocument()
        {
            _editorOperations.MoveToEndOfDocument(false);
        }

        public override void GotoLine(int lineNumber)
        {
            _editorOperations.GotoLine(lineNumber);
        }

        public override void MoveToStartOfLine()
        {
            _editorOperations.MoveToStartOfLine(false);
        }

        public override void MoveToStartOfLineText()
        {
            // MoveToHome 有 2 种语义: 1) MoveToStartOfLineText 2) MoveToStartOfLine, 先做一次移动, 以消去 2)
            _editorOperations.MoveToStartOfLine(false); // 最安全的消去方式, 如果只有一个字符 MoveToEndOfLine 会有问题
            _editorOperations.MoveToHome(false);
        }

        public override void MoveToEndOfLine()
        {
            _editorOperations.MoveToEndOfLine(false);
        }

        public override void MoveToPreviousWord()
        {
            SnapshotPoint old = _textView.Caret.Position.BufferPosition;

            _editorOperations.MoveToPreviousWord(false);

            if (char.IsWhiteSpace(this.GetCharAtCurrentPosition()))
            {
                SnapshotPoint pos = _textView.Caret.Position.BufferPosition;
                if (pos.GetContainingLine() == old.GetContainingLine())
                {
                    _editorOperations.MoveToPreviousWord(false);
                }
            }
        }

        public override void MoveToNextWord()
        {
            _editorOperations.MoveToNextWord(false);

            if (this.IsCurrentPositionAtEndOfLine())
            {
                // 最后一个单词时, VsEditor.MoveToNextWord 会跳到行尾, 而不是下一个单词,
                // 所以附加一次 VsEditor.MoveToNextWord
                if (this.IsCurrentPositionAtEndOfDocument())
                {
                    // 但如果是文档末尾, 则无视该规则
                    this.CaretLeft();
                }
                _editorOperations.MoveToNextWord(false);
            }
        }

        private void MoveToEndOfWordCore()
        {
            _editorOperations.MoveToNextWord(false);

            do {
                _editorOperations.MoveToPreviousCharacter(false);
                char ch = this.GetCharAtCurrentPosition();
                if (!char.IsWhiteSpace(ch)) {
                    break;
                }
            }
            while (true);
        }

        public override void MoveToEndOfWord()
        {
            if (this.IsCurrentPositionAtEndOfDocument())
            {
                return;
            }

            if (this.IsCurrentPositionAtEndOfLine())
            {
                this.MoveToNextWord();
                this.MoveToEndOfWordCore();
                return;
            }

            _editorOperations.MoveToNextCharacter(false);
            if (this.IsCurrentPositionAtEndOfDocument()) {
                this.CaretLeft();
                return;
            }

            char ch = this.GetCharAtCurrentPosition();
            if (char.IsWhiteSpace(ch))
            {
                this.MoveToNextWord();
                this.MoveToEndOfWordCore();
                return;
            }

            this.MoveToEndOfWordCore();
        }

        public override bool GoToMatch()
        {
            char[] Left_Brackets = { '(', '[', '{' };

            bool need_move_left = false;
            if (Left_Brackets.Contains(this.GetCharAtCurrentPosition())) {
                need_move_left = true;
            }

            if (!SafeExecuteCommand("Edit.GoToBrace")) {
                return false;
            }

            if (need_move_left) {
                this.CaretLeft();
            }

            return true;
        }

        public override void OpenLineAbove()
        {
            _editorOperations.OpenLineAbove();
        }

        public override void OpenLineBelow()
        {
            _editorOperations.OpenLineBelow();
        }

        public override void FormatLine()
        {
            SafeExecuteCommand("Edit.FormatSelection");
        }

        public override void FormatLineRange(VimPoint from, VimPoint to)
        {
            this.Select(from, to);
            SafeExecuteCommand("Edit.FormatSelection");
            this.Select(from, from);
        }

        public override void DeleteLine()
        {
            if (this.IsCurrentPositionAtStartOfLine() && this.IsCurrentPositionAtEndOfDocument())
            {
                // 处理文章尾的空行
                if (this.IsCurrentPositionAtFirstLine())
                {
                    return;
                }
                this.SafeExecuteCommand("Edit.DeleteBackwards");
            }
            else
            {
                if (_editorOperations.CanDelete)
                {
                    bool need_cleanup = false;
                    if (this.IsCurrentPositionAtLastLine())
                    {
                        need_cleanup = true;
                    }

                    _editorOperations.DeleteFullLine();

                    if (need_cleanup)
                    {
                        if (this.IsCurrentPositionAtFirstLine())
                        {
                            return;
                        }
                        this.SafeExecuteCommand("Edit.DeleteBackwards");
                    }
                }
            }
        }

        public override void DeleteRange(VimPoint from, VimPoint to)
        {
            this.Select(from, to);
            if (!_editorOperations.CanDelete) {
                this.Select(from, from);
            }
            _editorOperations.Delete();
        }

        public override void DeleteLineRange(VimPoint from, VimPoint to)
        {
            this.Select(from, to);
            if (!_editorOperations.CanDelete) {
                this.Select(from, from);
            }

            bool need_cleanup = (to.X == (_textView.TextSnapshot.LineCount - 1));

            _editorOperations.DeleteFullLine();

            if (need_cleanup) {
                if (this.IsCurrentPositionAtFirstLine()) {
                    return;
                }
                this.SafeExecuteCommand("Edit.DeleteBackwards");
            }
        }

        public override void DeleteTo(VimPoint pos)
        {
            throw new NotImplementedException();
        }

        public override void DeleteChar()
        {
            if (_editorOperations.CanDelete) {
                _editorOperations.Delete();
            }
        }

        public override void DeleteWord()
        {
            throw new NotImplementedException();
        }

        public override void JoinLines(int beginLine, int endLine)
        {
            Debug.Assert((beginLine < endLine) && (endLine < _textView.TextSnapshot.LineCount));

            ITextEdit edit = _textView.TextBuffer.CreateEdit();

            int pos = _textView.TextSnapshot.GetLineFromLineNumber(beginLine).End.Position + 1;
            int loop = endLine - beginLine;
            for (int i = (endLine - beginLine - 1); i >= 0; i--) {
                ITextSnapshotLine line = _textView.TextSnapshot.GetLineFromLineNumber(beginLine + i);
                int line_break_start = line.End.Position;
                //Span span = new Span(line_break_start, line.LineBreakLength);
                //edit.Replace(span, " ");

                pos = line_break_start;
                char ch = new SnapshotPoint(_textView.TextSnapshot, pos).GetChar();
                while (char.IsWhiteSpace(ch)) {
                    pos++;
                    ch = new SnapshotPoint(_textView.TextSnapshot, pos).GetChar();
                }
                Span span = new Span(line_break_start, pos - line_break_start);
                edit.Replace(span, " ");
            }

            edit.Apply();
        }
    }
}
