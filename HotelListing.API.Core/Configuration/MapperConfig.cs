using AutoMapper;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Core.Models.Hotel;
using HotelListing.API.Core.Models.Users;
using HotelListing.API.Data;

namespace HotelListing.API.Core.Configuration;

public class MapperConfig : Profile
{
  public MapperConfig()
  {
    CreateMap<Country, CountryDto>().ReverseMap();
    CreateMap<Country, GetCountryDto>().ReverseMap();
    CreateMap<Country, CreateCountryDto>().ReverseMap();
    CreateMap<Country, UpdateCountryDto>().ReverseMap();

    CreateMap<Hotel, HotelDto>().ReverseMap();
    CreateMap<Hotel, CreateHotelDto>().ReverseMap();

    CreateMap<ApiUserDto, ApiUser>().ReverseMap();
  }
}
