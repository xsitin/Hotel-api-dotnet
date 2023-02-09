using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Hotel.Api.IntegrationTests.Infrastructure;
using Hotel.Core.Dtos;
using VerifyXunit;
using Xunit;

namespace Hotel.Api.IntegrationTests;

[UsesVerify]
[Collection(nameof(TestServerClientCollection))]
public class RoomsControllerTests
{
    private readonly HttpClient _client;

    public RoomsControllerTests(TestServerClientFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/rooms/all");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFreeAsync_WithValidDates_ReturnsSuccessStatusCode()
    {
        // Arrange
        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(1);

        // Act
        var response = await _client.GetAsync($"/api/rooms/free?from={from}&to={to}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OrderRoomAsync_WithValidOrderDetails_ReturnsCreatedStatusCode()
    {
        // Arrange
        var orderDetails = new OrderDetailsDto
        {
            Room = new RoomDto() { Id = 1 },
            From = DateTime.Now.AddDays(5),
            Until = DateTime.Now.AddDays(6),
            Username = "user1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/rooms/order", orderDetails);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task OrderRoomAsync_WithConflictingOrderDetails_ReturnsConflictStatusCode()
    {
        // Arrange
        var orderDetails = new OrderDetailsDto
        {
            Room = new RoomDto() { Id = 1 },
            From = DateTime.Now.AddDays(1),
            Until = DateTime.Now.AddDays(2),
            Username = "user1"
        };
        var orderDetails2 = new OrderDetailsDto
        {
            Room = new RoomDto() { Id = 2 },
            From = DateTime.Now,
            Until = DateTime.Now.AddDays(3),
            Username = "user1"
        };
        await _client.PostAsJsonAsync("/api/rooms/order", orderDetails);

        // Act
        var response = await _client.PostAsJsonAsync("/api/rooms/order", orderDetails2);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOrderAsync_WithValidId_ReturnsOkStatusCode()
    {
        // Act
        var response = await _client.DeleteAsync("/api/rooms/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOrderAsync_WithInvalidId_ReturnsNotFoundStatusCode()
    {
        // Act
        var response = await _client.DeleteAsync("/api/rooms/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
