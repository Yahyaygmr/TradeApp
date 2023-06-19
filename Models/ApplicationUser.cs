using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        public string Adress { get; set; }
        public string City { get; set; }
        public string District { get; set; } //semt
        public string PostCode { get; set; } //posta kodu

        [NotMapped]
        public string Role { get; set; }


    }
}
