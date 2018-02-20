using System;

namespace Server.Servers
{
    public class PortIsInUseException : Exception
    {
        public PortIsInUseException(string message) : base(message)
        {
            
        }
    }
}