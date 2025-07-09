using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreManagementSystem.Models
{
    public class Invoice
    {
        public string InvoiceID { get; set; }
        public string CustomerID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}