using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EquipmentRental.Models;

namespace EquipmentRental.Controllers
{
    public class RentalsController : Controller
    {
        private readonly RentalDBContext _context;

        public RentalsController(RentalDBContext context)
        {
            _context = context;
        }

        // GET: Rentals
        public async Task<IActionResult> Index(string sortOrder)
        { 

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.EquipSortParm = sortOrder == "equipment" ? "equipment_desc" : "equipment";
            var rentals = await _context.Rentals.Include(x => x.Customer).Include(y => y.Equipment).AsNoTracking().ToListAsync();
            switch (sortOrder)
            {
                case "name_desc":
                    rentals = rentals.OrderByDescending(s => s.Customer.UserName).ToList();
                    break;
                case "equipment":
                    rentals = rentals.OrderBy(s => s.Equipment.Name).ToList();
                    break;
                case "equipment_desc":
                    rentals = rentals.OrderByDescending(s => s.Equipment.Name).ToList();
                    break;
                default:
                    rentals = rentals.OrderBy(s => s.Customer.UserName).ToList();
                    break;
            }
            return View(rentals);

        }
        public async Task<IActionResult> EndRental(int? id)
        {
            if (id == null || _context.Rentals == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }
            rental.IsCurrentRental = 0;
            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Rentals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Rentals == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals.Include(x => x.Customer).Include(y => y.Equipment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // GET: Rentals/Create
        public IActionResult Create()
        {
            List<Customer> custs = _context.Customers.AsNoTracking().ToList(); 
            ViewData["Customers"] = new SelectList(custs, "Id", "UserName");
            ViewData["Equipments"] = new SelectList(_context.Equipment, "Id", "Name"); 
            
            return View();
        }
         
        // POST: Rentals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,EquipmentId,RentalHours,IsCurrentRental,Quantity")] Rental rental)
        {
            try
            {
                ModelState.Remove("IsCurrentRental");
                ModelState.Remove("Customer");
                ModelState.Remove("Equipment");
                if (ModelState.IsValid)
                {
                    Equipment equip = _context.Equipment.Where(x => x.Id == rental.EquipmentId).FirstOrDefault();
                    Customer customer = _context.Customers.Where(x => x.Id == rental.CustomerId).FirstOrDefault();
                    if (rental.Quantity > equip.Copies)
                    {
                        List<Customer> custs = _context.Customers.AsNoTracking().ToList(); 
                        ViewData["Customers"] = new SelectList(custs, "Id", "UserName");
                        ViewData["Equipments"] = new SelectList(_context.Equipment, "Id", "Name");
                        ViewData["Error"] = "Only " + equip.Copies + " copies left for " + equip.Name + ". Please enter rental quantity less than " + equip.Copies + ".";
                        return View(rental);
                    }
                    if (rental.RentalHours == 0 || (rental.RentalHours > customer.RentalHours))
                    {
                        List<Customer> custs = _context.Customers.AsNoTracking().ToList();
                        ViewData["Customers"] = new SelectList(custs, "Id", "UserName");
                        ViewData["Equipments"] = new SelectList(_context.Equipment, "Id", "Name");
                        ViewData["Error"] = "Customer:  " + customer.UserName + " can rent an equipment for max " + customer.RentalHours + ". Please enter rental hour between 1 to " + customer.RentalHours + ".";
                        return View(rental);
                    }
                    rental.IsCurrentRental = 1;
                    _context.Add(rental);
                    await _context.SaveChangesAsync();

                    // Update Equipment Copies after successful rental
                    equip.Copies = equip.Copies - rental.Quantity.Value;
                    _context.Equipment.Update(equip); 
                    _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
            }
            List<Customer> custss = _context.Customers.AsNoTracking().ToList(); 
            ViewData["Customers"] = new SelectList(custss, "Id", "UserName");
            ViewData["Equipments"] = new SelectList(_context.Equipment, "Id", "Name");
 
            return View(rental);
        }

        // GET: Rentals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Rentals == null)
            {
                return NotFound();
            }

            var rental =  _context.Rentals.Include(x => x.Customer).Include(y => y.Equipment).Where(x=>x.Id == id).AsNoTracking().FirstOrDefault();
            if (rental == null)
            {
                return NotFound();
            }
            List<Customer> custs = _context.Customers.AsNoTracking().ToList();
            ViewData["Customers"] = new SelectList(custs, "Id", "UserName");
            ViewData["Equipments"] = new SelectList(_context.Equipment, "Id", "Name");

            return View(rental);
        }

        // POST: Rentals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,EquipmentId,RentalHours,IsCurrentRental,Quantity")] Rental rental)
        {
            if (id != rental.Id)
            {
                return NotFound();
            }

            ModelState.Remove("IsCurrentRental");
            ModelState.Remove("Customer");
            ModelState.Remove("Equipment");
            if (ModelState.IsValid)
            {
                try
                {

                    _context.Update(rental);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalExists(rental.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            List<Customer> custs = _context.Customers.AsNoTracking().ToList();
            ViewData["Customers"] = new SelectList(custs, "Id", "UserName");
            ViewData["Equipments"] = new SelectList(_context.Equipment, "Id", "Name");

            return View(rental);
        }

        // GET: Rentals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Rentals == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals.Include(x => x.Customer).Include(y => y.Equipment).AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: Rentals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Rentals == null)
            {
                return Problem("Entity set 'RentalDBContext.Rentals'  is null.");
            }
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RentalExists(int id)
        {
          return (_context.Rentals?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
