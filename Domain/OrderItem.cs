using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class OrderItem
    {
        public OrderItem () { }
        public OrderItem(Product product, int qty)
        {
            Product = product;
            Qty = qty;
            PriceCharged = product.Price;
        }

        public int OrderItemId { get; protected set; }
        public int OrderId { get; protected set; }
        public IReadOnlyProduct Product { get; protected set; }
        public int Qty { get; protected set; }
        public decimal PriceCharged { get; protected set; }
    }
}
