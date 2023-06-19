using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseManagement.Models
{
    public class SettingsService
    {
        public int id { get; set; }
        [Display(Name = "Начисляется ли")]
        public bool paymentStatus { get; set; }
        [Display(Name = "Индивидуальный/общедомовой прибор учета")]
        public bool? typeIMD { get; set; }
        [Display(Name = "Размер тарифа")]
        [RegularExpression(@"\d+[\.?\d+]*", ErrorMessage = "Некорректный ввод")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string valueRate { get; set; }
        [Display(Name = "Размер норматива")]
        [RegularExpression(@"\d+[\.?\d+]*", ErrorMessage = "Некорректный ввод")]
        public string? valueNormative { get; set; }
        [Display(Name = "Имеется ли счетчик")]
        public bool haveCounter { get; set; }
        [Display(Name = "Дата начала передачи показаний"), Range(1, 30, ErrorMessage = "Значением поля должно быть число от 1 до 30")]
        public int? startDateTransfer { get; set; }
        [Display(Name = "Дата окончания передачи показаний"), Range(1,30, ErrorMessage = "Значением поля должно быть число от 1 до 30")]
        public int? endDateTransfer { get; set; }
        [Display(Name = "Периоды начисления")]
        public string? paymentPeriod { get; set; }
        public int idService { get; set; }
        public int idHouse { get; set; }
        public SettingsService() { }
    }
}
