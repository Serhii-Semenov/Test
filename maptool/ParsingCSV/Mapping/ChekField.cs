using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParsingCSV.Mapping
{
     public class ChekField
    {
        public bool TextField { get; set; }
        public bool NotEmpty { get; set; }
        public bool NumericField { get; set; }
        public bool NeedVerification { get; set; }
    }
}
