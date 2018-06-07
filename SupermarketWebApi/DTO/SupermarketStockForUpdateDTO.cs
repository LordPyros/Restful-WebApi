using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi.DTO
{
    public class SupermarketStockForUpdateDTO
    {
        [Required(ErrorMessage = "A product Id is required")]
        public int SupermarketId { get; set; }

        [Required(ErrorMessage = "A supermarket Id is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Must include number of product in stock")]
        [Range(0, 100000000, ErrorMessage = "Number of product must be between 0 and 100,000,000")]
        public int NumberInStock { get; set; }
    }
}
