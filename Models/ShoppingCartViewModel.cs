using System.Collections.Generic;

namespace TradeApp.Models
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCard> ListCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
