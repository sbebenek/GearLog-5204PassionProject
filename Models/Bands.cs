using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _5204_PassionProject.Models
{
    public class Bands
    {
        [Key]
        public int bandid { get; set; }
        public string bandname { get; set; }
        public string bandimage { get; set; }
    }
}