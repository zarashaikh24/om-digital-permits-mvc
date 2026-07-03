namespace CleanValveManagement.Models
{
    public class CNGPermitReport
    {
        public int id { get; set; }
        public string? PermitType { get; set; }
        public string? AIC_name { get; set; }
        public string? TPE_name { get; set; }
        public string? PONo { get; set; }
        public string? STCNo { get; set; }
        public string? created_at { get; set; }
        public string? status { get; set; }
        public string? review_msg { get; set; }
        public string? approved_at { get; set; }
        public string? closed_at { get; set; }
    }
}