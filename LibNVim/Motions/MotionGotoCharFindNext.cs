using System;
using System.Collections.Generic;

using System.Text;
using System.Diagnostics;

namespace LibNVim.Motions
{
    /// <summary>
    /// 'f'
    /// </summary>
    class MotionGotoCharFindNext : AbstractVimMotion, Interfaces.IVimMotionSearchCharInLine
    {
        private char _toSearch = '\0';

        public MotionGotoCharFindNext(char toSearch, Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
            _toSearch = toSearch;
        }

        public override VimPoint Move()
        {
            if (this.Host.IsCurrentPositionAtEndOfLine()) {
                return this.Host.CurrentPosition;
            }

            VimPoint bak = this.Host.CurrentPosition;

            for (int i = 0; i < this.Repeat; i++) {
                do {
                    if (this.Host.IsCurrentPositionAtEndOfLine()) {
                        this.Host.MoveCursor(bak);
                        return this.Host.CurrentPosition;
                    }

                    this.Host.CaretRight();
                    char ch = this.Host.GetCharAtCurrentPosition();

                    if (ch == _toSearch) {
                        break;
                    }
                }
                while (true);
            }

            return this.Host.CurrentPosition;
        }
    }
}
