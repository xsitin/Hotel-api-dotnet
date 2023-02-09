using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hotel.Db;
using Hotel.Db.Orders;
using Hotel.Db.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hotel.Core.Services;

public interface IRoomService
{
    Task<IList<Room>> GetAllAsync(CancellationToken ctToken);
    Task<IList<Room>> GetFreeAsync(DateTime from, DateTime to, CancellationToken ctToken);
    Task<Order> OrderRoomAsync(Order order, CancellationToken ctToken);
    Task<Order> DeleteOrderAsync(int id, CancellationToken ctToken);
}

public class RoomService : IRoomService
{
    private RoomsContext Context { get; set; }
    private ILogger<RoomService> Logger { get; }
    private IUserService UserService { get; }

    public RoomService(RoomsContext context, ILogger<RoomService> logger, IUserService userService)
    {
        Context = context;
        Logger = logger;
        UserService = userService;
    }

    public async Task<IList<Room>> GetAllAsync(CancellationToken ctToken) =>
        await Context.Rooms.AsNoTracking().ToListAsync(cancellationToken: ctToken);

    public async Task<Room> GetByIdAsync(int id, CancellationToken ctToken) =>
        await Context.Rooms.FindAsync(new object[] { id }, cancellationToken: ctToken);

    public async Task<IList<Room>> GetFreeAsync(DateTime from, DateTime to, CancellationToken ctToken)
    {
        return await Context.Rooms.Where(room =>
                !room.Orders.Any(order =>
                    (from <= order.From && order.From <= to) ||
                    (from <= order.Until && order.Until <= to)))
            .AsNoTracking().ToListAsync(cancellationToken: ctToken);
    }

    private async Task<Room> GetRoomWithOrdersAsync(int roomId, CancellationToken ctToken)
    {
        return await Context.Rooms
            .Include(x => x.Orders)
            .FirstOrDefaultAsync(x => x.Id == roomId, ctToken);
    }

    private async Task<bool> IsOverlappingOrderExistsAsync(Order order, CancellationToken ctToken)
    {
        return await Context.Orders
            .AnyAsync(otherOrder => otherOrder.Room == order.Room &&
                                    otherOrder.Id != order.Id &&
                                    ((order.From >= otherOrder.From && order.From < otherOrder.Until) ||
                                     (order.Until > otherOrder.From && order.Until <= otherOrder.Until) ||
                                     (order.From <= otherOrder.From && order.Until >= otherOrder.Until)),
                ctToken);
    }

    public async Task<Order> OrderRoomAsync(Order order, CancellationToken ctToken)
    {
        Logger.LogDebug("Called RoomService with order: {Order}", JsonSerializer.Serialize(order));
        await using var transaction =
            await Context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken: ctToken);
        Order result = null;
        try
        {
            var room = await GetRoomWithOrdersAsync(order.Room.Id, ctToken);
            if (room == null)
                return null;
            order.Room = room;
            if (await IsOverlappingOrderExistsAsync(order, ctToken))
                return null;
            order.IsVip = UserService.IsVip(order.Username);
            order.Room.Orders.Add(order);
            result = order;
            await Context.SaveChangesAsync(ctToken);
            await transaction.CommitAsync(ctToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ctToken);
        }


        return result;
    }

    public async Task<Order> DeleteOrderAsync(int id, CancellationToken ctToken)
    {
        var order = await Context.Orders.FindAsync(id, ctToken);
        if (order == null)
        {
            return null;
        }

        Context.Orders.Remove(order);
        await Context.SaveChangesAsync(ctToken);
        return order;
    }


    public async Task<Room> CreateAsync(Room room, CancellationToken ctToken) =>
        (await Context.Rooms.AddAsync(room, ctToken)).Entity;
}
