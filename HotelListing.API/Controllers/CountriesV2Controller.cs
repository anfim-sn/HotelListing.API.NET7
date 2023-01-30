using AutoMapper;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Exceptions;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Controllers;

[Route("api/v{version:apiVersion}/countries")]
[ApiController]
[ApiVersion("2.0")]
public class CountriesV2Controller : ControllerBase
{
  private readonly ICountriesRepository _countriesRepository;
  private readonly ILogger<CountriesV2Controller> _logger;
  private readonly IMapper _mapper;

  public CountriesV2Controller(
    ICountriesRepository countriesRepository,
    IMapper mapper,
    ILogger<CountriesV2Controller> logger
  )
  {
    _countriesRepository = countriesRepository;
    _mapper = mapper;
    _logger = logger;
  }

  // GET: api/Countries
  [HttpGet]
  [EnableQuery]
  public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
  {
    _logger.LogInformation($"Action: {nameof(GetCountries)}");

    var countries = await _countriesRepository.GetAllAsync();

    var getCountries = _mapper.Map<List<GetCountryDto>>(countries);

    return Ok(getCountries);
  }

  // GET: api/Countries/5
  [HttpGet("{id}")]
  public async Task<ActionResult<CountryDto>> GetCountry(int id)
  {
    _logger.LogInformation($"Action: {nameof(GetCountry)} by id: {id}");

    var country = await _countriesRepository.GetDetails(id);

    if (country == null) throw new NotFoundException(nameof(GetCountry), id);

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
    _logger.LogInformation($"Action: {nameof(PutCountry)} with id: {id}");

    if (id != updateCountryDto.Id) return BadRequest("Invalid Record Id");

    var country = await _countriesRepository.GetAsync(id); // tracked

    if (country == null)
      throw new NotFoundException(nameof(PutCountry), id);

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
    _logger.LogInformation(
      $"Action: {nameof(PostCountry)} with name: {createCountry.Name}"
    );

    var country = _mapper.Map<Country>(createCountry);

    await _countriesRepository.AddAsync(country);

    return CreatedAtAction("GetCountry", new { id = country.Id }, country);
  }

  // DELETE: api/Countries/5
  [HttpDelete("{id}")]
  [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> DeleteCountry(int id)
  {
    _logger.LogInformation($"Action: {nameof(DeleteCountry)} with id: {id}");

    var country = await _countriesRepository.GetAsync(id);
    if (country == null)
      throw new NotFoundException(nameof(DeleteCountry), id);

    await _countriesRepository.DeleteAsync(id);

    return NoContent();
  }

  private async Task<bool> CountryExists(int id)
  {
    return await _countriesRepository.Exists(id);
  }
}
