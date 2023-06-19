using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace HouseManagement.Models
{
    public class UserModel
    {
        public string? id;

        [Display(Name = "Email пользователя")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректный адрес")]
        public string email { get; set; }

        [Display(Name = "Пароль пользователя"), MinLength(8, ErrorMessage = "Длина данного поля - минимум 8"), MaxLength(30, ErrorMessage = "Длина данного поля - максимум 30")]
        [DataType(DataType.Password)]
        public string? password { get; set; }
        [Display(Name = "Вид пользователя")]
        public string? role { get; set; }
        public int? idFlatOwner { get; set; }
        public List<int>? idHouses { get; set; }
        public UserModel() { }
        public UserModel(string id, string email, string password, string role, int? idFlatOwner, List<int>? idHouses)
        {
            this.id = id;
            this.email = email;
            this.password = password;
            this.role = role;
            this.idFlatOwner = idFlatOwner;
            this.idHouses = idHouses;
        }
    }
}
