namespace CleanValveManagement.Models
{
    public class SMAReportModel
    {
        public string? PermitNo { get; set; }
        public string? Gascoseeker { get; set; }
        public string? HiraDocument { get; set; }
        public string? TBTDocument { get; set; }
        public string? STC { get; set; }
        public string Technician { get; set; }

        public string Emergency { get; set; }

        public string OtherImage { get; set; }
        public string? ClosurePhoto1 { get; set; }
        public string? ClosurePhoto2 { get; set; }
        public string? ClosurePhoto3 { get; set; }

        public string? ValveName { get; set; }
        public string? District { get; set; }
        public string? ValveId { get; set; }
        public string? AICName { get; set; }
        public string? TPEName { get; set; }
        public string? STCNo { get; set; }
        public string? CreatedOn { get; set; }
        public string? Status { get; set; }
        public string? EmpId { get; set; }
        public string? ReviewMessage { get; set; }
        public string? ApprovedOn { get; set; }
        public string? ClosedOn { get; set; }
    }
}
