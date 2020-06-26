using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class ShippersViewModel
    {
        public Shipper Shipper { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public List<Shipper> Shippers { get; set; }
        public string TriggerError { get; set; }
        public string PermissionError { get; set; }
    }
}
