namespace CleanValveManagement.Models
{
    public class CngElectricalPermitViewModel
    {
        public string? PermitNo { get; set; }
        public string? DateOfIssue { get; set; }
        public string? SiteName { get; set; }
        public string? SiteLocation { get; set; }
        public string? WorkDescription { get; set; }
        public string? TransferredTo { get; set; }

        // Signatures / Persons
        public string? TransferringPerson { get; set; }
        public string? ReceivingPerson { get; set; }
        public string? IssuingAuthoritySignature { get; set; }
        public string? IssuingAuthorityName { get; set; }
        public string? IssuingAuthorityDesignation { get; set; }
        public string? SiteSupervisor { get; set; }
        public string? SiteSupervisorName { get; set; }
        public string? SiteSupervisorDesignation { get; set; }

        // Isolation
        public string? IsolatorPresent { get; set; }
        public string? IsolatorLock { get; set; }
        public string? IsolatorTag { get; set; }
        public string? IsolatorRemark { get; set; }

        public string? FusesPresent { get; set; }
        public string? FusesLock { get; set; }
        public string? FusesTag { get; set; }
        public string? FusesRemark { get; set; }

        public string? OthersPresent { get; set; }
        public string? OthersLock { get; set; }
        public string? OthersTag { get; set; }
        public string? OthersRemark { get; set; }

        public string? VoltageLevel { get; set; }
        public string? KeySafeNo { get; set; }

        // PPE
        public bool ElectricalSafetyShoes { get; set; }
        public bool InsulatedTools { get; set; }
        public bool ElectricalGloves { get; set; }
        public bool FireExtinguishers { get; set; }
        public string? OtherPPEs { get; set; }
    }
}
