using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreManagementSystem.Models
{
    public class InvoiceDetail
    {
        public int InvoiceDetailID { get; set; }
        public string InvoiceID { get; set; }
        public string ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}