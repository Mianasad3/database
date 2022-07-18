using System;
using System.Collections.Generic;

namespace EquipmentRental.Models
{
    public partial class Rental
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int EquipmentId { get; set; }
        public int RentalHours { get; set; }
        public int IsCurrentRental { get; set; }
        public int? Quantity { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Equipment Equipment { get; set; } = null!;
    }
}
