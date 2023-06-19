namespace HouseManagement.Models
{
    public class ModelCheckBox
    {
        public int id { get; set; }
        public bool selected { get; set; }
        public ModelCheckBox() { }

        public ModelCheckBox(int id, bool selected)
        {
            this.id = id;
            this.selected = selected;
        }
    }
}
