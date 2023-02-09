using System;
using System.ComponentModel.DataAnnotations.Schema;
using Hotel.Db.Rooms;

namespace Hotel.Db.Orders;

public class Order
{
    public int Id { get; set; }
    public DateTime From { get; set; }
    public DateTime Until { get; set; }
    public Room Room { get; set; }
    public string Username { get; set; }
    public bool IsVip { get; set; }
}
