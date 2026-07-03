namespace CleanValveManagement.Models
{
    public class CSEPermitPrintModel
    {
        public int PermitNo { get; set; }
        public DateTime? PrintDate { get; set; }
        public DateTime? StartDate { get; set; }

        public string? SiteName { get; set; }
        public string? Location { get; set; }
        public string? CrossReference { get; set; }
        public string? ContractorName { get; set; }

        public List<CSEChecklistItem> ChecklistItems { get; set; } = new();

        public CSEPerson FirstSignatory { get; set; } = new();
        public CSEPerson SecondSignatory { get; set; } = new();
        public CSEPerson ThirdSignatory { get; set; } = new();

        public CSEAtmosphereReading Atmosphere { get; set; } = new();
    }

    public class CSEChecklistItem
    {
        public int SrNo { get; set; }
        public string? Description { get; set; }
        public string? Answer { get; set; }  // Yes / No / NA
        public string? Remark { get; set; }
    }

    public class CSEPerson
    {
        public DateTime? TimeSigned { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? ContactNo { get; set; }
        public string? Signature { get; set; }
    }

    public class CSEAtmosphereReading
    {
        public string? OxygenRequired { get; set; }
        public string? NaturalGasRequired { get; set; }

        public string? O0 { get; set; }
        public string? O1 { get; set; }
        public string? O2 { get; set; }
        public string? O3 { get; set; }
        public string? O4 { get; set; }
        public string? O5 { get; set; }
        public string? O6 { get; set; }
        public string? O7 { get; set; }

        public string? N0 { get; set; }
        public string? N1 { get; set; }
        public string? N2 { get; set; }
        public string? N3 { get; set; }
        public string? N4 { get; set; }
        public string? N5 { get; set; }
        public string? N6 { get; set; }
        public string? N7 { get; set; }
    }
}