using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _5204_PassionProject.Models
{
    public class Instruments
    {
        [Key]
        public int instrumentid { get; set; }
        //file extension for the image
        public string instrumentimage { get; set; }
        public string instrumentbrand { get; set; }
        public string instrumenttitle { get; set; }
        public string instrumenttype { get; set; }
        public int instrumentstrings { get; set; }
        public string instrumentbody { get; set; }
        public string instrumentneck { get; set; }
        public string instrumentfretboard { get; set; }
        public string instrumentbridge { get; set; }
        public string instrumentneckpu { get; set; }
        public string instrumentbridgepu { get; set; }
        public string instrumentfeatures { get; set; }
        public string instrumenturl { get; set; }




        //collection of artists for this instrument
        //will grab from InstrumentsxArtists but a seperate view and controller function will be needed to create/delete
        public ICollection<Artists> Artists { get; set; }

    }
}