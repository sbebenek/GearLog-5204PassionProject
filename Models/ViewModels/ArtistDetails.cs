using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _5204_PassionProject.Models.ViewModels
{
    /// <summary>
    /// A ViewModel class for the artist details page containing the artist, their band, and the instruments they play
    /// </summary>
    public class ArtistDetails
    {
        public Artists artist { get; set; }
        //the band this artist is currently in
        public Bands band { get; set; }

        //@TODO: Add amps and pedals ICollection either here or in Artists model
    }
}