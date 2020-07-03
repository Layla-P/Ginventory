namespace Ginventory.Functions.Models
{
    public class Gin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GinBrandId { get; set; }
        public virtual GinBrand GinBrand { get; set; }
            
    }
}