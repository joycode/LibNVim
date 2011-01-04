using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Editions
{
    class EditionDeleteRange : AbstractVimRangeEdition
    {
        public EditionDeleteRange(IVimHost host, int repeat, IVimMotion motion)
            : base(host, repeat, motion)
        {
        }

        public override bool Apply()
        {
            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = this.Motion.Move();

            if (this.Motion is Interfaces.IVimMotionWordWise) {
                // "dw" on the last word of the line equals to "de"
                if (this.Host.IsCurrentPositionAtStartOfLineText()) {
                    this.Host.CaretUp();
                    this.Host.MoveToEndOfLine();
                    to = this.Host.CurrentPosition;
                }
            }
            else if (this.Motion is Interfaces.IVimMotionEndOfLine) {
                // '$' in range edition is a close end span while range edition operates on open end spans,
                // so a manual compensation is needed
                this.Host.CaretRight();
                to = this.Host.CurrentPosition;
            }

            for (int i = 0; i < this.Repeat; i++) {
                if (this.Motion is IVimMotionBetweenLines) {
                    this.Host.DeleteLineRange(from, to);
                }
                else {
                    this.Host.DeleteRange(from, to);
                }
            }

            if (this.Host.IsCurrentPositionAtEndOfLine())
            {
                this.Host.CaretLeft();
            }

            return true;
        }
    }
}
