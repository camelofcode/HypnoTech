using System;

namespace HypnoTech.Common.Models.Settings
{
    public class SerilogEntry
    { 
        public string Id { get; set; }

        public DateTime DateTime { get; set; }

        public string Level { get; set; }

        public string Message { get; set; }

        public string MessageDisplay => HasException ? Exception.Split(Environment.NewLine[0])[0] : Message;

        public string Exception { get; set; }

        public SerilogProperties Properties { get; set; }

        public bool HasException => Level == "Error" && !string.IsNullOrEmpty(Exception);

        public string TableClass => Level == "Error" ? "table-danger" : "table-warning";

        public string DetailsClass => Level == "Error" ? "text-danger" : "text-warning";
    }

    public class SerilogProperties
    {
        public string SiteName { get; set; }

        public string Path { get; set; }
    }
}
