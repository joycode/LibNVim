using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    /// <summary>
    /// 't'
    /// </summary>
    class MotionGotoBeforeCharFindNext : AbstractVimMotion, Interfaces.IVimMotionSearchCharInLine
    {
        private char _toSearch = '\0';

        public MotionGotoBeforeCharFindNext(char toSearch, Interfaces.IVimHost host, int repeat)
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

            host.CaretLeft();

            return host.CurrentPosition;
        }
    }
}
