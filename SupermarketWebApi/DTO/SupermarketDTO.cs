using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi.DTO
{
    public class SupermarketDTO
    {
        public int SupermarketId { get; set; }
        public string Location { get; set; }
        public int NumberOfStaff { get; set; }
    }
}
