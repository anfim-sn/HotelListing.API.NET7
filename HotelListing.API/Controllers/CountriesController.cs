using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Country;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : ControllerBase
{
  private readonly ICountriesRepository _countriesRepository;
  private readonly IMapper _mapper;

  public CountriesController(
    ICountriesRepository countriesRepository,
    IMapper mapper
  )
  {
    _countriesRepository = countriesRepository;
    _mapper = mapper;
  }

  // GET: api/Countries
  [HttpGet]
  public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
  {
    var countries = await _countriesRepository.GetAllAsync();

    var getCountries = _mapper.Map<List<GetCountryDto>>(countries);

    return Ok(getCountries);
  }

  // GET: api/Countries/5
  [HttpGet("{id}")]
  public async Task<ActionResult<CountryDto>> GetCountry(int id)
  {
    var country = await _countriesRepository.GetDetails(id);

    if (country == null) return NotFound();

    var countryDto = _mapper.Map<CountryDto>(country);

    return Ok(countryDto);
  }

  // PUT: api/Countries/5
  [HttpPut("{id}")]
  [Authorize]
  public async Task<IActionResult> PutCountry(
    int id,
    UpdateCountryDto updateCountryDto
  )
  {
    if (id != updateCountryDto.Id) return BadRequest("Invalid Record Id");

    var country = await _countriesRepository.GetAsync(id); // tracked

    if (country == null) return NotFound();

    _mapper.Map(updateCountryDto, country);

    try
    {
      await _countriesRepository.UpdateAsync(country); // change entity
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!await CountryExists(id)) return NotFound();
      throw;
    }

    return NoContent();
  }

  // POST: api/Countries
  [HttpPost]
  [Authorize]
  public async Task<ActionResult<CreateCountryDto>> PostCountry(
    CreateCountryDto createCountry
  )
  {
    var country = _mapper.Map<Country>(createCountry);

    await _countriesRepository.AddAsync(country);

    return CreatedAtAction("GetCountry", new { id = country.Id }, country);
  }

  // DELETE: api/Countries/5
  [HttpDelete("{id}")]
  [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> DeleteCountry(int id)
  {
    var country = await _countriesRepository.GetAsync(id);
    if (country == null) return NotFound();

    await _countriesRepository.DeleteAsync(id);

    return NoContent();
  }

  private async Task<bool> CountryExists(int id)
  {
    return await _countriesRepository.Exists(id);
  }
}
