using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _5204_PassionProject.Models.ViewModels
{
    public class ListArtists
    {
        /// <summary>
        /// A list of all artists
        /// </summary>
        public ICollection<Artists> artists { get; set; }
        /// <summary>
        /// A list of all bands that currently contain artists
        /// </summary>
        public ICollection<Bands> bands { get; set; }

    }
}