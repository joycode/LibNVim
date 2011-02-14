using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    class MotionGotoWordFindPrevious : AbstractVimMotion
    {
        public MotionGotoWordFindPrevious(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            if (VimGlobalInfo.FindWordRecord == null) {
                host.UpdateStatus("Error: No word to search");
                return host.CurrentPosition;
            }

            for (int i = 0; i < this.Repeat; i++) {
                host.FindPreviousWord(VimGlobalInfo.FindWordRecord);
            }

            return host.CurrentPosition;
        }
    }
}
