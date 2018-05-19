using System;

namespace BGG
{
    public interface IGame
    {
        int Id { get; set; }
        string Name { get; set; }
    }
    public interface IGeekItem
    {
        IGame Game { get; set; }
    }
    public interface IPlay
    {
        IGame Game { get; set; }
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
