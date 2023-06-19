using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseManagement.Models
{
    public class Counter
    {
        public int id { get; set; }

        [Display(Name = "Номер"), MinLength(5, ErrorMessage = "Данное поле должно содержать минимум 5 цифр"), MaxLength(20, ErrorMessage = "Данное поле должно содержать максимум 20 цифр")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string number { get; set; }

        [Display(Name = "Статус использования")]
        public bool used { get; set; }

        public bool IMDOrGHMD { get; set; }
        
        [Display(Name = "Дата предыдущей поверки")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public DateTime dateLastVerification { get; set; }

        [Display(Name = "Дата следующей поверки")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public DateTime dateNextVerification { get; set; }
        public int idService { get; set; }
        public int? idFlat { get; set; }
        public int? idHouse { get; set; }

        public Counter() 
        {
            this.dateLastVerification = DateTime.UtcNow;
            this.dateNextVerification = DateTime.UtcNow.AddYears(10);
        }
    }
}
