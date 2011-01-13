﻿using System;
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

        public override VimPoint Move()
        {
            string word = this.Host.GetWordAtCurrentPosition();
            if (Util.StringHelper.IsNullOrWhiteSpace(word)) {
                this.Host.UpdateStatus("Error: No string unser cursor");
                return this.Host.CurrentPosition;
            }

            VimGlobalInfo.IncrementalSearchWord = word;
            VimGlobalInfo.IsWholeWordSearch = true;

            for (int i = 0; i < this.Repeat; i++) {
                this.Host.FindPreviousWord(word, VimGlobalInfo.IsWholeWordSearch);
            }

            return this.Host.CurrentPosition;
        }
    }
}
