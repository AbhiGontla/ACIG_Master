using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACIGConsumer.Models.Home
{
    public class BannerModel
    {
        public string Name { get; set; }
        public int priority { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public IFormFile Image { get; set; }
    }
}
