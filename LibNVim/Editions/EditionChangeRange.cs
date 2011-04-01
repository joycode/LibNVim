using System;
using System.Collections.Generic;

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

        protected override void OnBeforeInsert(Interfaces.IVimHost host)
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
                    host.MoveToPreviousCharacter();

                    // the start point to test moving back
                    VimPoint anchor_pos = host.CurrentPosition;

                    // last 'w' moves to the end of the word
                    do {
                        host.MoveToPreviousCharacter();
                        if (host.CurrentPosition.X < to.X) {
                            // moving back to previous(above) line, then stop at the end of this line
                            // in case of "\r\n", use MoveToEndOfLine() to safely stop at the right position
                            host.MoveToEndOfLine();
                            break;
                        }

                        // if backing to the "from" point, then we should not move back at all, return to "anchor_pos"
                        if (host.CurrentPosition.CompareTo(from) <= 0) {
                            host.MoveCursor(anchor_pos);
                            break;
                        }

                        char ch = host.GetCharAtCurrentPosition();
                        if (!char.IsWhiteSpace(ch)) {
                            break;
                        }
                    }
                    while (true);

                    span = new VimSpan(from, host.CurrentPosition).GetClosedEnd();
                    if (span.Start.CompareTo(span.End) == 0) {
                        return;
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
                    return;
                }
            }

            if (this.Motion is Interfaces.IVimMotionBetweenLines) {
                VimRegister.YankLineToDefaultRegister(host, span);
                host.DeleteLineRange(span);
            }
            else {
                VimRegister.YankRangeToDefaultRegister(host, span);
                host.DeleteRange(span);
            }
        }
    }
}
