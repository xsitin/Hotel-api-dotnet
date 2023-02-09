using System;
using System.Collections.Generic;
using System.Linq;
using Hotel.Db;
using Hotel.Db.Orders;
using Hotel.Db.Rooms;

namespace Hotel.Api.IntegrationTests.Infrastructure.DataFeeders;

public class RoomsDataFeeder
{
    public static void Feed(RoomsContext context)
    {
        var rooms = new[]
        {
            new Room() { Id = 1, Name = "Rome", Orders = new List<Order>() },
            new Room() { Id = 2, Name = "Moscow", Orders = new List<Order>() },
            new Room() { Id = 3, Name = "Tokyo", Orders = new List<Order>() },
        };
        for (var i = 0; i < rooms.Length; i++)
        {
            var room = rooms[i];
            var order = new Order
            {
                Id = room.Id, From = DateTime.Today, Until = DateTime.Today.AddDays(1), Room = room, Username = "xsitin"
            };
            room.Orders.Add(order);
        }

        context.Rooms.AddRange(rooms);
        context.SaveChanges();
    }
}
