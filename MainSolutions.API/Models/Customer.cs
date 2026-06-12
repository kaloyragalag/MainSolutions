using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MainSolutions.API.Models;

public class Customer : BaseEntity
{


    // FK to User
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]

    [JsonIgnore]
    public User? User { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Gender { get; set; } // Male, Female, Other

    public DateTime? BirthDate { get; set; }

    [MaxLength(200)]
    public string? StreetAddress { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? MaritalStatus { get; set; } // Single, Married, etc.
}