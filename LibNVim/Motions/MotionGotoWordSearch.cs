using System;
using System.Collections.Generic;

using System.Text;
using System.Diagnostics;

namespace LibNVim.Motions
{
    class MotionGotoWordSearch : AbstractVimMotion
    {
        private string _wordToSearch = null;

        public MotionGotoWordSearch(string wordToSearch, Interfaces.IVimHost host)
            : base(host, 1)
        {
            Debug.Assert(!Util.StringHelper.IsNullOrWhiteSpace(wordToSearch));
            _wordToSearch = wordToSearch;
        }

        public override VimPoint Move()
        {
            VimGlobalInfo.IncrementalSearchWord = _wordToSearch;
            VimGlobalInfo.IsWholeWordSearch = false;

            this.Host.FindNextWord(_wordToSearch, VimGlobalInfo.IsWholeWordSearch);
            return this.Host.CurrentPosition;
        }
    }
}
