using APBD8.Models;
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

            // query checks if a given client exists in Client table
            string command = "SELECT TOP 1 * FROM Client WHERE IdClient = @IdClient";

            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", IdClient);

                var existingClient = await cmd.ExecuteScalarAsync();
                if (existingClient == null) return null;
            }

            // query checks if a given client exists in Client_Trip table
            command = "SELECT TOP 1 * FROM Client_Trip WHERE IdClient = @IdClient";

            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", IdClient);

                var existingClient = await cmd.ExecuteScalarAsync();
                if (existingClient == null) return clientTrips;
            }

            //query retrieves trips for the given client
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

    public async Task<int> AddClientAsync(Client client)
    {
        // the query below inserts a new client to the database, and afterwards returns the last auto-generated value
        // (here it's the client's id)
        string command = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
SELECT SCOPE_IDENTITY();";
        
        using(SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("@LastName", client.LastName);
            cmd.Parameters.AddWithValue("@Email", client.Email);
            cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", client.Pesel);
            
            await conn.OpenAsync();
            
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }
    }

    public async Task<string> AddRegistrationAsync(int IdClient, int IdTrip)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            // query checks whether a client with a given id exists
            string command = "SELECT TOP 1 * FROM Client WHERE IdClient = @IdClient";
            
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", IdClient);

                var existingClient = await cmd.ExecuteScalarAsync();
                if (existingClient == null) return "Client not found";
            }
            
            // query checks whether a trip with a given id exists
            command = "SELECT TOP 1 * FROM Trip WHERE IdTrip = @IdTrip";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdTrip", IdTrip);

                var existingTrip = await cmd.ExecuteScalarAsync();
                if (existingTrip == null) return "Trip not found";
            }
            
            // query checks whether given client has been already registered for a given trip (in such case registration
            //again would violate the primary keys conditions)
            command = "SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", IdClient);
                cmd.Parameters.AddWithValue("@IdTrip", IdTrip);

                var count = (int)await cmd.ExecuteScalarAsync();
                if (count != 0) return "such a trip has been already registered";
            }

            var numOfParticipants = 0;
            // query gets the number of clients already registered on the given trip (number of records with the given
            //trip id in the Client_Trip table)
            command = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdTrip", IdTrip);
                numOfParticipants = (int)await cmd.ExecuteScalarAsync();
                
            }
            
            var maxParticipants = 0;
            command = "SELECT MaxPeople FROM Trip WHERE IdTrip = @IdTrip";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdTrip", IdTrip);
                maxParticipants = (int)(await cmd.ExecuteScalarAsync());
            }
            
            if (numOfParticipants >= maxParticipants) return "Maximum number of participants reached";
            

            command = @"INSERT INTO Client_Trip (IdTrip, IdClient, RegisteredAt)
VALUES (@IdTrip, @IdClient, @RegisteredAt);";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdTrip", IdTrip);
                cmd.Parameters.AddWithValue("@IdClient", IdClient);
                cmd.Parameters.AddWithValue("@RegisteredAt", int.Parse(DateTime.Now.ToString("yyyyMMdd")));
                
                await cmd.ExecuteNonQueryAsync();
                return "Client registered for a trip successfully";
            }
        }
    }

    public async Task<bool> DeleteRegistrationAsync(int IdClient, int IdTrip)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            string command = "SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
            
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", IdClient);
                cmd.Parameters.AddWithValue("@IdTrip", IdTrip);

                var count = (int)await cmd.ExecuteScalarAsync(); 
                if (count != 1) return false;
            }

            command = "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", IdClient);
                cmd.Parameters.AddWithValue("@IdTrip", IdTrip);
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected == 1;
            }
        }
    }
}