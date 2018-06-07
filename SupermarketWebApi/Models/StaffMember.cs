using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi.Models
{
    public class StaffMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(50)]
        public string Address { get; set; }
        
        public int SupermarketId { get; set; }

        [ForeignKey("SupermarketId")]
        public Supermarket Supermarket { get; set; }
    }
}
