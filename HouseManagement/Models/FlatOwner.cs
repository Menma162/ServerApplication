using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace HouseManagement.Models
{
    public class FlatOwner
    {
        public int id { get; set; }

        [Display(Name = "ФИО")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string fullName { get; set; }

        [Display(Name = "Email")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректный адрес")]
        public string? email { get; set; }

        [Display(Name = "Номер телефона")]
        [RegularExpression(@"[7-8]{1}[0-9]{10}]?", ErrorMessage = "Некорректный ввод")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string phoneNumber { get; set; }
        public int idHouse { get; set; }
    }
}
