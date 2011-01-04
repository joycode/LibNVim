using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Interfaces
{
    /// <summary>
    /// vim 中所有动作的抽象, 包括: 光标移动, 修改, 模式切换
    /// </summary>
    public interface IVimAction
    {
        IVimHost Host { get; }
        int Repeat { get; }
    }
}
