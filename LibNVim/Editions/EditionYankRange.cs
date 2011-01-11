using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionYankRange : AbstractVimEdition, Interfaces.IVimRangeEdition
    {
        private VimRegister _register = null;

        public Interfaces.IVimMotion Motion { get; private set; }

        public EditionYankRange(VimRegister register, Interfaces.IVimMotion motion, Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
            _register = register;
            this.Motion = motion;
        }

        public override bool Apply()
        {
            // in yank, we need not to move cursor to new moved position
            VimPoint bak = this.Host.CurrentPosition;

            VimPoint from = bak;
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
                else if (this.Motion is Interfaces.IVimMotionSearchCharInLine) {
                    span = span.GetClosedEnd();
                }
            }

            if (this.Motion is Interfaces.IVimMotionSearchCharInLine) {
                if (span.Start.CompareTo(span.End) == 0) {
                    // may be search successfully, but most of time, it reflects a failure, so do nothing for simplification
                    // TODO can find a better way to handle this class of motion failure?
                    return false;
                }
            }

            this.Host.MoveCursor(bak);

            if (this.Motion is Interfaces.IVimMotionBetweenLines) {
                from = new VimPoint(span.Start.X, 0);
                to = this.Host.GetLineEndPosition(span.End.X);
                span = new VimSpan(from, to);
                
                _register.Remember(this.Host.GetText(span), true);
            }
            else {
                _register.Remember(this.Host.GetText(span), false);
            }

            return true;
        }
    }
}
