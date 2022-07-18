using System;
using System.Collections.Generic;

namespace EquipmentRental.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Rentals = new HashSet<Rental>();
        }

        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public int RentalHours { get; set; }

        public virtual ICollection<Rental> Rentals { get; set; }
    }
}
