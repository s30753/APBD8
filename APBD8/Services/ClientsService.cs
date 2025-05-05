using APBD8.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;" +
        "Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

    public async Task<List<ClientTripDTO>> GetTrips(int IdClient)
    {

        List<ClientTripDTO> clientTrips = new List<ClientTripDTO>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            // checking client in Client table
            string command = "SELECT TOP 1 * FROM Client WHERE IdClient = @IdClient";

            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", IdClient);

                var existingClient = await cmd.ExecuteScalarAsync();
                if (existingClient == null) return null;
            }

            // checking client in Client_Trip table
            command = "SELECT TOP 1 * FROM Client_Trip WHERE IdClient = @IdClient";

            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", IdClient);

                var existingClient = await cmd.ExecuteScalarAsync();
                if (existingClient == null) return clientTrips;
            }

            //retrieving trips for the client
            command = @"
SELECT ct.IdClient, ct.IdTrip, ct.RegisteredAt, ct.PaymentDate, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople
FROM Client_Trip ct JOIN Trip t ON ct.IdTrip = t.IdTrip";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {

                cmd.Parameters.AddWithValue("@IdClient", IdClient);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        clientTrips.Add(new ClientTripDTO()
                        {
                            IdClient = reader.GetInt32(0),
                            IdTrip = reader.GetInt32(1),
                            RegisteredAt = reader.GetInt32(2),
                            PaymentDate = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            TripName = reader.GetString(4),
                            Description = reader.GetString(5),
                            DateFrom = reader.GetDateTime(6),
                            DateTo = reader.GetDateTime(7),
                            MaxPeople = reader.GetInt32(8)

                        });
                    }

                    return clientTrips;
                }
            }

        }
    }
}