namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server(8888);
            server.Start();
        }
    }
}
