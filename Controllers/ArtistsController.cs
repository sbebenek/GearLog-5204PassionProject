using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _5204_PassionProject.Models;
using _5204_PassionProject.Models.ViewModels;
using System.IO;



namespace _5204_PassionProject.Controllers
{
    public class ArtistsController : Controller
    {
        private _5204_PassionProjectContext db = new _5204_PassionProjectContext();

        // GET: Artists
        public ActionResult Index()
        {
            //grab all artists
            List<Artists> artists = db.Artists.SqlQuery("select * from Artists").ToList();
            //sort by artist first name (reference - https://www.codeproject.com/Tips/761275/How-to-Sort-a-List)
            artists.Sort(delegate (Artists x, Artists y)
            {
                return x.artistfname.CompareTo(y.artistfname);
            });
            //grab all bands
            List<Bands> bands = db.Bands.SqlQuery("select * from bands").ToList();
            //I only want bands that have artists in them, so take out bands without any artists
            List<Bands> bandsWArtists = new List<Bands>();
            foreach (Artists a in artists)
            {
                foreach (Bands b in bands)
                {
                    if(b.bandid == a.bandid && !bandsWArtists.Contains(b))
                    {
                        bandsWArtists.Add(b);
                    }
                }
            }
            //sort bands by band name
            bandsWArtists.Sort(delegate (Bands x, Bands y)
            {
                return x.bandname.CompareTo(y.bandname);
            });


            ListArtists viewModel = new ListArtists();
            viewModel.artists = artists;
            viewModel.bands = bandsWArtists;
            return View(viewModel);
        }

        // GET: Artists/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //getting the artist
            Artists art = db.Artists.SqlQuery("select * from Artists where artistid = @id", new SqlParameter("id", id)).FirstOrDefault();
            if (art == null)
            {
                return HttpNotFound();
            }
            //getting the band 
            Bands band = db.Bands.SqlQuery("select * from Bands where bandid = @id", new SqlParameter("id", art.bandid)).FirstOrDefault();
            //getting the instruments this artist plays
            string query = "select Instruments.* from Instruments inner join InstrumentsArtists on Instruments.instrumentid=InstrumentsArtists.Instruments_instrumentid " +
                " where InstrumentsArtists.Artists_artistid=" + art.artistid;
            List<Instruments> instruments = db.Instruments.SqlQuery(query).ToList();
            Debug.WriteLine("Instrument List = " + instruments.ToString());
            art.Instruments = instruments;
            ArtistDetails viewmodel = new ArtistDetails();
            viewmodel.artist = art;
            viewmodel.band = band;

            return View(viewmodel);
        }

        // GET: Artists/Create
        public ActionResult Create()
        {
            List<Bands> bands = db.Bands.SqlQuery("select * from bands").ToList();
            return View(bands);
        }
        
        // POST: Artists/Create
        [HttpPost]
        public ActionResult Create(string artistfname, string artistlname, int bandid)
        {
            Debug.WriteLine("bandid submitted to create = " + bandid);

            string query = "insert into Artists (artistfname, artistlname, artistimage, bandid) values (@artistfname,@artistlname,@artistimage,@bandid)";
            SqlParameter[] sqlparams = new SqlParameter[4]; //0,1,2,3 pieces of information to add
            //binding parameters
            sqlparams[0] = new SqlParameter("@artistfname", artistfname);
            sqlparams[1] = new SqlParameter("@artistlname", artistlname);
            sqlparams[2] = new SqlParameter("@artistimage", ""); //empty because of no file upload in the creation stage
            sqlparams[3] = new SqlParameter("@bandid", bandid);

            //run create, update, and delete sql commands
            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("Index", new { add = true });
        }
       

        // GET: Artists/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //grabbing the artist
            Artists art = db.Artists.SqlQuery("select * from Artists where artistid = @id", new SqlParameter("id", id)).FirstOrDefault();
            if (art == null)
            {
                return HttpNotFound();
            }

            //grabbing the band the artists is currently in (so it can be "selected" in the drop down menu)
            Bands currBand = db.Bands.SqlQuery("select * from Bands where bandid = @id", new SqlParameter("id", art.bandid)).FirstOrDefault(); ;
            if (currBand == null)
            {
                return HttpNotFound();
            }

            //grabbing all bands
            string query = "select * from bands";
            List<Bands> allBands = db.Bands.SqlQuery(query).ToList();

            //create instance of our viewmodel
            showArtist viewmodel = new showArtist();
            viewmodel.artist = art;
            viewmodel.currentBand = currBand;
            viewmodel.allBands = allBands;

            return View(viewmodel);
        }

        // POST: Artists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit(int artistid, string artistfname, string artistlname, HttpPostedFileBase artistimage, int bandid)
        {
            /**BELOW CODE BORROWED FROM CLASS EXAMPLE - https://github.com/christinebittle/PetGroomingMVC/blob/master/PetGrooming/Controllers/PetController.cs**/
            string artistPicExtension = "";
            //checking to see if an image was uploaded
            if (artistimage != null)
            {
                Debug.WriteLine("File was uploaded...");
                //checking to see if the file size is greater than 0 (bytes)
                if (artistimage.ContentLength > 0)
                {
                    Debug.WriteLine("Successfully Identified Image");
                    Debug.WriteLine("Image uploaded was " + artistimage.FileName);

                    //file extensioncheck taken from https://www.c-sharpcorner.com/article/file-upload-extension-validation-in-asp-net-mvc-and-javascript/
                    var valtypes = new[] { "jpeg", "jpg", "png", "gif" };
                    var extension = Path.GetExtension(artistimage.FileName).Substring(1);

                    if (valtypes.Contains(extension))
                    {
                        try
                        {
                            //file name is the id of the image
                            string fileName = artistid + "." + extension;

                            //get a direct file path to ~/Content/Artists/{id}.{extension}
                            string path = Path.Combine(Server.MapPath("~/Content/Artists/"), Path.GetFileName(fileName));

                            //save the file

                            //****@TODO: delete the old image if it exists, currently it will save new images with different filepaths instead of overwriting


                            artistimage.SaveAs(path); //will overwrite any existing file with this name (aka the old artist's image)
                            //if these are all successful then we can set these fields
                            artistPicExtension = fileName;
                            Debug.WriteLine("Saving image at " + path);


                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Artist Image was not saved successfully.");
                            Debug.WriteLine("Exception:" + ex);
                        }



                    }
                }
            }//else, no file was uploaded. Don't save anything new.
            else
            {
                //check if the artist currently has an image, so that it doesn't get set to null upon update
                Artists thisArtist = db.Artists.SqlQuery("select * from Artists where artistid = @id", new SqlParameter("id", artistid)).FirstOrDefault();
                if (thisArtist == null)
                {
                    Debug.WriteLine("Somehow artist wasn't found when checking if it had an image?");
                }
                //if this artist's image isn't null (aka it already has an image)
                else if(thisArtist.artistimage != null)
                {
                    artistPicExtension = thisArtist.artistimage;
                }
            }



            string query = "update Artists set artistfname=@fName, artistlname=@lName, artistimage=@image, bandid=@bandid where artistid=@id";
            SqlParameter[] sqlparams = new SqlParameter[5];
            sqlparams[0] = new SqlParameter("@fName", artistfname);
            sqlparams[1] = new SqlParameter("@lName", artistlname);
            sqlparams[2] = new SqlParameter("@bandid", bandid);
            sqlparams[3] = new SqlParameter("@id", artistid);
            sqlparams[4] = new SqlParameter("@image", artistPicExtension);

            Debug.Write("Setting artistimage = " + artistPicExtension);

            db.Database.ExecuteSqlCommand(query, sqlparams);
            return RedirectToAction("Index", new { update = true });
        }


        // POST: Artists/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            /** DELETE ALL RECORDS IN ARTISTSXINSTRUMENTS WITH THIS ARTIST ID**/
            Debug.WriteLine("deleting from InstrumentsArtists with artistid = " + id);

            string query = "delete from InstrumentsArtists where Artists_artistid = " + id;
            db.Database.ExecuteSqlCommand(query);

            /**PUT IMAGE FILE DELETION HERE**/
            //grab the artist record so that you can get the image
            Artists artist = db.Artists.SqlQuery("select * from Artists where artistid = @id", new SqlParameter("id", id)).FirstOrDefault();
            if (artist == null)
            {
                return HttpNotFound();
            }
            if (artist.artistimage != "")//if this record does have an image file
            {
                try
                {
                    // Check if file exists with its full path    
                    if (System.IO.File.Exists(Path.Combine("~/Content/Artists/", artist.artistimage)))
                    {
                        // If file found, delete it    
                        System.IO.File.Delete(Path.Combine("~/Content/Artists/", artist.artistimage));
                        Debug.WriteLine("File deleted.");
                    }
                    else Debug.WriteLine("File not found");
                }
                catch (IOException ioExp)
                {
                    Debug.WriteLine(ioExp.Message);
                }
            }

            /**NOW, DELETE THE ARTIST**/
            Debug.WriteLine("deleting from Artists where artistid = " + id);

            query = "delete from Artists where artistid = @id";
            SqlParameter[] sqlparams = new SqlParameter[1]; //0,1,2,3 pieces of information to add
            //binding parameters
            sqlparams[0] = new SqlParameter("@id", id);


            //run create, update, and delete sql commands
            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("Index", new { delete = true });
        }

        [HttpGet]
        public ActionResult showDetails(int instrumentid)
        {
            Debug.WriteLine("Receiving instrumentid as " + instrumentid);
            //grabbing the instrument
            Instruments inst = db.Instruments.SqlQuery("select * from Instruments where instrumentid = @id", new SqlParameter("id", instrumentid)).FirstOrDefault();
            return PartialView("_showInstrument", inst);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
