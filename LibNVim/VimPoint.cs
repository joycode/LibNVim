using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim
{
    /// <summary>
    /// X 坐标代表行, Y 坐标代表列
    /// </summary>
    public class VimPoint
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
    }
}
