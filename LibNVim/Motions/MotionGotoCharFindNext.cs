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

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            if (host.IsCurrentPositionAtEndOfLine()) {
                return host.CurrentPosition;
            }

            VimPoint bak = host.CurrentPosition;

            for (int i = 0; i < this.Repeat; i++) {
                do {
                    if (host.IsCurrentPositionAtEndOfLine()) {
                        host.MoveCursor(bak);
                        return host.CurrentPosition;
                    }

                    host.CaretRight();
                    char ch = host.GetCharAtCurrentPosition();

                    if (ch == _toSearch) {
                        break;
                    }
                }
                while (true);
            }

            return host.CurrentPosition;
        }
    }
}
