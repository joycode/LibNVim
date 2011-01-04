using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Interfaces
{
    public interface IVimHost
    {
        IVimMode CurrentMode { get; set; }

        VimPoint CurrentPosition { get; }
        VimPoint CurrentLineEndPosition { get; }

        int TextLineCount { get; }

        IVimEditionRedoable LastAction { get; }

        void KeyDown(VimKeyEventArgs args);

        void Beep();
        void DismissDisplayWindows();

        void Undo();
        void Redo();

        /// <summary>
        /// 文档尾返回 '\0'
        /// </summary>
        /// <returns></returns>
        char GetCharAtCurrentPosition();

        bool IsCurrentPositionAtStartOfLine();
        bool IsCurrentPositionAtStartOfLineText();
        bool IsCurrentPositionAtEndOfLine();
        bool IsCurrentPositionAtFirstLine();
        bool IsCurrentPositionAtLastLine();
        
        //bool IsCurrentPositionAtEndOfWord();

        //bool IsPositionAtStartOfLine(VimPoint pos);
        //bool IsPositionAtEndOfLine(VimPoint pos);
        //bool IsPositionAtFirstLine(VimPoint pos);
        //bool IsPositionAtLastLine(VimPoint pos);

        void MoveCursor(VimPoint pos);
        void Select(VimPoint from, VimPoint to);

        void CaretLeft();
        void CaretRight();
        void CaretUp();
        void CaretDown();

        void MoveToStartOfDocument();
        void MoveToEndOfDocument();

        void GotoLine(int lineNumber);

        void MoveToStartOfLine();
        void MoveToStartOfLineText();
        void MoveToEndOfLine();

        void MoveToPreviousWord();
        void MoveToNextWord();
        void MoveToEndOfWord();

        bool GoToMatch();

        void OpenLineAbove();
        void OpenLineBelow();

        void FormatLine();
        void FormatLineRange(VimPoint from, VimPoint to);

        void DeleteLine();
        void DeleteRange(VimPoint from, VimPoint to);
        void DeleteLineRange(VimPoint from, VimPoint to);

        void DeleteTo(VimPoint pos);

        void DeleteChar();

        void DeleteWord();

        void JoinLines(int beginLine, int endLine);
    }
}
