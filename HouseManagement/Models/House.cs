using System.ComponentModel.DataAnnotations;

namespace HouseManagement.Models
{
    public class House
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Display(Name = "Название")]
        public string? name { get; set; }
        public House() { }
    }
}
