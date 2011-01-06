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
        private VsVim.IBlockCaret _blockCaret = null;
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

        public VsHost(ITextView textView, _DTE dte, IEditorOperations editorOperations, VsVim.IBlockCaret blockCaret,
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
            this.SafeExecuteCommand("Edit.Undo");
            this.Select(this.CurrentPosition, this.CurrentPosition);
            if (this.IsCurrentPositionAtEndOfLine()) {
                this.CaretLeft();
            }
        }

        public override void Redo()
        {
            this.SafeExecuteCommand("Edit.Redo");
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

        private bool IsPositionAtEndOfLine(VimPoint pos)
        {
            SnapshotPoint point = this.TranslatePoint(pos);
            ITextSnapshotLine line = point.GetContainingLine();

            return (point.Position == line.End.Position);
        }

        private SnapshotSpan TranslateSpan(VimSpan span)
        {
            if (span.StartClosed && span.EndClosed) {
                SnapshotPoint from = this.TranslatePoint(span.Start);

                Debug.Assert(!this.IsPositionAtEndOfLine(span.End));
                SnapshotPoint to = this.TranslatePoint(new VimPoint(span.End.X, span.End.Y + 1));

                return new SnapshotSpan(from, to);
            }
            else if (span.StartClosed && !span.EndClosed) {
                SnapshotPoint from = this.TranslatePoint(span.Start);
                SnapshotPoint to = this.TranslatePoint(span.End);
                return new SnapshotSpan(from, to);
            }
            else if (!span.StartClosed && span.EndClosed) {
                Debug.Assert(!this.IsPositionAtEndOfLine(span.Start));
                SnapshotPoint from = this.TranslatePoint(new VimPoint(span.Start.X, span.Start.Y + 1));

                Debug.Assert(!this.IsPositionAtEndOfLine(span.End));
                SnapshotPoint to = this.TranslatePoint(new VimPoint(span.End.X, span.End.Y + 1));

                return new SnapshotSpan(from, to);
            }
            else if (!span.StartClosed && !span.EndClosed) {
                Debug.Assert(!this.IsPositionAtEndOfLine(span.Start));
                SnapshotPoint from = this.TranslatePoint(new VimPoint(span.Start.X, span.Start.Y + 1));

                SnapshotPoint to = this.TranslatePoint(span.End);

                return new SnapshotSpan(from, to);
            }
            else {
                Debug.Assert(false);
                throw new InvalidOperationException();
            }
        }

        public override void MoveCursor(VimPoint pos)
        {
            _textView.Caret.MoveTo(this.TranslatePoint(pos));
        }

        public override void Select(VimSpan span)
        {
            SnapshotSpan editor_span = this.TranslateSpan(span);

            _editorOperations.SelectAndMoveCaret(new VirtualSnapshotPoint(editor_span.Start),
                new VirtualSnapshotPoint(editor_span.End));
        }

        public override void Select(VimPoint from, VimPoint to)
        {
            VimSpan span = new VimSpan(from, true, to, false);
            this.Select(span);
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
            // MoveToHome has 2 behaviors: 1) MoveToStartOfLineText 2) MoveToStartOfLine
            // we need be insure only 1) happens

            // a safe way to do this, else such as MoveToEndOfLine will have problem if only one char in the line
            _editorOperations.MoveToStartOfLine(false);
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
                // when last word, VsEditor.MoveToNextWord jumps to end of line, not the next word
                // so an additional VsEditor.MoveToNextWord is executed
                if (this.IsCurrentPositionAtEndOfDocument())
                {
                    // if we are at the end of document, then no next word to go
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
            this.SafeExecuteCommand("Edit.LineOpenAbove");
        }

        public override void OpenLineBelow()
        {
            this.SafeExecuteCommand("Edit.LineOpenBelow");
        }

        public override void FormatLine()
        {
            this.SafeExecuteCommand("Edit.FormatSelection");
        }

        public override void FormatLineRange(VimPoint from, VimPoint to)
        {
            this.Select(from, to);
            this.SafeExecuteCommand("Edit.FormatSelection");
            this.Select(from, from);
        }

        public override void DeleteLine()
        {
            if (this.IsCurrentPositionAtEndOfDocument() && this.IsCurrentPositionAtStartOfLine())
            {
                // empty line at the end of document
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

        public override void DeleteRange(VimSpan span)
        {
            SnapshotSpan editor_span = this.TranslateSpan(span);
            this.Select(span);
            if (!_editorOperations.CanDelete) {
                this.Select(span.Start, span.End);
                return;
            }
            _editorOperations.Delete();
        }

        public override void DeleteRange(VimPoint from, VimPoint to)
        {
            this.DeleteRange(new VimSpan(from, true, to, false));
        }

        public override void DeleteLineRange(VimSpan span)
        {
            this.Select(span);
            if (!_editorOperations.CanDelete) {
                this.Select(span.Start, span.Start);
                return;
            }

            int dst_end_line = Math.Max(span.Start.X, span.End.X);
            bool need_cleanup = dst_end_line == (_textView.TextSnapshot.LineCount - 1);

            _editorOperations.DeleteFullLine();

            if (need_cleanup) {
                if (this.IsCurrentPositionAtFirstLine()) {
                    return;
                }
                this.SafeExecuteCommand("Edit.DeleteBackwards");
            }
        }

        public override void DeleteLineRange(VimPoint from, VimPoint to)
        {
            this.DeleteLineRange(new VimSpan(from, true, to, false));
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
                Span span = new Span(line_break_start, line.LineBreakLength);
                edit.Replace(span, " ");

                int line_break_end = line.EndIncludingLineBreak.Position;
                pos = line_break_end;
                char ch = new SnapshotPoint(_textView.TextSnapshot, pos).GetChar();
                while (char.IsWhiteSpace(ch)) {
                    if (ch == '\r' || ch == '\n') {
                        // not overflow to next line
                        break;
                    }
                    pos++;
                    ch = new SnapshotPoint(_textView.TextSnapshot, pos).GetChar();
                }
                span = new Span(line_break_end, pos - line_break_end);
                edit.Replace(span, " ");
            }

            edit.Apply();
        }
    }
}
