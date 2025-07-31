using Microsoft.EntityFrameworkCore;
using RabbitQueue.WebApi.Models;

namespace RabbitQueue.WebApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Optional: Add any specific model configurations here
        base.OnModelCreating(modelBuilder);
    }
}