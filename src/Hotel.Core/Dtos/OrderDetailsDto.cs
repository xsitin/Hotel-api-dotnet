namespace Hotel.Core.Dtos;

public class OrderDetailsDto
{
    public int Id { get; set; }
    public DateTime From { get; set; }
    public DateTime Until { get; set; }
    public RoomDto Room { get; set; }
    public string Username { get; set; }
}
