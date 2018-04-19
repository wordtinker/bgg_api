using System;

namespace BGG
{
    public interface IPlay
    {
        string GameId { get; set; }
        int Minutes { get; set; }
    }
    public interface IUser
    {
        string UserName { get; set; }
    }
    public interface IRating
    {
        IUser User { get; set; }
        double Value { get; set; }
        DateTime TimeStamp { get; set; }
    }
}
