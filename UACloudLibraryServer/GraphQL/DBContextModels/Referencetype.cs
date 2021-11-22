﻿
using System.ComponentModel.DataAnnotations;

namespace UACloudLibrary.DbContextModels
{
    public partial class Referencetype
    {
        [Key]
        public int Referencetype_id { get; set; }

        public long Nodeset_id { get; set; }

        public string Referencetype_BrowseName { get; set; }

        public string Referencetype_Value { get; set; }

        public string Referencetype_Namespace { get; set; }
    }
}
