using System.ComponentModel.DataAnnotations;

namespace HouseManagement.Models
{
    public class NewModel
    {
        public int Id { get; set; }
        [Display(Name = "Наименование")]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }

        public NewModel(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
