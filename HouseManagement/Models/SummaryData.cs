using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace HouseManagement.Models
{
    public class SummaryData
    {
        [Display(Name = "Название дома")]
        public string nameHouse { get; set; }
        [Display(Name = "Услуга")]
        public string nameService { get; set; }

        [Display(Name = "Сумма показаний")]
        public int summa { get; set; }
        [Display(Name = "Единица измерения")]
        public string unit { get; set; }

        public SummaryData(string nameHouse, string nameService, int summa, string unit)
        {
            this.nameHouse = nameHouse;
            this.nameService = nameService;
            this.summa = summa;
            this.unit = unit;
        }
    }
}
