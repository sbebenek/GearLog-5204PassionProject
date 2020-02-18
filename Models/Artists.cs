using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _5204_PassionProject.Models
{
    public class Artists
    {
        [Key]
        public int artistid { get; set; }
        public string artistfname { get; set; }
        public string artistlname { get; set; }

        //extension for image, Content/Artists/artistimage
        public string artistimage { get; set; }
        public int bandid { get; set; }
        [ForeignKey("bandid")]
        public virtual Bands Bands { get; set; }
        //collection of instruments for this artist
        public ICollection<Instruments> Instruments { get; set; }

    }
}