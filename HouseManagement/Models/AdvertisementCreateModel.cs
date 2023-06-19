using System.ComponentModel.DataAnnotations.Schema;

namespace HouseManagement.Models
{
    public class AdvertisementCreateModel
    {
        public int id { get; set; }
        public DateTime date { get; set; }
        public string description { get; set; }
        public List<int>? idHouses { get; set; }
        public AdvertisementCreateModel() { }
        public AdvertisementCreateModel(DateTime date, string description, List<int>? idHouses)
        {
            this.date = date;
            this.description = description;
            this.idHouses = idHouses;
        }
    }
}
