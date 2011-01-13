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
        public override VimPoint Move()
        {
            for (int i = 0; i < this.Repeat; i++) {
                this.Host.CaretRight();
            }

            if (this.Host.IsCurrentPositionAtEndOfLine())
            {
                // if at the end of the line, move back one cursor
                this.Host.CaretLeft();
            }

            return this.Host.CurrentPosition;
        }
    }
}
