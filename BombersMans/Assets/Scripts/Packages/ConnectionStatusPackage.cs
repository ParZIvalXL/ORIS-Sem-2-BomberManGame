namespace NetCode.Packages
{
    public class ConnectionStatusPackage
    {
        public int ConnectionState { get; set; } // - статус подключения
        public string ConnectionDescription { get; set; } // - описание подключения

        public string Type {get; set;} = "ConnectionStatusPackage";
    }
}