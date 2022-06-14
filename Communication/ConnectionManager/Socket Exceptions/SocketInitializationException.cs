namespace Plus.Communication.ConnectionManager.Socket_Exceptions;

public class SocketInitializationException : Exception
{
    public SocketInitializationException(string message)
        : base(message) { }
}