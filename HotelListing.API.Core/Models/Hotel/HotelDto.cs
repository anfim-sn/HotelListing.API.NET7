namespace HotelListing.API.Core.Models.Hotel;

public class HotelDto : BaseHotelDto
{
  public int Id { get; set; }

  //[Required] string Name

  //[Required] string Address

  //double? Rating

  //[Required] [Range(1, int.MaxValue)] int CountryID
}
