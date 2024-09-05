using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBLayer.Models
{
    public partial class HEvent
    {
        [Key]
        public long ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }       //cesta k obrazku, je špecifikovana v appsettings.json ako FileStorage
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }


    }

    public partial class UserHEvent     //prepojenie medzi userom a eventom - M:N
    {
        [Key]
        public long ID { get; set; }
        public HEvent HEvent { get; set; }
        public IdentityUserWebOwn User { get; set; }
    }
}
