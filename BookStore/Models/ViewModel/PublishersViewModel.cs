using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class PublishersViewModel
    {
        public Publisher Publisher { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public List<Publisher> Publishers { get; set; }
        public string TriggerError { get; set; }
        public string PermissionError { get; set; }
    }
}
