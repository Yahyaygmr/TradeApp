using System.ComponentModel.DataAnnotations;

namespace TradeApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
