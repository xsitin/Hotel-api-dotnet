using Hotel.Db.Orders;
using Hotel.Db.Rooms;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Db;

public class RoomsContext : DbContext
{
    public RoomsContext(DbContextOptions<RoomsContext> contextOptions) : base(contextOptions)
    {
    }

    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RoomEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
    }
}
