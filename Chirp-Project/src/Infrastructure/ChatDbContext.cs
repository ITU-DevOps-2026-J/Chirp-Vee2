using Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure;


/// <summary>
/// Creates our entity framework for our database
/// </summary>
public class ChatDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// These DbSets represents the collection of all entities in the context, 
    /// or that can be queried from the database, of a given type. 
    /// DbSet objects are created from a DbContext using the DbContext.Set method.
    /// </summary>
    public DbSet<Cheep> Cheeps { get; set; }

    public DbSet<Author> Authors { get; set; }
    
    public DbSet<Latest> Latests { get; set; }

    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var listComparer = new ValueComparer<List<int>>(
            (a, b) => (a ?? new List<int>()).SequenceEqual(b ?? new List<int>()),
            v => (v ?? new List<int>()).Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
            v => v == null ? new List<int>() : v.ToList());

        modelBuilder.Entity<Author>()
            .Property(a => a.Follows)
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(listComparer);

        modelBuilder.Entity<Author>()
            .Property(a => a.CheepLikes)
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(listComparer);
        
    }
}