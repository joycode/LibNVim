using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    class MotionGotoPreviousBrace : AbstractVimMotion
    {
        public MotionGotoPreviousBrace(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            if (this.Host.IsCurrentPositionAtStartOfDocument()) {
                return this.Host.CurrentPosition;
            }

            VimPoint bak = this.Host.CurrentPosition;

            for (int i = 0; i < this.Repeat; i++) {
                int depth = 1;

                do {
                    if (this.Host.IsCurrentPositionAtStartOfDocument()) {
                        this.Host.MoveCursor(bak);
                        return this.Host.CurrentPosition;
                    }

                    this.Host.MoveToPreviousCharacter();
                    char ch = this.Host.GetCharAtCurrentPosition();

                    if (ch == '}') {
                        depth++;
                    }
                    else if (ch == '{') {
                        depth--;
                        if (depth == 0) {
                            break;
                        }
                    }
                }
                while (true);
            }

            return this.Host.CurrentPosition;
        }
    }
}
