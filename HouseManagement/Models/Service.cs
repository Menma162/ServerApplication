using System.ComponentModel.DataAnnotations;

namespace HouseManagement.Models
{
    public class Service
    {
        public int id { get; set; }
        public string nameService { get; set; }
        public string nameCounter { get; set; }
        public Service() { }
    }
}
