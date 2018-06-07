using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi.DTO
{
    public class StaffMemberForUpdateDTO
    {
        [Required(ErrorMessage = "Hame is required")]
        [MaxLength(50, ErrorMessage = "Name cannot be longer than 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [MaxLength(20, ErrorMessage = "Phone Number cannot be longer than 20 characters")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(50, ErrorMessage = "Address cannot be longer than 50 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "A supermarket Id is required")]
        public int SupermarketId { get; set; }
    }
}
