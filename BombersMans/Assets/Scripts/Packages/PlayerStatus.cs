namespace NetCode.Packages
{
    public class PlayerStatus
    {
        public int PlayerCode {get; set;}
        public string PlayerNickname {get; set;}
        public string TextStatus {get; set;}
        public string Type { get; set; } = "PlayerStatus";
    }
}