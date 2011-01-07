using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
            Debug.Assert(start.CompareTo(end) <= 0);

            this.Start = start;
            this.StartClosed = startClosed;
            this.End = end;
            this.EndClosed = endClosed;
        }

        /// <summary>
        /// default set start closed, end open
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public VimSpan(VimPoint start, VimPoint end)
            : this(start, true, end, false)
        {
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

        public VimSpan ExtendEndTo(VimPoint end, bool endClosed)
        {
            return new VimSpan(this.Start, this.StartClosed, end, endClosed);
        }

        /// <summary>
        /// open end default
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public VimSpan ExtendEndTo(VimPoint end)
        {
            return this.ExtendEndTo(end, false);
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
