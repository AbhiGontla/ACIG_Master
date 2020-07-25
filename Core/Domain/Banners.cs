using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain
{
    public class Banners : BaseEntity
    {

        public string Name { get; set; }
        public int priority { get; set; }
        public string ImageUrl { get; set; }
        public string Type { get; set; }
    }
}
