using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParsingCSV.Mapping
{
    public class Err
    {
        public string OnlyOne { get; set; } = "";
        public string RequiredField { get; set; } = "";
        public string IsText { get; set; } = "";
        public string IsNuber{ get; set; } = "";
        public string IsEmpty { get; set; } = "";
        public string SKUplusBrand { get; set; } = "";

        public bool GetError()
        {
            bool b = true;
            if (OnlyOne == "" &&
                RequiredField == "" &&
                IsText == "" &&
                IsNuber == "" &&
                IsEmpty == "" &&
                SKUplusBrand == "") b = false;
            return b;
        }
    }
}
