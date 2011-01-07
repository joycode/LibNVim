using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Editions
{
    class EditionDeleteRange : AbstractVimEditionRedoable, IVimRangeEdition
    {
        public IVimMotion Motion { get; private set; }

        public EditionDeleteRange(IVimHost host, int repeat, IVimMotion motion)
            : base(host, repeat)
        {
            this.Motion = motion;
        }

        public override bool Apply()
        {
            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = null;
            for (int i = 0; i < this.Repeat; i++) {
                to = this.Motion.Move();
            }

            VimSpan span = null;
            if (from.CompareTo(to) > 0) {
                span = new VimSpan(to, from);
            }
            else {
                span = new VimSpan(from, to);

                if (this.Motion is Interfaces.IVimMotionNextWord) {
                    // "dw" on the last word of the line deletes to the end of the line
                    if (this.Host.IsCurrentPositionAtStartOfLineText()) {
                        this.Host.CaretUp();
                        this.Host.MoveToEndOfLine();
                        span = new VimSpan(from, this.Host.CurrentPosition);
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
            }

            if (this.Motion is IVimMotionBetweenLines) {
                this.Host.DeleteLineRange(span);
            }
            else {
                this.Host.DeleteRange(span);
            }

            if (this.Host.IsCurrentPositionAtEndOfLine()) {
                this.Host.CaretLeft();
            }

            return true;
        }
    }
}
