using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Classification;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;

namespace VsNVim
{
    [Export(typeof(IKeyProcessorProvider))]
    [Order(Before = "VisualStudioKeyProcessor")]
    [Name("VsNVim")]
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class VsHostFactory : IVsTextViewCreationListener, IKeyProcessorProvider
    {

        [Import]
        private IEditorOperationsFactoryService _editorOperationsFactoryService = null;
        [Import]
        private IEditorFormatMapService _editorFormatMapService = null;
        [Import]
        private IVsEditorAdaptersFactoryService _adaptersFactory = null;
        [Import]
        private SVsServiceProvider _vsServiceProvider = null;
        [Import]
        private ICompletionBroker _completionBroker = null;
        //[Import]
        //private ISignatureHelpBroker _signatureBroker = null;
        //[Import]
        //private ISmartTagBroker _smartTagBroker = null;
        //[Import]
        //private IQuickInfoBroker _quickInfoBroker = null;

        public VsHostFactory()
        {
        }

        /// <summary>
        /// 永不返回 null
        /// </summary>
        /// <param name="wpfTextView"></param>
        /// <returns></returns>
        private VsHost GetOrCreateVimHost(IWpfTextView wpfTextView)
        {
            if (VsHostManager.Singleton.HostMap.ContainsKey(wpfTextView)) {
                return VsHostManager.Singleton.HostMap[wpfTextView];
            }

            IEditorOperations editor_operations = _editorOperationsFactoryService.GetEditorOperations(wpfTextView);
            IBlockCaret block_caret = new BlockCaretFactoryService(_editorFormatMapService).CreateBlockCaret(wpfTextView);

            _DTE dte = (_DTE)_vsServiceProvider.GetService(typeof(_DTE));

            VsHost host = new VsHost(wpfTextView, dte, editor_operations, block_caret, _completionBroker);
            VsHostManager.Singleton.HostMap.Add(wpfTextView, host);

            return host;
        }

        KeyProcessor IKeyProcessorProvider.GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            VsHost host = this.GetOrCreateVimHost(wpfTextView);
            Debug.Assert(host != null);

            return new VsKeyProcessor(host);
        }

        void IVsTextViewCreationListener.VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView wpfTextView = _adaptersFactory.GetWpfTextView(textViewAdapter);
            if (wpfTextView == null) {
                return;
            }

            VsHost host = this.GetOrCreateVimHost(wpfTextView);
            Debug.Assert(host != null);

            new VsKeyProcessorAdditional(host, textViewAdapter, _vsServiceProvider);
        }
    }
}
