using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    /// <summary>
    /// '#'
    /// </summary>
    class MotionGotoWordSharp : AbstractVimMotion
    {
        public MotionGotoWordSharp(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            string word = host.GetWordAtCurrentPosition();
            if (Util.StringHelper.IsNullOrWhiteSpace(word)) {
                host.UpdateStatus("Error: No string under cursor");
                return host.CurrentPosition;
            }

            VimGlobalInfo.FindWordRecord = new VimFindWordRecord(word, VimFindWordRecord.FindOptions.WholeWord);

            for (int i = 0; i < this.Repeat; i++) {
                host.FindPreviousWord(VimGlobalInfo.FindWordRecord);
            }

            return host.CurrentPosition;
        }
    }
}
