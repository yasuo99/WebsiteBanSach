
﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectListItem1<T>(this IEnumerable<T> items, int selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("SecondName"),
                       Value = item.GetPropertyValue("ID"),
                       Selected = item.GetPropertyValue("ID").Equals(selectedValue.ToString())
                   };
        }
        public static IEnumerable<SelectListItem> ToSelectListItem2<T>(this IEnumerable<T> items, int selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Code"),
                       Value = item.GetPropertyValue("Code"),
                       Selected = item.GetPropertyValue("Code").Equals(selectedValue.ToString())
                   };
        }
        public static IEnumerable<SelectListItem> ToSelectListItem3<T>(this IEnumerable<T> items, int selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Vehicle_Category_Description"),
                       Value = item.GetPropertyValue("Vehicle_Category_ID"),
                       Selected = item.GetPropertyValue("Vehicle_Category_ID").Equals(selectedValue.ToString())
                   };
        }
        public static IEnumerable<SelectListItem> ToSelectListItem4<T>(this IEnumerable<T> items, int selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Cars_for_Sale_ID"),
                       Value = item.GetPropertyValue("Cars_for_Sale_ID"),
                       Selected = item.GetPropertyValue("Cars_for_Sale_ID").Equals(selectedValue.ToString())
                   };
        }
        public static IEnumerable<SelectListItem> ToSelectListItem5<T>(this IEnumerable<T> items, int selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Customers_ID"),
                       Value = item.GetPropertyValue("Customers_ID"),
                       Selected = item.GetPropertyValue("Customers_ID").Equals(selectedValue.ToString())
                   };
        }
        public static IEnumerable<SelectListItem> ToSelectListItem7<T>(this IEnumerable<T> items, int selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Cars_Sold_ID"),
                       Value = item.GetPropertyValue("Cars_Sold_ID"),
                       Selected = item.GetPropertyValue("Cars_Sold_ID").Equals(selectedValue.ToString())
                   };
        }

        public static IEnumerable<SelectListItem> ToSelectListItemString<T>(this IEnumerable<T> items, string selectedValue)
        {
            if (selectedValue == null)
            {
                selectedValue = "";
            }
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Name"),
                       Value = item.GetPropertyValue("Id"),
                       Selected = item.GetPropertyValue("Id").Equals(selectedValue.ToString())
                   };
        }
    }
}