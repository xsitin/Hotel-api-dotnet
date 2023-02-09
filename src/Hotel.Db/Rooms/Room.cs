using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Hotel.Db.Orders;

namespace Hotel.Db.Rooms;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IList<Order> Orders { get; set; }
}
