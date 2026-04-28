using System;
using System.ComponentModel;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace API.Data;

public class AppDBContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Photo> Photos { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dataTimeConverter=new ValueConverter<DateTime, DateTime>(
            v=> v.ToUniversalTime(),
            v=> DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );
        foreach(var entityType in modelBuilder.Model.GetEntityTypes() )
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dataTimeConverter);
                }
            }
        }
    }
}
