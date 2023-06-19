using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseManagement.Models
{
    public class Payment
    {
        public int id { get; set; }
        [Display(Name = "Период")]
        public string period { get; set; }
        [Display(Name = "Сумма начисления")]
        [RegularExpression(@"\d+[\.?\d+]*", ErrorMessage = "Некорректный ввод")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string amount { get; set; }
        [Display(Name = "Пени")]
        [RegularExpression(@"\d+[\.?\d+]*", ErrorMessage = "Некорректный ввод")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string penalties { get; set; }
        public int idService { get; set; }
        public int idFlat { get; set; }
        public Payment() { }
    }
}
