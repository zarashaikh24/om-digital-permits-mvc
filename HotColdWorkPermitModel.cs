namespace CleanValveManagement.Models
{
    public class HotColdWorkPermitModel
    {
        public string? PermitNo { get; set; }
        public string? SiteSupervisorName { get; set; }
        public string? AStartDate { get; set; }
        public string? AEndDate { get; set; }
        public string? SiteLocation { get; set; }
        public string? NatureOfWork { get; set; }

        public string? AEngineerName { get; set; }
        public string? ADesignation { get; set; }
        public string? ASignature { get; set; }
        public string? ATelephone { get; set; }

        public string? MP_HD { get; set; }
        public string? MP_HE { get; set; }
        public string? MP_HI { get; set; }
        public string? MP_HMM { get; set; }
        public string? MP_Lock { get; set; }
        public string? MP_Tag { get; set; }

        public string? PG_HD { get; set; }
        public string? PG_HE { get; set; }
        public string? PG_HI { get; set; }
        public string? PG_HMM { get; set; }
        public string? PG_Lock { get; set; }
        public string? PG_Tag { get; set; }

        public string? ROE_HD { get; set; }
        public string? ROE_HE { get; set; }
        public string? ROE_HI { get; set; }
        public string? ROE_HMM { get; set; }
        public string? ROE_Lock { get; set; }
        public string? ROE_Tag { get; set; }

        public string? Elec_HD { get; set; }
        public string? Elec_HE { get; set; }
        public string? Elec_HI { get; set; }
        public string? Elec_HMM { get; set; }
        public string? Elec_Lock { get; set; }
        public string? Elec_Tag { get; set; }

        public string? CSE_HD { get; set; }
        public string? CSE_HE { get; set; }
        public string? CSE_HI { get; set; }
        public string? CSE_HMM { get; set; }
        public string? CSE_Lock { get; set; }
        public string? CSE_Tag { get; set; }

        public string? Other_HD { get; set; }
        public string? Other_HE { get; set; }
        public string? Other_HI { get; set; }
        public string? Other_HMM { get; set; }
        public string? Other_Lock { get; set; }
        public string? Other_Tag { get; set; }

        public string? ASiteSupervisor { get; set; }
        public string? ASSDesignation { get; set; }
        public string? ASSName { get; set; }
        public string? ASSTelephone { get; set; }

        public bool CheckBox1 { get; set; }
        public bool CheckBox2 { get; set; }
        public bool CheckBox3 { get; set; }
        public bool CheckBox4 { get; set; }
        public bool CheckBox5 { get; set; }
        public bool CheckBox6 { get; set; }
        public bool CheckBox7 { get; set; }

        public string? ProcedureRefNo { get; set; }
        public string? OtherScc { get; set; }

        public string? CEngineer { get; set; }
        public string? CDesignation { get; set; }
        public string? CName { get; set; }
        public string? CTelephone { get; set; }

        public string? Lor1 { get; set; }
        public string? Lor2 { get; set; }
        public string? Lor3 { get; set; }
        public string? Lor4 { get; set; }
        public string? Lor5 { get; set; }
        public string? Lor6 { get; set; }
        public string? Lor7 { get; set; }
        public string? Lor8 { get; set; }
        public string? Lor9 { get; set; }
        public string? Lor10 { get; set; }

        public string? Tor1 { get; set; }
        public string? Tor2 { get; set; }
        public string? Tor3 { get; set; }
        public string? Tor4 { get; set; }
        public string? Tor5 { get; set; }
        public string? Tor6 { get; set; }
        public string? Tor7 { get; set; }
        public string? Tor8 { get; set; }
        public string? Tor9 { get; set; }
        public string? Tor10 { get; set; }

        public string? R1 { get; set; }
        public string? R2 { get; set; }
        public string? R3 { get; set; }
        public string? R4 { get; set; }
        public string? R5 { get; set; }
        public string? R6 { get; set; }
        public string? R7 { get; set; }
        public string? R8 { get; set; }
        public string? R9 { get; set; }
        public string? R10 { get; set; }
        public List<HotColdHazardItem> HazardItems { get; set; } = new();

        public List<HotColdReadingItem> ReadingItems { get; set; } = new();

        public Signatory AuthorizingPerson { get; set; } = new();

        public Signatory SiteSupervisor { get; set; } = new();

        public Signatory WorkCompleted { get; set; } = new();
    }
    public class HotColdHazardItem
    {
        public string? Hazard { get; set; }
        public string? HazardDetails { get; set; }
        public string? HazardExists { get; set; }
        public string? HazardIsolated { get; set; }
        public string? HazardMitigation { get; set; }
        public string? LockNo { get; set; }
        public string? TagNo { get; set; }
    }

    public class HotColdReadingItem
    {
        public string? Location { get; set; }
        public string? Time { get; set; }
        public string? Reading { get; set; }
    }

    public class Signatory
    {
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? ContactNo { get; set; }
        public string? Signature { get; set; }
        public DateTime? TimeSigned { get; set; }
    }
}