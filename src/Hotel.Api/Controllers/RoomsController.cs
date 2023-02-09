using Hotel.Core.Dtos;
using Hotel.Core.Services;
using Hotel.Db.Orders;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[Route("api/[controller]")]
public class RoomsController : ApiControllerBase
{
    public RoomsController(IRoomService roomService)
    {
        RoomService = roomService;
    }

    private IRoomService RoomService { get; }

    [HttpGet("all")]
    public async Task<ActionResult> GetAllAsync()
    {
        return new JsonResult((await RoomService.GetAllAsync(CancellationToken.None)).Adapt<IList<RoomDto>>());
    }

    [HttpGet("free")]
    public async Task<ActionResult> GetFreeAsync([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        return new JsonResult((await RoomService.GetFreeAsync(from, to, CancellationToken.None))
            .Adapt<IList<RoomDto>>());
    }

    [HttpPost("order")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> OrderRoomAsync([FromBody] OrderDetailsDto order)
    {
        var result = await RoomService.OrderRoomAsync(order.Adapt<Order>(), CancellationToken.None);
        if (result == null)
            return Conflict("overlaps with an existing order or room not found");

        return new CreatedResult("/api/rooms/" + result.Id, new JsonResult(result.Adapt<OrderDto>()));
    }


    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteOrderAsync(int id)
    {
        var result = await RoomService.DeleteOrderAsync(id, CancellationToken.None);
        if (result == null)
            return NotFound("Order not found");
        return new JsonResult(result.Adapt<OrderDto>());
    }
}
