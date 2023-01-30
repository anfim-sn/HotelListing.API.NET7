using HotelListing.API.Core.Models.Country;
using HotelListing.API.Data;

namespace HotelListing.API.Core.Contracts;

public interface ICountriesRepository : IGenericRepository<Country>
{
  // Specific methods for countries repository
  Task<CountryDto> GetDetails(int id);
}
