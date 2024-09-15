﻿using ETICARET.Entities;

namespace ETICARET.WebUI.Models
{
    public class ProductListModel
    {
        public PageInfo PageInfo { get; set; }
        public List<Product> Products { get; set; }
        public List<Image>? Images { get; set; }

        public ProductListModel()
        {
            Images = new List<Image>();
        }
    }

    public class PageInfo
    {
        public int TotalItems { get; set; }
        public int ItemPerPage { get; set; }
        public int CurrentPage { get; set; }
        public string CurrentCategory { get; set; }

        public int TotalPages()
        {
            return (int)Math.Ceiling((decimal)TotalItems / ItemPerPage);
        }
    }
}