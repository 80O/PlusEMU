namespace Plus.HabboHotel.Users.UserData
{
    public interface IUserDataFactory
    {
        Task<Habbo?> Create(int userId);
        Task<string> GetUsernameForHabboById(int userId);
    }
}