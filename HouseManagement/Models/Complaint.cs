using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseManagement.Models
{
    public class Complaint
    {
        public int id { get; set; }
        [Display(Name = "Статус заявки")]
        public string? status { get; set; }

        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Display(Name = "Описание")]
        public string description { get; set; }
        [Display(Name = "Фото")]
        public string? photo { get; set; }
        [Display(Name = "Заявка от ")]
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public int? idFlat { get; set; }
        public Complaint() { }
    }
}
