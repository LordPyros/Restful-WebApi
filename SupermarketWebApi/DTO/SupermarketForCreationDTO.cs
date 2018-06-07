using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi.DTO
{
    public class SupermarketForCreationDTO
    {
        
        [Required(ErrorMessage = "Location is required to create a new supermarket ")]
        [MaxLength(50, ErrorMessage = "Location cannot be more than 50 characters")]
        public string Location { get; set; }

        [Range(0, 10000, ErrorMessage = "NumberOfStaff must be a number between 0 and 10,000")]
        [Required(ErrorMessage = "Cannot create supermarket without including NumberOfStaff") ]
        public int NumberOfStaff { get; set; }

    }
}
