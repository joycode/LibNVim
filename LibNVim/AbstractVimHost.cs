using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Motions;
using System.Diagnostics;
using LibNVim.Modes;
using LibNVim.Interfaces;

namespace LibNVim
{
    public abstract class AbstractVimHost : IVimHost
    {
        public IVimMode CurrentMode { get; set; }

        public abstract VimPoint CurrentPosition { get; }
        public abstract VimPoint CurrentLineEndPosition { get; }

        public abstract string LineBreak { get; }

        public abstract int TextLineCount { get; }

        protected AbstractVimHost()
        {
            this.CurrentMode = new ModeNormal(this);
        }

        public virtual bool CanProcess(VimKeyInput keyInput)
        {
            return this.CurrentMode.CanProcess(keyInput);
        }

        public virtual void KeyDown(VimKeyEventArgs args)
        {
            Debug.Assert(this.CurrentMode != null);
            this.CurrentMode.KeyDown(args);
        }

        public abstract void Beep();
        public abstract void UpdateStatus(string text);
        public abstract void DismissDisplayWindows();

        public abstract void Undo();
        public abstract void Redo();

        public abstract char GetCharAtCurrentPosition();
        public abstract char GetChar(VimPoint pos);
        public abstract string GetWordAtCurrentPosition();
        public abstract string GetText(VimSpan span);

        public abstract bool FindPreviousChar(char toSearch, out VimPoint pos);
        public abstract bool FindNextChar(char toSearch, out VimPoint pos);

        public abstract bool FindLeftBrace(VimPoint startPosition, out VimPoint pos);
        public abstract bool FindRightBrace(VimPoint startPosition, out VimPoint pos);

        public abstract bool  FindWord(string word);
        public abstract bool FindNextWord(string word);
        public abstract bool FindPreviousWord(string word);

        public abstract VimPoint GetLineEndPosition(int lineNumber);

        public abstract bool IsCurrentPositionAtStartOfDocument();
        public abstract bool IsCurrentPositionAtEndOfDocument();

        public abstract bool IsCurrentPositionAtFirstLine();
        public abstract bool IsCurrentPositionAtLastLine();

        public abstract bool IsCurrentPositionAtStartOfLine();
        public abstract bool IsCurrentPositionAtStartOfLineText();
        public abstract bool IsCurrentPositionAtEndOfLine();

        public abstract void MoveCursor(VimPoint pos);
        public abstract void Select(VimSpan span);

        public abstract void CaretLeft();
        public abstract void CaretRight();
        public abstract void CaretUp();
        public abstract void CaretDown();

        public abstract void MoveToStartOfDocument();
        public abstract void MoveToEndOfDocument();

        public abstract void GotoLine(int lineNumber);

        public abstract void MoveToStartOfLine();
        public abstract void MoveToStartOfLineText();
        public abstract void MoveToEndOfLine();

        public abstract void MoveToPreviousWord();
        public abstract void MoveToNextWord();
        public abstract void MoveToEndOfWord();

        public abstract void MoveToPreviousCharacter();
        public abstract void MoveToNextCharacter();

        public abstract bool GotoMatch();

        public abstract void InsertTextAtCurrentPosition(string text);
        public abstract void InsertTextAtPosition(VimPoint pos, string text);

        public abstract void OpenLineAbove();
        public abstract void OpenLineBelow();

        public abstract void FormatLine();
        public abstract void FormatLineRange(VimSpan span);

        public abstract void DeleteLine();
        public abstract void DeleteRange(VimSpan span);
        public abstract void DeleteLineRange(VimSpan span);

        public abstract void JoinLines(int beginLine, int endLine);

        public abstract void ScrollLineCenter();
    }
}
