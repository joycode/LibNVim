using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Motions
{
    class MotionCaretRight : AbstractVimMotion
    {
        public MotionCaretRight(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        /// <summary>
        /// move to at most one cursor before the end of the line
        /// </summary>
        /// <returns></returns>
        public override VimPoint Move(IVimHost host)
        {
            for (int i = 0; i < this.Repeat; i++) {
                host.CaretRight();
            }

            if (host.IsCurrentPositionAtEndOfLine())
            {
                // if at the end of the line, move back one cursor
                host.CaretLeft();
            }

            return host.CurrentPosition;
        }
    }
}
