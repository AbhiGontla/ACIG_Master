using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Content
{
    public class Articles:BaseEntity
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }
}
