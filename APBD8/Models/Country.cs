using System.ComponentModel.DataAnnotations;

namespace APBD8.Models;

public class Country
{
    public int IdCountry { get; set; }
    [MaxLength(120)]
    public string Name { get; set; }
}