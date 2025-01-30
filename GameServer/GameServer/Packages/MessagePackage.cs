using System.Net.Http.Json;
using Newtonsoft.Json;

namespace GameServer.Packages;

public class MessagePackage
{
    public string Sender { get; set; }
    public string Content { get; set; }
    public static string Type { get; set; }
}