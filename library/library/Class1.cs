using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace library
{
    [Serializable]
    public class Product
    {
        public string name;
        public double price;
        public int type_of_trans;
        public int ClientNumber;
        
        public Product()
        { }       
    }
}
