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

    // gets trips for a given client
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        var clientTrips = await _clientsService.GetTrips(id);
        if (clientTrips == null) return NotFound("Client not found");
        return Ok(clientTrips);
    }

    // inserts new client
    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] Client client)
    {
        var id = await _clientsService.AddClientAsync(client);
        return Ok(id);
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> AddRegistration([FromRoute] int id, [FromRoute] int tripId)
    {
        var msg = await _clientsService.AddRegistrationAsync(id, tripId);
        if (msg == "Client not found" || msg == "Trip not found") return NotFound(msg);
        if (msg == "Maximum number of participants reached" || msg == "such a trip has been already registered") return Conflict(msg);
        return Ok(msg);
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteTrip([FromRoute] int id, [FromRoute] int tripId)
    {
        var successful = await _clientsService.DeleteRegistrationAsync(id, tripId);
        if (!successful) return NotFound("This client has not been registered for this trip");
        return Ok("successfully deleted registration");
    }
}