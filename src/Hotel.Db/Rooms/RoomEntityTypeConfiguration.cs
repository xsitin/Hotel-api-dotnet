using Hotel.Db.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Db.Rooms;

public class RoomEntityTypeConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(room => room.Id);
        builder.HasIndex(room => room.Id).IsUnique();
        builder.HasMany(room => room.Orders);
    }
}
