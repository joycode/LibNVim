using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim
{
    public class VimSpan
    {
        public VimPoint Start { get; private set; }
        public bool StartClosed { get; private set; }

        public VimPoint End { get; private set; }
        public bool EndClosed { get; private set; }

        public VimSpan(VimPoint start, bool startClosed, VimPoint end, bool endClosed)
        {
            this.Start = start;
            this.StartClosed = startClosed;
            this.End = end;
            this.EndClosed = endClosed;
        }

        public VimSpan GetClosedStart()
        {
            return new VimSpan(this.Start, true, this.End, this.EndClosed);
        }

        public VimSpan GetOpenStart()
        {
            return new VimSpan(this.Start, false, this.End, this.EndClosed);
        }

        public VimSpan GetClosedEnd()
        {
            return new VimSpan(this.Start, this.StartClosed, this.End, true);
        }

        public VimSpan GetOpenEnd()
        {
            return new VimSpan(this.Start, true, this.End, false);
        }

        public bool Contains(VimPoint pos)
        {
            if (this.StartClosed) {
                if (pos.CompareTo(this.Start) < 0) {
                    return false;
                }
            }
            else {
                if (pos.CompareTo(this.Start) <= 0) {
                    return false;
                }
            }

            if (this.EndClosed) {
                if (pos.CompareTo(this.End) > 0) {
                    return false;
                }
            }
            else {
                if (pos.CompareTo(this.End) >= 0) {
                    return false;
                }
            }

            return true;
        }
    }
}
