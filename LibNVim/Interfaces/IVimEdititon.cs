using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Interfaces
{
    public interface IVimEdititon : IVimAction
    {
        bool ModeSwitched { get; }
        IVimMode NewMode { get; }

        bool Apply();
    }
}
