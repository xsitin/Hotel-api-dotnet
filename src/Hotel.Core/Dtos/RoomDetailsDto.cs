using Hotel.Db.Orders;

namespace Hotel.Core.Dtos;

public class RoomDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IList<Order> Orders { get; set; }

}
