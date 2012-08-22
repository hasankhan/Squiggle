using System;
namespace Squiggle.Core
{
    public interface ISquiggleEndPoint
    {
        System.Net.IPEndPoint Address { get; set; }
        string ClientID { get; set; }
    }
}
