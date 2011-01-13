using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    /// <summary>
    /// 'T'
    /// </summary>
    class MotionGotoBeforeCharFindPrevious : AbstractVimMotion, Interfaces.IVimMotionSearchCharInLine
    {
        private char _toSearch = '\0';

        public MotionGotoBeforeCharFindPrevious(char toSearch, Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
            _toSearch = toSearch;
        }

        public override VimPoint Move()
        {
            if (this.Host.IsCurrentPositionAtStartOfLine()) {
                return this.Host.CurrentPosition;
            }

            VimPoint bak = this.Host.CurrentPosition;

            for (int i = 0; i < this.Repeat; i++) {
                do {
                    if (this.Host.IsCurrentPositionAtStartOfLine()) {
                        this.Host.MoveCursor(bak);
                        return this.Host.CurrentPosition;
                    }

                    this.Host.CaretLeft();
                    char ch = this.Host.GetCharAtCurrentPosition();

                    if (ch == _toSearch) {
                        break;
                    }
                }
                while (true);
            }

            this.Host.CaretRight();

            return this.Host.CurrentPosition;
        }
    }
}
