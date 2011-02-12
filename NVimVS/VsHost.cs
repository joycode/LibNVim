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
using Microsoft.VisualStudio.Text.IncrementalSearch;

namespace NVimVS
{
    public sealed class VsHost : AbstractVimHost
    {
        private ITextView _textView = null;
        private _DTE _dte = null;
        private IEditorOperations _editorOperations = null;
        private ITextStructureNavigatorSelectorService _textStructureNavigatorSelectorService = null;
        private ITextSearchService _textSearchService = null;
        private IIncrementalSearch _incrementalSearch = null;
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

        public override string SelectedText
        {
            get { return _editorOperations.SelectedText; }
        }

        public override string ClipboardText
        {
            get
            {
                if (!System.Windows.Clipboard.ContainsText(System.Windows.TextDataFormat.Text)) {
                    return null;
                }
                return System.Windows.Clipboard.GetText(System.Windows.TextDataFormat.Text);
            }
            set
            {
                System.Windows.Clipboard.SetText(value, System.Windows.TextDataFormat.Text);
            }
        }

        public override string LineBreak
        {
            get { return "\r\n"; }
        }

        public override int TextLineCount
        {
            get { return _textView.TextSnapshot.LineCount; }
        }

        public VsHost(ITextView textView, _DTE dte, IEditorOperations editorOperations, 
            ITextStructureNavigatorSelectorService textStructureNavigatorSelectorService,
            ITextSearchService textSearchService, IIncrementalSearch incrementalSearch,
            VsVim.IBlockCaret blockCaret, ICompletionBroker completionBroker)
        {
            _textView = textView;
            _dte = dte;
            _editorOperations = editorOperations;
            _textStructureNavigatorSelectorService = textStructureNavigatorSelectorService;
            _textSearchService = textSearchService;
            _incrementalSearch = incrementalSearch;
            _blockCaret = blockCaret;
            _completionBroker = completionBroker;
        }

        private bool SafeExecuteCommand(string command, string args = "")
        {
            try {
                _dte.ExecuteCommand(command, args);
                return true;
            }
            catch (Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
#endif
                return false;
            }
        }

        public override void Beep()
        {
            SystemSounds.Beep.Play();
        }

        public override void UpdateStatus(string text)
        {
            _dte.StatusBar.Text = text;
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
            _editorOperations.ResetSelection();

            if (this.IsCurrentPositionAtEndOfLine()) {
                this.CaretLeft();
            }
        }

        public override void Redo()
        {
            this.SafeExecuteCommand("Edit.Redo");
            _editorOperations.ResetSelection();

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
       
        public override char GetChar(VimPoint pos)
        {
            return this.TranslatePoint(pos).GetChar();
        }

        public override string GetWordAtCurrentPosition()
        {
            char ch = this.GetCharAtCurrentPosition();
            if (char.IsWhiteSpace(ch)) {
                return ch.ToString();
            }

            SnapshotPoint bak = _textView.Caret.Position.BufferPosition;
            _editorOperations.SelectCurrentWord();
            string word = _editorOperations.SelectedText;

            _editorOperations.SelectAndMoveCaret(new VirtualSnapshotPoint(bak), 
                new VirtualSnapshotPoint(bak));

            return word;
        }

        public override string GetText(VimSpan span)
        {
            return this.TranslateSpan(span).GetText();
        }

        private VimPoint TranslatePoint(SnapshotPoint pos)
        {
            ITextSnapshotLine line = pos.GetContainingLine();
            return new VimPoint(line.LineNumber, pos.Position - line.Start.Position);
        }

        private bool IsPositionAtEndOfDocument(SnapshotPoint pos)
        {
            ITextSnapshotLine line = pos.GetContainingLine();
            if (line.LineNumber < (line.Snapshot.LineCount - 1)) {
                return false;
            }

            return (pos.Position == line.End);
        }

        public override bool FindLeftBrace(VimPoint startPosition, out VimPoint pos)
        {
            pos = null;

            SnapshotPoint current_pos = this.TranslatePoint(startPosition);
            if (current_pos.Position == 0) {
                return false;
            }

            bool found = false;

            int depth = 1;            
            ITextSnapshotLine current_line = current_pos.GetContainingLine();
            do {
                if (current_pos.Position != current_line.Start.Position) {
                    current_pos = new SnapshotPoint(current_pos.Snapshot, current_pos.Position - 1);
                }
                else {
                    if (current_line.LineNumber == 0) {
                        break;
                    }
                    current_line = current_line.Snapshot.GetLineFromLineNumber(current_line.LineNumber - 1);
                    current_pos = current_line.End;
                }

                char ch = current_pos.GetChar();

                if (ch == '}') {
                    depth++;
                }
                else if (ch == '{') {
                    depth--;
                    if (depth == 0) {
                        found = true;
                        break;
                    }
                }
            }
            while (current_pos.Position != 0);

            if (!found) {
                return false;
            }

            pos = this.TranslatePoint(current_pos);

            return true;
        }

        public override bool FindRightBrace(VimPoint startPosition, out VimPoint pos)
        {
            pos = null;

            SnapshotPoint current_pos = this.TranslatePoint(startPosition);
            if (this.IsPositionAtEndOfDocument(current_pos)) {
                return false;
            }

            bool found = false;

            int depth = 1;
            ITextSnapshotLine current_line = current_pos.GetContainingLine();
            do {
                if (!this.IsPositionAtEndOfLine(current_pos)) {
                    current_pos = new SnapshotPoint(current_pos.Snapshot, current_pos.Position + 1);
                }
                else {
                    if (this.IsPositionAtEndOfDocument(current_pos)) {
                        break;
                    }
                    current_line = current_line.Snapshot.GetLineFromLineNumber(current_line.LineNumber + 1);
                    current_pos = current_line.Start;

                    if (this.IsPositionAtEndOfDocument(current_pos)) {
                        // in case start of the new line is also end of document
                        break;
                    }
                }

                char ch = current_pos.GetChar();

                if (ch == '{') {
                    depth++;
                }
                else if (ch == '}') {
                    depth--;
                    if (depth == 0) {
                        found = true;
                        break;
                    }
                }
            }
            while (!this.IsPositionAtEndOfDocument(current_pos));

            if (!found) {
                return false;
            }

            pos = this.TranslatePoint(current_pos);

            return true;
        }

        public override bool FindNextWord(string word, bool wholeWord)
        {
            _editorOperations.MoveToNextCharacter(false);

            FindOptions opts = FindOptions.None;
            if (wholeWord) {
                opts |= FindOptions.WholeWord;
            }

            SnapshotSpan? span = _textSearchService.FindNext(_textView.Caret.Position.BufferPosition.Position, true, 
                (new FindData(word, _textView.TextSnapshot, opts, 
                _textStructureNavigatorSelectorService.GetTextStructureNavigator(_textView.TextBuffer))));

            if (!span.HasValue) {
                _editorOperations.MoveToPreviousCharacter(false);
                return false;
            }

            _textView.Caret.MoveTo(span.Value.Start);

            return true;
        }

        public override bool FindPreviousWord(string word, bool wholeWord)
        {
            FindOptions opts = FindOptions.SearchReverse;
            if (wholeWord) {
                opts |= FindOptions.WholeWord;
            }

            SnapshotSpan? span = _textSearchService.FindNext(_textView.Caret.Position.BufferPosition.Position, true,
                (new FindData(word, _textView.TextSnapshot, opts,
                _textStructureNavigatorSelectorService.GetTextStructureNavigator(_textView.TextBuffer))));

            if (!span.HasValue) {
                return false;
            }

            _textView.Caret.MoveTo(span.Value.Start);

            return true;
        }

        public override VimPoint GetLineEndPosition(int lineNumber)
        {
            ITextSnapshotLine line = _textView.TextSnapshot.GetLineFromLineNumber(lineNumber);
            return new VimPoint(lineNumber, line.End.Position - line.Start.Position);
        }

        public override bool IsCurrentPositionAtStartOfDocument()
        {
            SnapshotPoint pos = _textView.Caret.Position.BufferPosition;
            ITextSnapshotLine line = pos.GetContainingLine();

            return (line.LineNumber == 0);
        }

        public override bool IsCurrentPositionAtEndOfDocument()
        {
            SnapshotPoint pos = _textView.Caret.Position.BufferPosition;

            ITextSnapshotLine line = pos.GetContainingLine();
            if (line.LineNumber != (_textView.TextSnapshot.LineCount - 1))
            {
                return false;
            }

            return this.IsCurrentPositionAtEndOfLine();
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

        private SnapshotPoint TranslatePoint(VimPoint pos)
        {
            Debug.Assert(pos.X < _textView.TextSnapshot.LineCount);
            ITextSnapshotLine line = _textView.TextSnapshot.GetLineFromLineNumber(pos.X);

            Debug.Assert(pos.Y < (line.Length + 1));
            int dst_pos = line.Start.Position + pos.Y;

            return new SnapshotPoint(_textView.TextSnapshot, dst_pos);
        }

        private bool IsPositionAtEndOfLine(SnapshotPoint pos)
        {
            ITextSnapshotLine line = pos.GetContainingLine();

            return (pos.Position == line.End.Position);
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

        private void Select(VimSpan span)
        {
            SnapshotSpan editor_span = this.TranslateSpan(span);

            _editorOperations.SelectAndMoveCaret(new VirtualSnapshotPoint(editor_span.Start),
                new VirtualSnapshotPoint(editor_span.End));
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

        public override void MoveToPreviousCharacter()
        {
            _editorOperations.MoveToPreviousCharacter(false);
        }

        public override void MoveToNextCharacter()
        {
            _editorOperations.MoveToNextCharacter(false);
        }

        public override bool GotoMatch()
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

        public override void InsertTextAtCurrentPosition(string text)
        {
            ITextEdit edit = _textView.TextBuffer.CreateEdit();
            edit.Insert(_textView.Caret.Position.BufferPosition.Position, text);
            edit.Apply();
        }

        public override void InsertTextAtPosition(VimPoint pos, string text)
        {
            ITextEdit edit = _textView.TextBuffer.CreateEdit();

            SnapshotPoint editor_pos = this.TranslatePoint(pos);
            edit.Insert(editor_pos.Position, text);

            edit.Apply();
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

        public override void FormatLineRange(VimSpan span)
        {
            this.Select(span);
            this.SafeExecuteCommand("Edit.FormatSelection");
            _editorOperations.ResetSelection();
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
                _editorOperations.ResetSelection();
                return;
            }
            _editorOperations.Delete();
        }

        public override void DeleteLineRange(VimSpan span)
        {
            this.Select(span);
            if (!_editorOperations.CanDelete) {
                _editorOperations.ResetSelection();
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

        public override void ScrollLineCenter()
        {
            _editorOperations.ScrollLineCenter();
        }
    }
}
