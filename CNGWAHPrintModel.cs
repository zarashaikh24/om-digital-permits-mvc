namespace CleanValveManagement.Models
{
    public class CNGWAHPrintModel
    {
        public string? PermitNo { get; set; }
        public string? CName { get; set; }
        public string? Activity { get; set; }
        public string? Date { get; set; }
        public string? TimeOfIssue { get; set; }
        public string? Location { get; set; }
        public string? CrossRef { get; set; }

        public string? STC1 { get; set; }
        public string? STC2 { get; set; }
        public string? STC3 { get; set; }

        public string? AuthTime { get; set; }
        public string? AuthName { get; set; }
        public string? AuthDesig { get; set; }
        public string? AuthContact { get; set; }
        public string? AuthSign { get; set; }

        public string? TpeTime { get; set; }
        public string? TpeName { get; set; }
        public string? TpeDesig { get; set; }
        public string? TpeContact { get; set; }
        public string? TpeSign { get; set; }

        public string? SuperTime { get; set; }
        public string? SuperName { get; set; }
        public string? SuperDesig { get; set; }
        public string? SuperContact { get; set; }
        public string? SuperSign { get; set; }

        public string? CowName { get; set; }
        public string? CowDate { get; set; }
        public string? CowInTime { get; set; }
        public string? CowSign { get; set; }
        public string? CowOutTime { get; set; }
        public string? CowSign2 { get; set; }
        public string? CowContact { get; set; }
        public Dictionary<int, string> Responses { get; set; } = new();
        public Dictionary<int, string> STCNos { get; set; } = new();
    }
}