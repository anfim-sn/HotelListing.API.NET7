using AutoMapper;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models;
using HotelListing.API.Core.Models.Country;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Controllers;

[Route("api/v{version:apiVersion}/countries")]
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
public class CountriesController : ControllerBase
{
  private readonly ICountriesRepository _countriesRepository;
  private readonly ILogger<CountriesController> _logger;
  private readonly IMapper _mapper;

  public CountriesController(
    ICountriesRepository countriesRepository,
    IMapper mapper,
    ILogger<CountriesController> logger
  )
  {
    _countriesRepository = countriesRepository;
    _mapper = mapper;
    _logger = logger;
  }

  // GET: api/Countries/GetAll
  [HttpGet("GetAll")]
  [Authorize]
  public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
  {
    _logger.LogInformation($"Action: {nameof(GetCountries)}");

    var countries = await _countriesRepository.GetAllAsync<GetCountryDto>();

    return Ok(countries);
  }

  // GET: api/Countries/?StartIndex=0&PageSize=25&PageNumber=1
  [HttpGet]
  [Authorize]
  public async Task<ActionResult<PagedResult<GetCountryDto>>> GetPagedCountries(
    [FromQuery] QueryParameters queryParameters
  )
  {
    _logger.LogInformation($"Action: {nameof(GetPagedCountries)}");

    var pagedCountriesResult =
      await _countriesRepository.GetAllAsync<GetCountryDto>(queryParameters);

    return Ok(pagedCountriesResult);
  }

  // GET: api/Countries/5
  [HttpGet("{id}")]
  [Authorize]
  public async Task<ActionResult<CountryDto>> GetCountry(int id)
  {
    _logger.LogInformation($"Action: {nameof(GetCountry)} by id: {id}");

    var country = await _countriesRepository.GetDetails(id);

    return Ok(country);
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

    try
    {
      await _countriesRepository.UpdateAsync(id, updateCountryDto);
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

    var country =
      await _countriesRepository.AddAsync<CreateCountryDto, GetCountryDto>(
        createCountry
      );

    return CreatedAtAction(
      nameof(GetCountry),
      new { id = country.Id },
      country
    );
  }

  // DELETE: api/Countries/5
  [HttpDelete("{id}")]
  [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> DeleteCountry(int id)
  {
    _logger.LogInformation($"Action: {nameof(DeleteCountry)} with id: {id}");

    await _countriesRepository.DeleteAsync(id);

    return NoContent();
  }

  private async Task<bool> CountryExists(int id)
  {
    return await _countriesRepository.Exists(id);
  }
}
