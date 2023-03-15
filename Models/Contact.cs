using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoPizza.Models
{
    public class Contact
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }
        public string? LedgerId { get; set; }
        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
}
