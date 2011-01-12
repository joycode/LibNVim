using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    /// <summary>
    /// '*'
    /// </summary>
    class MotionGotoWordFindStar : AbstractVimMotion
    {
        public MotionGotoWordFindStar(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            string word = this.Host.GetWordAtCurrentPosition();
            if (string.IsNullOrWhiteSpace(word)) {
                this.Host.UpdateStatus("Error: No string unser cursor");
                return this.Host.CurrentPosition;
            }

            VimGlobalInfo.IncrementalSearchWord = word;

            for (int i = 0; i < this.Repeat; i++) {
                this.Host.FindNextWord(word);
            }

            return this.Host.CurrentPosition;
        }
    }
}
