using QueryProvider.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace QueryProvider
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://services.odata.org/V3/Northwind/Northwind.svc/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var oDataClient = new ODataClient<Invoice>(client, "Invoices");
            IQueryable<Invoice> invoices = new ODataQuery<Invoice>(oDataClient);
            
            var query = invoices.Where(i => i.ShipCity != "Berlin")
                                .Skip(5)
                                .Take(2)
                                .OrderBy(i => i.OrderDate)
                                .Select(i => new Person() { FirstName = i.ShipCity });

            //Console.WriteLine("Query:\n{0}\n", query);

            var list = query.ToList();
        }
    }
}
