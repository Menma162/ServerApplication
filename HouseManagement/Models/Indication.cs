using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseManagement.Models
{
    public class Indication
    {
        public int id { get; set; }

        [Display(Name = "Дата передачи")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [DataType(DataType.Date)]
        public DateTime dateTransfer { get; set; }

        [Display(Name = "Значение"), Range(0, 99999, ErrorMessage = "Значением данного поля может быть число от 0 до 99999")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int? value { get; set; }
        public int idCounter { get; set; }
        public Indication() { }
    }
}
