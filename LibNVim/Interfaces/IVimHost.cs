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

        string ClipboardText { get; set; }

        string LineBreak { get; }

        int TextLineCount { get; }

        bool CanProcess(VimKeyInput keyInput);
        void KeyDown(VimKeyEventArgs args);

        void Beep();
        void UpdateStatus(string text);
        void DismissDisplayWindows();

        void Undo();
        void Redo();

        /// <summary>
        /// return '\0' at the end of document
        /// </summary>
        /// <returns></returns>
        char GetCharAtCurrentPosition();
        char GetChar(VimPoint pos);
        /// <summary>
        /// if on white space, return that single char
        /// </summary>
        /// <returns></returns>
        string GetWordAtCurrentPosition();
        string GetText(VimSpan span);

        bool FindPreviousChar(char toSearch, out VimPoint pos);
        bool FindNextChar(char toSearch, out VimPoint pos);

        bool FindLeftBrace(VimPoint startPosition, out VimPoint pos);
        bool FindRightBrace(VimPoint startPosition, out VimPoint pos);

        bool FindWord(string word);
        bool FindNextWord(string word);
        bool FindPreviousWord(string word);

        VimPoint GetLineEndPosition(int lineNumber);

        bool IsCurrentPositionAtStartOfDocument();
        bool IsCurrentPositionAtEndOfDocument();

        bool IsCurrentPositionAtFirstLine();
        bool IsCurrentPositionAtLastLine();

        bool IsCurrentPositionAtStartOfLine();
        bool IsCurrentPositionAtStartOfLineText();
        bool IsCurrentPositionAtEndOfLine();
        
        //bool IsCurrentPositionAtEndOfWord();

        //bool IsPositionAtStartOfLine(VimPoint pos);
        //bool IsPositionAtEndOfLine(VimPoint pos);
        //bool IsPositionAtFirstLine(VimPoint pos);
        //bool IsPositionAtLastLine(VimPoint pos);

        void MoveCursor(VimPoint pos);
        void Select(VimSpan span);

        void CaretLeft();
        void CaretRight();
        void CaretUp();
        void CaretDown();

        void MoveToStartOfDocument();
        void MoveToEndOfDocument();

        /// <summary>
        /// line number start from 0
        /// </summary>
        /// <param name="lineNumber"></param>
        void GotoLine(int lineNumber);

        void MoveToStartOfLine();
        void MoveToStartOfLineText();
        void MoveToEndOfLine();

        void MoveToPreviousWord();
        void MoveToNextWord();
        void MoveToEndOfWord();

        void MoveToPreviousCharacter();
        void MoveToNextCharacter();

        bool GotoMatch();

        void InsertTextAtCurrentPosition(string text);
        void InsertTextAtPosition(VimPoint pos, string text);

        void OpenLineAbove();
        void OpenLineBelow();

        void FormatLine();
        void FormatLineRange(VimSpan span);

        void DeleteLine();
        void DeleteRange(VimSpan span);
        void DeleteLineRange(VimSpan span);

        void JoinLines(int beginLine, int endLine);

        void ScrollLineCenter();
    }
}
