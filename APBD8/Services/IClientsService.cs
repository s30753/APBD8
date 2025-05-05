using APBD8.Models.DTOs;

namespace APBD8.Services;

public interface IClientsService
{
    Task<List<ClientTripDTO>> GetTrips(int clientId);
}