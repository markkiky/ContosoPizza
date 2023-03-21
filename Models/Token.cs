namespace ContosoPizza.Models
{
    public class Token
    {
        public int Id { get; set; }

        public string? Type { get; set; }
        public string? Name { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int CompanyId { get; set; }

        public virtual Company? Company { get; set; }
    }
}
