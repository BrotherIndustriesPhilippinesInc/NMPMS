namespace NMPMS.Models
{
    public class PmsRecord
    {
        public string ControlNo { get; set; }
        public string PmsCreate { get; set; }
        public string PersonInCharge { get; set; }
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
        public string ProblemPhoto { get; set; }
        public string Attachment { get; set; }
        public string AttachmentName { get; set; }
    }
}
