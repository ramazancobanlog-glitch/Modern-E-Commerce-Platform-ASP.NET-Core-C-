using System.Collections.Generic;

namespace login.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Product>? Products { get; set; }
        public IEnumerable<Category>? Categories { get; set; }
    }
}