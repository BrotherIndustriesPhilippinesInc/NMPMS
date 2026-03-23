namespace NMPMS.ViewModels
{
    public class CreateIssueViewModel
    {
        public string PmsCreate { get; set; }
        public string ProblemName { get; set; }
        public string PhenomenonDetails { get; set; }
        public string Stage { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string AreaDetection { get; set; }
        public string Process { get; set; }
        public string IssuedBy { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string Supplier { get; set; }
        public string ControlNo { get; set; }

        public IFormFile ProblemPhoto { get; set; }
        public IFormFile Attachment { get; set; }
    }
}
