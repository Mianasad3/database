using System;
using System.Collections.Generic;

namespace EquipmentRental.Models
{
    public partial class Equipment
    {
        public Equipment()
        {
            Rentals = new HashSet<Rental>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Copies { get; set; }

        public virtual ICollection<Rental> Rentals { get; set; }
    }
}
