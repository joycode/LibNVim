using System;
using System.Collections.Generic;

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

        public override bool Apply(Interfaces.IVimHost host)
        {
            VimPoint from = host.CurrentPosition;
            VimPoint to = null;
            for (int i = 0; i < this.Repeat; i++) {
                to = this.Motion.Move(host);
            }

            VimSpan span = null;
            if (from.CompareTo(to) > 0) {
                span = new VimSpan(to, from);
            }
            else {
                span = new VimSpan(from, to);

                if (this.Motion is Interfaces.IVimMotionNextWord) {
                    if (host.IsCurrentPositionAtStartOfLineText()) {
                        if (from.X < host.CurrentPosition.X) {
                            // "dw" on the last word of the line deletes to the end of the line
                            host.CaretUp();
                            host.MoveToEndOfLine();
                            if (from.CompareTo(host.CurrentPosition) >= 0) {
                                // delete action begins at the line end
                                // adjust the cursor to one left char before the end of the line
                                host.CaretLeft();
                                return true;
                            }

                            span = new VimSpan(from, host.CurrentPosition);
                        }
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

            if (this.Motion is IVimMotionBetweenLines) {
                VimRegister.YankLineToDefaultRegister(host, span);
                host.DeleteLineRange(span);
            }
            else {
                VimRegister.YankRangeToDefaultRegister(host, span);
                host.DeleteRange(span);
            }

            if (host.IsCurrentPositionAtEndOfLine()) {
                // adjust the cursor to one left char before the end of the line
                host.CaretLeft();
            }

            return true;
        }
    }
}
