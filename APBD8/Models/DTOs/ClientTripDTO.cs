﻿using System.ComponentModel.DataAnnotations;

namespace APBD8.Models.DTOs;

public class ClientTripDTO
{
    public int IdClient { get; set; } 
    public int IdTrip { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
    [MaxLength(120)]
    public string TripName { get; set; }
    [MaxLength(220)]
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
}