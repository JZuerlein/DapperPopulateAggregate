using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public interface IReadOnlyProduct
    {
        int ProductId { get; }
        string Name { get; }
        decimal Price { get; }
    }

    public class Product : IReadOnlyProduct
    {
        public Product() { }

        public int ProductId { get; protected set; }
        public string Name { get; protected set; } = string.Empty;
        public decimal Price { get; protected set; }
    }
}
