using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Interfaces
{
    public interface IVimHost
    {
        IVimMode CurrentMode { get; set; }

        /// <summary>
        /// text cursor's position
        /// </summary>
        VimPoint CurrentPosition { get; }
        /// <summary>
        /// line end position of current cursor
        /// </summary>
        VimPoint CurrentLineEndPosition { get; }
        /// <summary>
        /// text's selected text
        /// </summary>
        string SelectedText { get; }

        /// <summary>
        /// interoperate with system's clipboard
        /// </summary>
        string ClipboardText { get; set; }

        string LineBreak { get; }
        /// <summary>
        /// document's line count
        /// </summary>
        int TextLineCount { get; }

        /// <summary>
        /// tells editor whether "keyInput" can be processed by IVimHost
        /// </summary>
        /// <param name="keyInput"></param>
        /// <returns></returns>
        bool CanProcess(VimKeyInput keyInput);
        /// <summary>
        /// to process editor's keydown
        /// </summary>
        /// <param name="args"></param>
        void KeyDown(VimKeyEventArgs args);

        void Beep();
        /// <summary>
        /// update editor's status bar
        /// </summary>
        /// <param name="text"></param>
        void UpdateStatus(string text);
        /// <summary>
        /// dismiss popup windows in editor, such compeletion popup windows
        /// </summary>
        void DismissDisplayWindows();

        void Undo();
        void Redo();

        /// <summary>
        /// return '\0' at the end of document
        /// </summary>
        /// <returns></returns>
        char GetCharAtCurrentPosition();
        /// <summary>
        /// get characher at specified position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        char GetChar(VimPoint pos);
        /// <summary>
        /// if on white space, return that single char
        /// </summary>
        /// <returns></returns>
        string GetWordAtCurrentPosition();
        /// <summary>
        /// get text between span
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        string GetText(VimSpan span);

        /// <summary>
        /// "[{"
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool FindLeftBrace(VimPoint startPosition, out VimPoint pos);
        /// <summary>
        /// "]}"
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool FindRightBrace(VimPoint startPosition, out VimPoint pos);

        /// <summary>
        /// search word
        /// </summary>
        /// <param name="word"></param>
        /// <param name="wholeWord"></param>
        /// <returns></returns>
        bool FindNextWord(string word, bool wholeWord);
        /// <summary>
        /// search word backward
        /// </summary>
        /// <param name="word"></param>
        /// <param name="wholeWord"></param>
        /// <returns></returns>
        bool FindPreviousWord(string word, bool wholeWord);

        /// <summary>
        /// get position of the end of specifiled line
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        VimPoint GetLineEndPosition(int lineNumber);

        bool IsCurrentPositionAtStartOfDocument();
        bool IsCurrentPositionAtEndOfDocument();

        bool IsCurrentPositionAtFirstLine();
        bool IsCurrentPositionAtLastLine();

        bool IsCurrentPositionAtStartOfLine();
        bool IsCurrentPositionAtStartOfLineText();
        bool IsCurrentPositionAtEndOfLine();

        /// <summary>
        /// move editor's cursor to specified position
        /// </summary>
        /// <param name="pos"></param>
        void MoveCursor(VimPoint pos);

        /// <summary>
        /// move cursor left
        /// </summary>
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
