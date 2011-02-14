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

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            VimGlobalInfo.FindWordRecord = new VimFindWordRecord(_wordToSearch, VimFindWordRecord.FindOptions.UserRegex);
  
            host.FindNextWord(VimGlobalInfo.FindWordRecord);

            return host.CurrentPosition;
        }
    }
}
