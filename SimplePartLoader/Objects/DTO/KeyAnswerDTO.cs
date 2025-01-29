namespace SimplePartLoader.Objects.DTO
{
    public class KeyAnswerDTO
    {
        public string ModId { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;
    }

    public class KeyAnswer
    {
        public string ModId { get; set; } = string.Empty;
        public byte[] Key { get; set; }
        public byte[] IV { get; set; } 
        public string Checksum { get; set; } = string.Empty;
        public string PublicKey { get; set; }
    }
}
