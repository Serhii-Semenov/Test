namespace ParsingCSV.Mapping
{
    public class Param 
    {
        public int Id { get; set; }

        public string NameParam { get; set; }

        public bool OnlyOne { get; set; }

        public bool RequiredField { get; set; }

        public bool LoadToDb { get; set; }

        public string queryAdditive { get; set; }

        public ChekField Field { get; set; }
    }
}
