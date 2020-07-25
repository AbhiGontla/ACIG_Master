using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure
{
    public class PageListMetadata
    {
        public int Page { get; set; }

        public int Pages { get; set; }

        public int Perpage { get; set; }

        public int Total { get; set; }

        public string Sort { get; set; }

        public string Field { get; set; }

        public string Query { get; set; }
    }
}
