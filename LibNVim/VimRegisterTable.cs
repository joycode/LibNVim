using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim
{
    class VimRegisterTable
    {
        private readonly static string[] Register_Symbols = { "1", "2", "3", "4", "5", "6", "7", "8", "9", 
                                                     "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n",
                                                     "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        private static VimRegisterTable _instance = new VimRegisterTable();

        private Dictionary<string, VimRegister> _symbolRegisters = new Dictionary<string, VimRegister>();

        public static VimRegisterTable Singleton
        {
            get { return _instance; }
        }

        public Dictionary<string, VimRegister> SymbolRegisters
        {
            get { return _symbolRegisters; }
        }

        private VimRegisterTable()
        {
            foreach (string s in Register_Symbols) {
                _symbolRegisters.Add(s, new VimRegister(s));
            }

            _symbolRegisters.Add(VimRegister.DefaultRegister.Name, VimRegister.DefaultRegister);
            _symbolRegisters.Add(VimRegister.SystemRegister.Name, VimRegister.SystemRegister);
        }
    }
}
