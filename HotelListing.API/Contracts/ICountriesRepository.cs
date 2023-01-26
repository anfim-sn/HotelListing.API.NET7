using HotelListing.API.Data;

namespace HotelListing.API.Contracts;

public interface ICountriesRepository : IGenericRepository<Country>
{
  // Specific methods for countries repository
  Task<Country> GetDetails(int id);
}
