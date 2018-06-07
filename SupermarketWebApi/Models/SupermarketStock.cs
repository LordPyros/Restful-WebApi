using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi.Models
{
    public class SupermarketStock
    {
        [Key]
        public int Id { get; set; }
        
        public int SupermarketId { get; set; }

        [ForeignKey("SupermarketId")]
        public Supermarket Supermarket { get; set; }
        
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        public int NumberInStock { get; set; }
    }
}
