using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure
{
    public class BaseSearchModel
    {
        public int Page { get; set; }

        public int Pages { get; set; }

        public int Perpage { get; set; } = 10;

        public int Total { get; set; }

        public string Sort { get; set; }

        public string Field { get; set; }

        public string Query { get; set; }
    }
}
