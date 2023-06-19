using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.Diagnostics.CodeAnalysis;

namespace HouseManagement.Models
{
    public class Flat
    {
        public int id { get; set; }

        [Display(Name = "Лицевой счет"), MinLength(8, ErrorMessage = "Данное поле должно содержать минимум 8 цифр"), MaxLength(20, ErrorMessage = "Данное поле должно содержать максимум 20 цифр")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string personalAccount { get; set; }

        [Display(Name = "Номер квартиры")]
        [RegularExpression(@"^[0-9]{1,3}[а-яА-Я]?$", ErrorMessage = "Некорректный ввод")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string flatNumber { get; set; }

        [Display(Name = "Общая площадь")]
        [RegularExpression(@"\d+[\.?\d+]*", ErrorMessage = "Некорректный ввод")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string totalArea { get; set; }

        [Display(Name = "Полезная площадь")]
        [RegularExpression(@"\d+[\.?\d+]*", ErrorMessage = "Некорректный ввод")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string usableArea { get; set; }

        [Display(Name = "Номер подъезда"), Range(1, 99, ErrorMessage = "Значением данного поля может быть число от 1 до 99")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int? entranceNumber { get; set; }

        [Display(Name = "Количество комнат"), Range(1, 99, ErrorMessage = "Значением данного поля может быть число от 1 до 99")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int? numberOfRooms { get; set; }

        [Display(Name = "Количество зарегистрированных жителей"), Range(0, 99, ErrorMessage = "Значением данного поля может быть число от 0 до 99")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int? numberOfRegisteredResidents { get; set; }

        [Display(Name = "Количество владельцев жилья"), Range(1, 99, ErrorMessage = "Значением данного поля может быть число от 1 до 99")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public int? numberOfOwners { get; set; }
        public int idHouse { get; set; }

        public Flat() { }
    }
}
