using HotelListing.API.Models.Hotel;

namespace HotelListing.API.Models.Country;

public class CountryDto : BaseCountryDto
{
  public int Id { get; set; }

  //[Required] string Name

  //string ShortName

  public List<HotelDto> Hotels { get; set; }
}
