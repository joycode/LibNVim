using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;

namespace VsNVim.VsVim
{
    internal sealed class BlockCaretFactoryService : IBlockCaretFactoryService
    {
        internal const string BlockCaretAdornmentLayerName = "BlockCaretAdornmentLayer";

        private readonly IEditorFormatMapService _formatMapService;

#pragma warning disable 169
        [Export(typeof(AdornmentLayerDefinition))]
        [Name(BlockCaretAdornmentLayerName)]
        [Order(After = PredefinedAdornmentLayers.Selection)]
        private AdornmentLayerDefinition _blockCaretAdornmentLayerDefinition;
#pragma warning restore 169

        [ImportingConstructor]
        internal BlockCaretFactoryService(IEditorFormatMapService formatMapService)
        {
            _formatMapService = formatMapService;
        }

        public IBlockCaret CreateBlockCaret(IWpfTextView textView)
        {
            var formatMap = _formatMapService.GetEditorFormatMap(textView);
            return new BlockCaret(textView, BlockCaretAdornmentLayerName, formatMap);
        }
    }
}
