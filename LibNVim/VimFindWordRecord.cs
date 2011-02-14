using System;
using System.Collections.Generic;
using System.Text;

namespace LibNVim
{
    public class VimFindWordRecord
    {
        [Flags]
        public enum FindOptions
        {
            None = 0,
            WholeWord = 1,
            UserRegex = 2,
        }

        public string Word { get; private set; }
        public FindOptions Options { get; private set; }

        public VimFindWordRecord(string word, FindOptions options)
        {
            this.Word = word;
            this.Options = options;
        }
    }
}
