using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim
{
    public class VimRegister
    {
        public readonly static VimRegister SystemRegister = new VimRegister("0");
        public readonly static VimRegister DefaultRegister = new VimRegister("+");

        private string _text = null;

        public string Name { get; private set; }
        public string Text
        {
            get
            {
                if (this.IsSystemRegister()) {
                    throw new NotImplementedException();
                }
                else {
                    return _text;
                }
            }
            private set
            {
                if (this.IsSystemRegister()) {
                    throw new NotImplementedException();
                }
                else {
                    _text = value;
                }
            }
        }
        public bool IsTextLines { get; private set; }

        public VimRegister(string name)
        {
            this.Name = name;
        }

        public void Remember(string text, bool isTextLines)
        {
            this.Text = text;
            this.IsTextLines = isTextLines;
        }

        private bool IsSystemRegister()
        {
            return (this == SystemRegister);
        }
    }
}
