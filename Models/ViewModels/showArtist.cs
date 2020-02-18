using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _5204_PassionProject.Models.ViewModels
{
    /// <summary>
    /// A ViewModel class for the Edit Artists page containing the artist, their band, and a list of all bands
    /// </summary>
    public class showArtist
    {
        public Artists artist { get; set; }
        //the band this artist is currently in
        public Bands currentBand { get; set; }
        //a list of all bands
        public ICollection<Bands> allBands { get; set; }
    }
}