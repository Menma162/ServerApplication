using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseManagement.Models
{
    public class Residence
    {
        public Residence(int idFlatOwner, int idFlat)
        {
            this.idFlatOwner = idFlatOwner;
            this.idFlat = idFlat;
        }
        public int id { get; set; }
        public int idFlatOwner { get; set; }
        public int idFlat { get; set; }
    }
}
