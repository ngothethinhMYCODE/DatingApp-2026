
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace API.Entities;

public class AppUser: IdentityUser
{
    
    public required string DisplayName {get; set;}
    public string? ImageUrl { get; set; } 
    public string? RefreshToken{get; set;}
    public DateTime? RefreshTokenExpiry{get; set;}
    // Navigation property
    public Member Member { get; set; }=null!;
}
