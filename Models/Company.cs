using System.ComponentModel.DataAnnotations;

namespace ContosoPizza.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? erp { get; set; }

        public string? LedgerId { get; set; }
        
        public bool IsActive { get; set; }

        public bool MultiCurrency { get; set; }

        public bool FullUpdate { get; set; }

        public DateTime? SyncDate { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
    }

    public enum Erp
    {
        Zoho, QuickBooksOnlineSandbox, QuickBooksOnline
    }
}
