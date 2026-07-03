namespace CleanValveManagement.Models
{
    public class PermitDetailsViewModel
    {
        public int PermitNumber { get; set; }
        public string PermitType { get; set; }
        public string RequestorName { get; set; }

        public List<PermitImageViewModel> Images { get; set; } = new List<PermitImageViewModel>();
    }

    public class PermitImageViewModel
    {
        public string Title { get; set; }
        public string ImagePath { get; set; }
    }
}