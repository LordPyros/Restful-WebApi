using System.ComponentModel.DataAnnotations;

namespace SupermarketWebApi.DTO
{
    public class SupermarketForUpdateDTO
    {
        [Required(ErrorMessage = "Location is required to update a supermarket ")]
        [MaxLength(50, ErrorMessage = "Location cannot be more than 50 characters")]
        public string Location { get; set; }

        [Range(0, 10000, ErrorMessage = "NumberOfStaff must be a number between 0 and 10,000")]
        [Required(ErrorMessage = "Cannot update supermarket without including NumberOfStaff")]
        public int NumberOfStaff { get; set; }
    }
}
