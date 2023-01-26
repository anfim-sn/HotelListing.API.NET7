using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Hotel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
  private readonly IHotelsRepository _hotelsRepository;
  private readonly IMapper _mapper;

  public HotelsController(IHotelsRepository hotelsRepository, IMapper mapper)
  {
    _hotelsRepository = hotelsRepository;
    _mapper = mapper;
  }

  // GET: api/Hotels
  [HttpGet]
  public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
  {
    var hotels = await _hotelsRepository.GetAllAsync();
    var getHotels = _mapper.Map<List<HotelDto>>(hotels);

    return Ok(getHotels);
  }

  // GET: api/Hotels/5
  [HttpGet("{id}")]
  public async Task<ActionResult<HotelDto>> GetHotel(int id)
  {
    var hotel = await _hotelsRepository.GetAsync(id);

    if (hotel == null) return NotFound();

    var hotelDto = _mapper.Map<HotelDto>(hotel);

    return hotelDto;
  }

  // PUT: api/Hotels/5
  [HttpPut("{id}")]
  [Authorize]
  public async Task<IActionResult> PutHotel(
    int id,
    HotelDto hotelDto
  )
  {
    if (id != hotelDto.Id) return BadRequest("Invalid Record Id");

    var hotel = await _hotelsRepository.GetAsync(id); // tracked

    if (hotel == null) return NotFound();

    _mapper.Map(hotelDto, hotel);

    try
    {
      await _hotelsRepository.UpdateAsync(hotel); // change entity
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
    var hotel = _mapper.Map<Hotel>(createHotel);

    await _hotelsRepository.AddAsync(hotel);

    return CreatedAtAction("GetHotel", new { id = hotel.Id }, hotel);
  }

  // DELETE: api/Hotels/5
  [HttpDelete("{id}")]
  [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> DeleteHotel(int id)
  {
    var hotel = await _hotelsRepository.GetAsync(id);
    if (hotel == null) return NotFound();

    await _hotelsRepository.DeleteAsync(id);

    return NoContent();
  }

  private async Task<bool> HotelExists(int id)
  {
    return await _hotelsRepository.Exists(id);
  }
}
