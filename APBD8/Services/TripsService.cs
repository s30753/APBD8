using APBD8.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace APBD8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;" +
                                                "Integrated Security=True;Connect Timeout=30;Encrypt=False;" +
                                                "Trust Server Certificate=False;Application Intent=ReadWrite;" +
                                                "Multi Subnet Failover=False";

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        // query gets all trips and information about the country of the trip
        string command = @"
SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.IdCountry, c.Name AS Country 
FROM Trip t JOIN  Country_Trip ct ON t.IdTrip = ct.IdTrip JOIN Country c ON ct.IdCountry = c.IdCountry";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    trips.Add(new TripDTO()
                    {
                        IdTrip = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                        IdCountry = reader.GetInt32(6),
                        Country = reader.GetString(7)
                    });
                }
            }
        }

        return trips;
    }
}