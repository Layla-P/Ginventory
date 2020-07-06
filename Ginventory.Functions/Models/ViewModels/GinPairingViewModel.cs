using System.Collections.Generic;

namespace Ginventory.Functions.Models.ViewModels
{
    public class GinPairingViewModel
    {
        public string GinName { get; set; }
        public List<string> Botanicals { get; set; } = new List<string>();
        public List<string> Tonics { get; set; } = new List<string>();
    }
}