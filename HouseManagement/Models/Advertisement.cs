using System.ComponentModel.DataAnnotations;

namespace HouseManagement.Models
{
    public class Advertisement
    {
        public int id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Объявление от")]
        public DateTime date { get; set; }

        [Display(Name = "Описание")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string description { get; set; }
        public int idHouse { get; set; }

        public Advertisement() { }
    }
}
