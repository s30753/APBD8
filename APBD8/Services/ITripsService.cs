using APBD8.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace APBD8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
}