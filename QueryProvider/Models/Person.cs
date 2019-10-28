using System;
using System.Collections.Generic;
using System.Text;

namespace QueryProvider.Models
{
    public class Person
    {
        public Person()
        {
            CreatedDate = DateTime.Now;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public bool IsEmployed { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
