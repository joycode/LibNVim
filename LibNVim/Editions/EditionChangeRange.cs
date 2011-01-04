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

            if (this.Motion is Interfaces.IVimMotionWordWise)
            {
                // 'w' equals to 'e' here
                this.Host.MoveToPreviousWord();
                this.Host.MoveToEndOfWord();
                to = new VimPoint(this.Host.CurrentPosition.X, this.Host.CurrentPosition.Y + 1);
            }
            else if (this.Motion is Interfaces.IVimMotionEndOfLine) {
                // '$' in range edition is a close end span while range edition operates on open end spans,
                // so a manual compensation is needed
                this.Host.CaretRight();
                to = this.Host.CurrentPosition;
            }

            for (int i = 0; i < this.Repeat; i++) {
                if (this.Motion is Interfaces.IVimMotionBetweenLines) {
                    this.Host.DeleteLineRange(from, to);
                }
                else {
                    this.Host.DeleteRange(from, to);
                }
            }

            Modes.ModeInsert mode = new Modes.ModeInsert(this.Host, this.Host.CurrentMode, this);
            this.Host.CurrentMode = mode;
            return true;
        }
    }
}
