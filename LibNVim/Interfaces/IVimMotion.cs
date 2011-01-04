using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Interfaces
{
    interface IVimMotion : IVimAction
    {
        /// <summary>
        /// 常规状态下的移动
        /// </summary>
        /// <returns></returns>
        VimPoint Move();
        /// <summary>
        /// 有些移动, 比如 'w', 范围编辑时与常规 Move() 行为不一致, 所以额外该移动功能
        /// </summary>
        /// <returns></returns>
        VimPoint MoveInRangeEdition();
        /// <summary>
        /// 只测试移动后的坐标, 不做实际移动
        /// </summary>
        /// <returns></returns>
        VimPoint TestMove();
    }
}
