namespace HypnoTech.Ecommerce.Models
{
    public class DownloadCode
    {
        public string Code { get; set; }

        public string AssignedTo { get; set; }

        public bool Assigned => !string.IsNullOrEmpty(AssignedTo);
    }
}
