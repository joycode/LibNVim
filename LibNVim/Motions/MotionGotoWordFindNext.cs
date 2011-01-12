using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    class MotionGotoWordFindNext : AbstractVimMotion
    {
        public MotionGotoWordFindNext(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            if (VimGlobalInfo.IncrementalSearchWord == null) {
                this.Host.UpdateStatus("Error: No word to search");
                return this.Host.CurrentPosition;
            }

            for (int i = 0; i < this.Repeat; i++) {
                this.Host.FindNextWord(VimGlobalInfo.IncrementalSearchWord, VimGlobalInfo.IsWholeWordSearch);
            }

            return this.Host.CurrentPosition;
        }
    }
}
