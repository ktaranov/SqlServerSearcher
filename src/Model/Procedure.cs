namespace SQLServerSearcher.Model
{
    using System;

    public class Procedure
    {
        public string SchemaName { get; set; }
        public string Name { get; set; }
        public string ParameterName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime LastExecutionTime { get; set; }
    }
}
