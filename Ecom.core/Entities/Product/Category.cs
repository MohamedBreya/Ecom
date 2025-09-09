using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.core.Entities.Product
{
    public class Category:BaseEntity<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }

       
        //public ICollection<Product> products { get; set; }=new List<Product>();  -----> (Gives Error So I Commented It)
    }
}
