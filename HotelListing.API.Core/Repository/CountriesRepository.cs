using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Exceptions;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Core.Repository;

public class CountriesRepository : GenericRepository<Country>,
  ICountriesRepository
{
  private readonly IMapper _mapper;
  public HotelListingDbContext _context;

  public CountriesRepository(HotelListingDbContext context, IMapper mapper) :
    base(context, mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  public async Task<CountryDto> GetDetails(int id)
  {
    var country = await _context.Countries.Include(c => c.Hotels)
      .ProjectTo<CountryDto>(_mapper.ConfigurationProvider)
      .FirstOrDefaultAsync(c => c.Id == id);

    if (country == null) throw new NotFoundException(nameof(GetDetails), id);

    return country;
  }
}
