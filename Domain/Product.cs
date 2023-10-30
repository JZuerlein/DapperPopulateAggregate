using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Product
    {
        public Product() { }

        public int ProductId { get; protected set; }
        public string Name { get; protected set; } = string.Empty;
        public decimal Price { get; protected set; }
    }
}
