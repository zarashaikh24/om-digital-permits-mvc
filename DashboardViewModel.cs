namespace CleanValveManagement.Models
{
    public class DashboardViewModel
    {
        public int TotalPermits { get; set; }

        public int CPPermits { get; set; }
        public int CNGPermits { get; set; }

        public int CPRequested { get; set; }
        public int CPApproved { get; set; }
        public int CPRejected { get; set; }
        public int CPClosed { get; set; }

        public int CNGRequested { get; set; }
        public int CNGApproved { get; set; }
        public int CNGRejected { get; set; }
        public int CNGClosed { get; set; }

        public int SMAPermits { get; set; }

        public int SMARequested { get; set; }
        public int SMAApproved { get; set; }
        public int SMARejected { get; set; }
        public int SMAClosed { get; set; }
    }
}