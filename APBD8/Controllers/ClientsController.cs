using APBD8.Models;
using APBD8.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD8.Controllers;

[Route("api/clients")]
[ApiController]
public class ClientsController : ControllerBase
{

    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }

    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        var clientTrips = await _clientsService.GetTrips(id);
        if (clientTrips == null) return NotFound("Client not found");
        return Ok(clientTrips);
    }

    [HttpPost]
    public async Task<IActionResult> AddTrip([FromBody] Trip trip)
    {
        return Ok(trip);
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> UpdateTrip(int tripId, [FromBody] Trip trip)
    {
        return Ok(trip);
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteTrip(int tripId)
    {
        return Ok();
    }
}