﻿using System;
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
        public IVimEditionRedoable LastAction { get; private set; }

        public abstract VimPoint CurrentPosition { get; }
        public abstract VimPoint CurrentLineEndPosition { get; }

        public abstract int TextLineCount { get; }

        protected AbstractVimHost()
        {
            this.CurrentMode = new ModeNormal(this);
        }

        public virtual void KeyDown(VimKeyEventArgs args)
        {
            Debug.Assert(this.CurrentMode != null);
            this.CurrentMode.KeyDown(args);
        }

        public abstract void Beep();
        public abstract void DismissDisplayWindows();

        public abstract void Undo();
        public abstract void Redo();

        public abstract char GetCharAtCurrentPosition();

        public abstract bool IsCurrentPositionAtStartOfLine();
        public abstract bool IsCurrentPositionAtStartOfLineText();
        public abstract bool IsCurrentPositionAtEndOfLine();
        public abstract bool IsCurrentPositionAtFirstLine();
        public abstract bool IsCurrentPositionAtLastLine();

        public abstract void MoveCursor(VimPoint pos);
        public abstract void Select(VimPoint from, VimPoint to);

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

        public abstract bool GoToMatch();

        public abstract void OpenLineAbove();
        public abstract void OpenLineBelow();

        public abstract void FormatLine();
        public abstract void FormatLineRange(VimPoint from, VimPoint to);

        public abstract void DeleteLine();
        public abstract void DeleteRange(VimPoint from, VimPoint to);
        public abstract void DeleteLineRange(VimPoint from, VimPoint to);

        public abstract void DeleteTo(VimPoint pos);

        public abstract void DeleteChar();

        public abstract void DeleteWord();

        public abstract void JoinLines(int beginLine, int endLine);

    }
}