using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACIGConsumer.Models.Content
{
    public class ArticlesModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }
}
