using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionChangeRange : AbstractVimEditionInsertText, Interfaces.IVimRangeEdition
    {
        public Interfaces.IVimMotion Motion { get; private set; }

        public EditionChangeRange(Interfaces.IVimHost host, int repeat, Interfaces.IVimMotion motion)
            : base(host, repeat)
        {
            this.Motion = motion;
        }

        protected override void OnBeforeInsert()
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
                    // last 'w' moves to the end of the word
                    do {
                        this.Host.MoveToPreviousCharacter();
                        char ch = this.Host.GetCharAtCurrentPosition();
                        if (!char.IsWhiteSpace(ch)) {
                            break;
                        }
                    }
                    while (true);

                    span = new VimSpan(from, this.Host.CurrentPosition).GetClosedEnd();
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
                    return;
                }
            }

            if (this.Motion is Interfaces.IVimMotionBetweenLines) {
                VimRegister.YankLineToDefaultRegister(this.Host, span);
                this.Host.DeleteLineRange(span);
            }
            else {
                VimRegister.YankRangeToDefaultRegister(this.Host, span);
                this.Host.DeleteRange(span);
            }
        }
    }
}
