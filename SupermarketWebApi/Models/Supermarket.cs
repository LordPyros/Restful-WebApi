using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupermarketWebApi.Models
{
    public class Supermarket
    {
        [Key]
        public int SupermarketId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Location { get; set; }
        [Required]
        public int NumberOfStaff { get; set; }
        public ICollection<SupermarketStock> SupermarketStocks { get; set; }
            = new List<SupermarketStock>();
        public ICollection<StaffMember> StaffMembers { get; set; }
            = new List<StaffMember>();
    }
}
