using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _5204_PassionProject.Models.ViewModels
{
    public class InstrumentDetails
    {
        /// <summary>
        /// The instrument
        /// </summary>
        public Instruments instrument { get; set; }
        /// <summary>
        /// A list of all artists associated with this instrument
        /// </summary>
        public ICollection<Artists> artists { get; set; }
        /// <summary>
        /// A list of all artists 
        /// </summary>
        public ICollection<Artists> allArtists { get; set; }
    }
}