using System.Collections.Generic;

namespace NetCode.Packages
{
    public class PlayerListPackage
    {
        public string Type { get; set; }
        public List<PlayerPackage> List { get; set; }
    }
}