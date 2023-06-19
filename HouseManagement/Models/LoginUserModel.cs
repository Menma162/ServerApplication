using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace HouseManagement.Models
{
    public class LoginUserModel
    {
        [Display(Name = "Email пользователя")]
        public string email { get; set; }

        [Display(Name = "Пароль пользователя")]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }
}
