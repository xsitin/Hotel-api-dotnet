namespace Hotel.Core.Services;

public interface IUserService
{
    bool IsVip(string username);
}

public class UserService : IUserService
{
    public bool IsVip(string username)
    {
        return username == "xsitin";
    }
}
