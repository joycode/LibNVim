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

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            if (host.IsCurrentPositionAtStartOfLine()) {
                return host.CurrentPosition;
            }

            VimPoint bak = host.CurrentPosition;

            for (int i = 0; i < this.Repeat; i++) {
                do {
                    if (host.IsCurrentPositionAtStartOfLine()) {
                        host.MoveCursor(bak);
                        return host.CurrentPosition;
                    }

                    host.CaretLeft();
                    char ch = host.GetCharAtCurrentPosition();

                    if (ch == _toSearch) {
                        break;
                    }
                }
                while (true);
            }

            host.CaretRight();

            return host.CurrentPosition;
        }
    }
}
