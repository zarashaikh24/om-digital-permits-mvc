namespace CleanValveManagement.Models
{
    public class CngElectricalPermitPrintModel
    {
        public int PermitNo { get; set; }
        public DateTime? SiteDate { get; set; }

        public string? SiteName { get; set; }
        public string? Location { get; set; }
        public string? CrossReference { get; set; }
        public string? NatureOfWork { get; set; }

        public string? VoltageLevel { get; set; }
        public string? KeySafe { get; set; }

        public string? ESSPresent { get; set; }
        public string? ITPresent { get; set; }
        public string? EGPresent { get; set; }
        public string? FEPresent { get; set; }
        public string? OtherPPEs { get; set; }

        public List<CngElectricalApparatus> ApparatusList { get; set; } = new();

        public CngElectricalPerson IssuingAuthority { get; set; } = new();
        public CngElectricalPerson ReceivingPerson { get; set; } = new();
        public CngElectricalPerson ClosurePerson { get; set; } = new();
    }

    public class CngElectricalApparatus
    {
        public string? Apparatus { get; set; }
        public string? Lock { get; set; }
        public string? Tag { get; set; }
        public string? Remarks { get; set; }
    }

    public class CngElectricalPerson
    {
        public string? IdNo { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? ContactNo { get; set; }
        public string? Signature { get; set; }
        public DateTime? TimeSigned { get; set; }
    }
}