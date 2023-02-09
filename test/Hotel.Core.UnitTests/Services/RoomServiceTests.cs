using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hotel.Core.Services;
using Hotel.Db;
using Hotel.Db.Orders;
using Hotel.Db.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hotel.Core.UnitTests.Services
{
    public class RoomServiceTests
    {
        private readonly RoomsContext _context;
        private readonly IRoomService _roomService;
        private readonly Mock<ILogger<RoomService>> _loggerMock;
        private readonly Mock<IUserService> _userServiceMock;

        public RoomServiceTests()
        {
            var options = new DbContextOptionsBuilder<RoomsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new RoomsContext(options);
            _loggerMock = new Mock<ILogger<RoomService>>();
            _userServiceMock = new Mock<IUserService>();
            _roomService = new RoomService(_context, _loggerMock.Object, _userServiceMock.Object);
        }


        private async Task<Room[]> AddRooms()
        {
            var rooms = new[]
            {
                new Room { Id = 1, Name = "101" },
                new Room { Id = 2, Name = "102" },
                new Room { Id = 3, Name = "103" },
            };
            await _context.Rooms.AddRangeAsync(rooms);
            await _context.SaveChangesAsync();
            return rooms;
        }

        private async Task AddOrders(Room[] rooms)
        {
            var orders = new[]
            {
                new Order
                {
                    Id = 1, Room = rooms[0], From = new DateTime(2022, 1, 1), Until = new DateTime(2022, 1, 5)
                },
                new Order
                {
                    Id = 2, Room = rooms[1], From = new DateTime(2022, 2, 1), Until = new DateTime(2022, 2, 5)
                },
            };
            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();
        }

        private async Task<Room[]> AddRoomsAndOrders()
        {
            var rooms = await AddRooms();
            await AddOrders(rooms);
            return rooms;
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRooms()
        {
            // Arrange
            var rooms = await AddRooms();

            // Act
            var result = await _roomService.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equivalent(rooms, result);
        }


        [Fact]
        public async Task GetFreeAsync_ReturnsFreeRooms()
        {
            // Arrange
            var rooms = await AddRoomsAndOrders();

            // Act
            var result = await _roomService.GetFreeAsync(new DateTime(2022, 1, 1), new DateTime(2022, 1, 2),
                CancellationToken.None);

            // Assert
            rooms[1].Orders = null;
            rooms[2].Orders = null;
            Assert.Equal(2, result.Count);
            Assert.Equivalent(new[] { rooms[1], rooms[2] }, result
            );
        }


        [Fact]
        public async Task BookAsync_ReturnsNullForOccupiedRoom()
        {
            // Arrange
            var rooms = await AddRoomsAndOrders();

            _userServiceMock.Setup(x => x.IsVip(It.IsAny<string>())).Returns(true);
            var order = new Order()
            {
                Room = new Room() { Id = 1 }, From = new DateTime(2022, 1, 2), Until = new DateTime(2022, 1, 4)
            };

            // Act
            var result = await _roomService.OrderRoomAsync(order, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task BookAsync_ReturnsOrderForBookFreeRoom()
        {
            // Arrange
            var rooms = await AddRoomsAndOrders();

            _userServiceMock.Setup(x => x.IsVip(It.IsAny<string>()))
                .Returns(true);
            var order = new Order()
            {
                Room = new Room() { Id = 3 }, From = new DateTime(2022, 1, 2), Until = new DateTime(2022, 1, 4)
            };

            // Act
            var result = await _roomService.OrderRoomAsync(order, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var roomOrders = _context.Rooms.First(x => x.Id == 3).Orders;
            Assert.Single(roomOrders);
            Assert.Equal(3, roomOrders.Single().Room.Id);
            Assert.Equal(new DateTime(2022, 1, 2), roomOrders.Single().From);
            Assert.Equal(new DateTime(2022, 1, 4), roomOrders.Single().Until);
        }

        [Fact]
        public async Task DeleteOrderAsync_DeletesOrderSuccessfully()
        {
            // Arrange
            var rooms = await AddRoomsAndOrders();
            var orderToDelete = new Order()
            {
                Room = rooms[2],
                From = new DateTime(2022, 1, 2),
                Until = new DateTime(2022, 1, 4)
            };
            await _context.Orders.AddAsync(orderToDelete);
            await _context.SaveChangesAsync();
            var orderId = orderToDelete.Id;

            // Act
            await _roomService.DeleteOrderAsync(orderId, CancellationToken.None);

            // Assert
            var deletedOrder = await _context.Orders.FindAsync(orderId);
            Assert.Null(deletedOrder);
        }
    }
}
