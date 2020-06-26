﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class DiscountsViewModel
    {
        public Discount Discount { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public List<Discount> Discounts { get; set; }
        public string TriggerError { get; set; }
        public string PermissionError { get; set; }
    }
}
