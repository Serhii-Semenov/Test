namespace ParsingCSV.Mapping
{
    public class ColmnMapping
    {
        public string ColmnName { get; private set; }
        public Param Parameter { get; set; }
        public string Examples { get; set; }
        public string Error { get; set; } 
        public Err Errors { get; set; }

        public ColmnMapping(string ColmnName, Param Parameter, string ex, Err Errors)
        {
            this.ColmnName = ColmnName;
            this.Parameter = Parameter;
            Examples = ex;
            Error = "";
            this.Errors = Errors;
        }

        public void GetError()
        {
            if (!Errors.GetError())
            {
                Error = "";
                return;
            }
            string e = "";
            if (Errors.IsEmpty != "") e += " /" + Errors.IsEmpty + "/ ";
            if (Errors.IsNuber != "") e += "/" + Errors.IsNuber + "/";
            if (Errors.IsText != "") e += "/" + Errors.IsText + "/";
            if (Errors.OnlyOne != "" && Parameter.OnlyOne) e += "/" + Errors.OnlyOne + "/";
            if (Errors.RequiredField != "") e += "/" + Errors.RequiredField + "/";
            if (Errors.SKUplusBrand != "") e += "/" + Errors.SKUplusBrand + "/";
            Error = e;
        }
    }
}
