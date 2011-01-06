using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim
{
    /// <summary>
    /// X stands line, Y stands column
    /// </summary>
    public class VimPoint : IComparable<VimPoint>
    {
        public int X { get; set; }
        public int Y { get; set; }

        public VimPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public VimPoint()
            : this(0, 0)
        {
        }

        public int CompareTo(VimPoint other)
        {
            if (this.X < other.X) {
                return -1;
            }
            else if (this.X == other.X) {
                if (this.Y < other.Y) {
                    return -1;
                }
                else if (this.Y == other.Y) {
                    return 0;
                }
                else {
                    return 1;
                }
            }
            else {
                return 1;
            }
        }
    }
}
