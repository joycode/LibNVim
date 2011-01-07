using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionFormatRange : AbstractVimEditionRedoable, Interfaces.IVimRangeEdition
    {
        public Interfaces.IVimMotion Motion { get; private set; }

        public EditionFormatRange(Interfaces.IVimHost host, int repeat, Interfaces.IVimMotion motion)
            : base(host, repeat)
        {
            this.Motion = motion;
        }

        public override bool Apply()
        {
            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = this.Motion.Move();

            VimSpan span = null;
            if (from.CompareTo(to) <= 0) {
                span = new VimSpan(from, to);
            }
            else {
                span = new VimSpan(to, from);
            }

            this.Host.FormatLineRange(span);

            return true;
        }
    }
}
