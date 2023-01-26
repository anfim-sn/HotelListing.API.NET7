namespace HotelListing.API.Models.Country;

public class CreateCountryDto : BaseCountryDto { }

public class UpdateCountryDto : BaseCountryDto
{
  public int Id { get; set; }

  //[Required] string Name

  //string ShortName
}
