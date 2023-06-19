using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeApp.Models
{
    public class ShoppingCard
    {
        public ShoppingCard()
        {
            Count = 1;
        }

        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } //Checck This Type

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public int Count { get; set; }

        [NotMapped]
        public double Price { get; set; }

    }
}
