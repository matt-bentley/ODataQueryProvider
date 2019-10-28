using System;

namespace QueryProvider.Models
{
    public class Invoice
    {
        public int OrderID { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipCountry { get; set; }
        public DateTime OrderDate { get; set; }
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
