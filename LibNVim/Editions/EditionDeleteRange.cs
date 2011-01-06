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

            VimSpan span = new VimSpan(from, true, to, false);

            if (this.Motion is Interfaces.IVimMotionNextWord) {
                // "dw" on the last word of the line equals to "de"
                if (this.Host.IsCurrentPositionAtStartOfLineText()) {
                    this.Host.CaretUp();
                    this.Host.MoveToEndOfLine();
                    span = new VimSpan(from, true, this.Host.CurrentPosition, false);
                }
            }
            else if (this.Motion is Interfaces.IVimMotionEndOfWord) {
                span = span.GetClosedEnd();
            }
            else if (this.Motion is Interfaces.IVimMotionEndOfLine) {
                // '$' in range edition is a close end span while range edition operates on open end spans,
                // so a manual compensation is needed
                span = span.GetClosedEnd();
            }

            for (int i = 0; i < this.Repeat; i++) {
                if (this.Motion is IVimMotionBetweenLines) {
                    this.Host.DeleteLineRange(span.Start, span.End);
                }
                else {
                    this.Host.DeleteRange(span);
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
