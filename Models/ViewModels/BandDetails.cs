using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _5204_PassionProject.Models.ViewModels
{
    public class BandDetails
    {
        /// <summary>
        /// The band
        /// </summary>
        public Bands band { get; set; }
        /// <summary>
        /// A list of all artists in this band
        /// </summary>
        public ICollection<Artists> artists { get; set; }
    }
}