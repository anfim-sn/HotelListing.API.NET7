using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Models.Users;

public class ApiUserDto : LoginDto
{
  [Required]
  public string FirstName { get; set; }

  [Required]
  public string LastName { get; set; }

  //[Required] [EmailAddress] string Email

  //[Required] [StringLength(15, ErrorMessage = "Password should be from {2} to {1} characters",MinimumLength = 6)]
  //string Password { get; set; }
}
