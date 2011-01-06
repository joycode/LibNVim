using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionChangeRange : AbstractVimRangeEdition
    {
        public EditionChangeRange(Interfaces.IVimHost host, int repeat, Interfaces.IVimMotion motion)
            : base(host, repeat, motion)
        {
        }

        public override bool Apply()
        {
            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = this.Motion.Move();

            VimSpan span = new VimSpan(from, true, to, false);

            if (this.Motion is Interfaces.IVimMotionNextWord) {
                // 'w' equals to 'e' here
                this.Host.MoveToPreviousWord();
                this.Host.MoveToEndOfWord();
                span = new VimSpan(from, true, this.Host.CurrentPosition, true);
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
                if (this.Motion is Interfaces.IVimMotionBetweenLines) {
                    this.Host.DeleteLineRange(span.Start, span.End);
                }
                else {
                    this.Host.DeleteRange(span);
                }
            }

            Modes.ModeInsert mode = new Modes.ModeInsert(this.Host, this.Host.CurrentMode, this);
            this.Host.CurrentMode = mode;
            return true;
        }
    }
}
