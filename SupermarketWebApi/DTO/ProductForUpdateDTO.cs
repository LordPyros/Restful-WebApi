using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi.DTO
{
    public class ProductForUpdateDTO
    {
        [Required(ErrorMessage = "Product Name is required")]
        [MaxLength(50, ErrorMessage = "Name cannot be more than 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Product Price is required")]
        [Range(0.00, 10000, ErrorMessage = "Price must be a number between 0 and 10,000")]
        public double Price { get; set; }
    }
}
