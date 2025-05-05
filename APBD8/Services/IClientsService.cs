using APBD8.Models;
using APBD8.Models.DTOs;

namespace APBD8.Services;

public interface IClientsService
{
    Task<List<ClientTripDTO>> GetTrips(int clientId);
    Task<int> AddClientAsync(Client client);
    Task<string> AddRegistrationAsync(int IdClient, int IdTrip);
}