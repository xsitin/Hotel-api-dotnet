using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Hotel.Api.Controllers;
using Hotel.Core.Dtos;
using Hotel.Core.Services;
using Hotel.Db.Orders;
using Hotel.Db.Rooms;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hotel.Api.UnitTests.Controllers;

public class RoomsControllerTests : ControllerTestsBase<RoomsController>
{
    private readonly Mock<IRoomService> _roomServiceMock;
    public static readonly IEnumerable<object[]> EmptyList = new[] { new[] { new List<RoomDto>() } };

    public RoomsControllerTests()
    {
        _roomServiceMock = Mocker.GetMock<IRoomService>();
    }


    [Theory]
    [AutoData]
    [MemberData(nameof(EmptyList))]
    public async Task GetAll_should_return_expected_results(IList<RoomDto> rooms)
    {
        _roomServiceMock.Setup(x =>
                x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rooms.Adapt<IList<Room>>());
        var expected = new JsonResult(rooms);


        var result = await Controller.GetAllAsync();


        result.Should().NotBeNull();
        result.Should().BeAssignableTo<JsonResult>().And.BeEquivalentTo(expected);
        _roomServiceMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    [MemberData(nameof(EmptyList))]
    public async Task GetFree_should_return_expected_results(IList<RoomDto> rooms)
    {
        _roomServiceMock.Setup(x =>
                x.GetFreeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rooms.Adapt<IList<Room>>());
        var expected = new JsonResult(rooms);


        var result = await Controller.GetFreeAsync(DateTime.Now, DateTime.Today.AddDays(1));


        result.Should().NotBeNull();
        result.Should().BeAssignableTo<JsonResult>().And.BeEquivalentTo(expected);
        _roomServiceMock.Verify(
            x => x.GetFreeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteOrder_should_remove_if_exists()
    {
        var expected = new Order() { Id = Random.Shared.Next(1, 100_000), Room = new Room() };
        _roomServiceMock.Setup(x => x.DeleteOrderAsync(It.Is<int>(id => id == expected.Id),
            It.IsAny<CancellationToken>())).ReturnsAsync(expected);


        var result = await Controller.DeleteOrderAsync(expected.Id);


        result.Should().NotBeNull();
        result.Should().BeAssignableTo<JsonResult>();
        result.Should().BeEquivalentTo(new JsonResult(expected.Adapt<OrderDto>()));
        _roomServiceMock.Verify(service =>
                service.DeleteOrderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteOrder_should_return_404_if_doesnt_exists()
    {
        _roomServiceMock.Setup(x => x.DeleteOrderAsync(It.IsAny<int>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(value: null);


        var result = await Controller.DeleteOrderAsync(Random.Shared.Next(1, 100_000));


        result.Should().BeAssignableTo<NotFoundObjectResult>();
    }

    [Fact]
    public async Task OrderRoom_should_return_created_order()
    {
        var expected = new Order
        {
            Id = 451,
            Room = new Room() { Id = 42, Name = "North Pole" },
            From = DateTime.Today,
            Until = DateTime.Today.AddDays(1),
            Username = "xsitin"
        };
        _roomServiceMock.Setup(x =>
                x.OrderRoomAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var input = new OrderDetailsDto()
        {
            From = expected.From, Until = expected.Until, Room = expected.Room.Adapt<RoomDto>(),
            Username = expected.Username
        };


        var result = await Controller.OrderRoomAsync(input);


        result.Should().NotBeNull();
        result.Should().BeAssignableTo<CreatedResult>();
        result.Should()
            .BeEquivalentTo(new CreatedResult("/api/rooms/" + expected.Id, new JsonResult(expected.Adapt<OrderDto>())));
        _roomServiceMock.Verify(service =>
                service.OrderRoomAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OrderRoom_should_return_409_if_room_doesnt_exists_or_order_invalid()
    {
        _roomServiceMock.Setup(x =>
                x.OrderRoomAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: null);


        var result = await Controller.OrderRoomAsync(new OrderDetailsDto() { Id = 12345 });


        result.Should().NotBeNull();
        result.Should().BeAssignableTo<ConflictObjectResult>();
        result.Should().BeEquivalentTo(new ConflictObjectResult("overlaps with an existing order or room not found"));
    }
}
