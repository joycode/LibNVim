using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace NVimVS
{
    class VsHostManager
    {

        private static VsHostManager _instance = new VsHostManager();

        private Dictionary<IWpfTextView, VsHost> _hostMap = new Dictionary<IWpfTextView, VsHost>();

        public static VsHostManager Singleton { get { return _instance; } }        

        public Dictionary<IWpfTextView, VsHost> HostMap { get { return _hostMap; } }

        private VsHostManager()
        {
        }

    }
}
