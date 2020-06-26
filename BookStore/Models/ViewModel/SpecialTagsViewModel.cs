using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class SpecialTagsViewModel
    {
        public SpecialTag SpecialTag { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public List<SpecialTag> SpecialTags { get; set; }
        public string TriggerError { get; set; }
        public string PermissionError { get; set; }
    }
}
