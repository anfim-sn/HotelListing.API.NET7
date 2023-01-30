using AutoMapper;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models;
using HotelListing.API.Core.Models.Hotel;
using HotelListing.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
  private readonly IHotelsRepository _hotelsRepository;
  private readonly ILogger<HotelsController> _logger;
  private readonly IMapper _mapper;

  public HotelsController(
    IHotelsRepository hotelsRepository,
    IMapper mapper,
    ILogger<HotelsController> logger
  )
  {
    _hotelsRepository = hotelsRepository;
    _mapper = mapper;
    _logger = logger;
  }

  // GET: api/Hotels
  [HttpGet("GetAll")]
  public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
  {
    _logger.LogInformation($"Action: {nameof(GetHotels)}");

    var hotels = await _hotelsRepository.GetAllAsync<HotelDto>();

    return Ok(hotels);
  }

  // GET: api/Hotels/?StartIndex=0&PageSize=25&PageNumber=1
  [HttpGet]
  public async Task<ActionResult<PagedResult<HotelDto>>> GetPagedHotels(
    [FromQuery] QueryParameters queryParameters
  )
  {
    _logger.LogInformation($"Action: {nameof(GetPagedHotels)}");

    var pagedHotelsResult =
      await _hotelsRepository.GetAllAsync<HotelDto>(queryParameters);

    return Ok(pagedHotelsResult);
  }

  // GET: api/Hotels/5
  [HttpGet("{id}")]
  public async Task<ActionResult<HotelDto>> GetHotel(int id)
  {
    _logger.LogInformation($"Action: {nameof(GetHotel)} by id: {id}");

    var hotel = await _hotelsRepository.GetAsync<HotelDto>(id);

    return hotel;
  }

  // PUT: api/Hotels/5
  [HttpPut("{id}")]
  [Authorize]
  public async Task<IActionResult> PutHotel(
    int id,
    HotelDto hotelDto
  )
  {
    _logger.LogInformation($"Action: {nameof(PutHotel)} with id: {id}");

    if (id != hotelDto.Id) return BadRequest("Invalid Record Id");

    try
    {
      await _hotelsRepository.UpdateAsync(id, hotelDto);
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!await HotelExists(id))
        return NotFound();
      throw;
    }

    return NoContent();
  }

  // POST: api/Hotels
  [HttpPost]
  [Authorize]
  public async Task<ActionResult<Hotel>> PostHotel(
    CreateHotelDto createHotel
  )
  {
    _logger.LogInformation(
      $"Action: {nameof(PostHotel)} with name: {createHotel.Name}"
    );

    var hotel =
      await _hotelsRepository.AddAsync<CreateHotelDto, HotelDto>(createHotel);

    return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
  }

  // DELETE: api/Hotels/5
  [HttpDelete("{id}")]
  [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> DeleteHotel(int id)
  {
    _logger.LogInformation($"Action: {nameof(DeleteHotel)} with id: {id}");

    await _hotelsRepository.DeleteAsync(id);

    return NoContent();
  }

  private async Task<bool> HotelExists(int id)
  {
    return await _hotelsRepository.Exists(id);
  }
}
