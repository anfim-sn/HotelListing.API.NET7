using HotelListing.API.Core.Models.Hotel;

namespace HotelListing.API.Core.Models.Country;

public class CountryDto : BaseCountryDto
{
  public int Id { get; set; }

  //[Required] string Name

  //string ShortName

  public List<HotelDto> Hotels { get; set; }
}
