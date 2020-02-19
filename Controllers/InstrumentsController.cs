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
    public class InstrumentsController : Controller
    {
        private _5204_PassionProjectContext db = new _5204_PassionProjectContext();

        // GET: Instruments
        public ActionResult Index()
        {
            List<Instruments> instruments = db.Instruments.SqlQuery("select * from Instruments").ToList();
            return View(instruments);
        }

        // GET: Instruments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //getting the instrument
            Instruments instrument = db.Instruments.SqlQuery("select * from Instruments where instrumentid = @id", new SqlParameter("id", id)).FirstOrDefault();
            if (instrument == null)
            {
                return HttpNotFound();
            }
            //getting the artists that are matched with this instrument in the bridging table
            string query = "select Artists.* from Artists inner join InstrumentsArtists on Artists.artistid=InstrumentsArtists.Artists_artistid " +
                " where InstrumentsArtists.Instruments_instrumentid=" + id;
            List<Artists> artists = db.Artists.SqlQuery(query).ToList();
            Debug.WriteLine("Artists that play this instrument = ");
            foreach(var a in artists)
            {
                Debug.WriteLine(a.artistfname + " " + a.artistlname);
            }


            //getting all artists EXCEPT for those already associated with this instrument (to prevent duplicates)
            //if there are artists matched, grab everyone else who isn't matched
            List<Artists> allArtists;
            if (artists.Any())
            {
                string artistIds = "";
                foreach (var a in artists)
                {
                    artistIds += a.artistid.ToString() + ",";
                }
                artistIds = artistIds.Remove(artistIds.Length - 1); //removing the last comma in the list
                query = "select * from Artists where artistid NOT IN (" + artistIds + ")";
                allArtists = db.Artists.SqlQuery(query).ToList();

            }
            else //there are no artists matched with this instrument, so grab everybody
            {
                allArtists = db.Artists.SqlQuery("select* from Artists").ToList();
            }






            InstrumentDetails viewModel = new InstrumentDetails();
            viewModel.instrument = instrument;
            viewModel.artists = artists;
            viewModel.allArtists = allArtists;

            return View(viewModel);
        }

        // GET: Instruments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Instruments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string instrumentbrand, string instrumenttitle, string instrumenttype, int instrumentstrings, string instrumentbody,
            string instrumentneck, string instrumentfretboard, string instrumentbridge, string instrumentneckpu, string instrumentbridgepu,
            string instrumentfeatures, string instrumenturl)
        {
            //add http:// to the website url if its not given (reference https://stackoverflow.com/questions/5289739/add-scheme-to-url-if-needed)
            if (instrumenturl != "" && !instrumenturl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                instrumenturl = new UriBuilder(instrumenturl).Uri.ToString();
            }
            Debug.WriteLine("URL converted to " + instrumenturl);

            string query = "insert into Instruments (instrumentbrand, instrumenttitle, instrumenttype, instrumentstrings, instrumentbody, " +
            "instrumentneck, instrumentfretboard, instrumentbridge, instrumentneckpu, instrumentbridgepu, instrumentfeatures, instrumenturl," +
            "instrumentimage) values (@instrumentbrand,@instrumenttitle,@instrumenttype,@instrumentstrings,@instrumentbody,@instrumentneck," +
            "@instrumentfretboard,@instrumentbridge,@instrumentneckpu,@instrumentbridgepu,@instrumentfeatures,@instrumenturl,@instrumentimage)";
            SqlParameter[] sqlparams = new SqlParameter[13]; //0,1 pieces of information to add
            //binding parameters
            sqlparams[0] = new SqlParameter("@instrumentbrand", instrumentbrand);
            sqlparams[1] = new SqlParameter("@instrumenttitle", instrumenttitle);
            sqlparams[2] = new SqlParameter("@instrumenttype", instrumenttype);
            sqlparams[3] = new SqlParameter("@instrumentstrings", instrumentstrings);
            sqlparams[4] = new SqlParameter("@instrumentbody", instrumentbody);
            sqlparams[5] = new SqlParameter("@instrumentneck", instrumentneck);
            sqlparams[6] = new SqlParameter("@instrumentfretboard", instrumentfretboard);
            sqlparams[7] = new SqlParameter("@instrumentbridge", instrumentbridge);
            sqlparams[8] = new SqlParameter("@instrumentneckpu", instrumentneckpu);
            sqlparams[9] = new SqlParameter("@instrumentbridgepu", instrumentbridgepu);
            sqlparams[10] = new SqlParameter("@instrumentfeatures", instrumentfeatures);
            sqlparams[11] = new SqlParameter("@instrumenturl", instrumenturl);
            sqlparams[12] = new SqlParameter("@instrumentimage", ""); //empty at the creation stage

            //run create, update, and delete sql commands
            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("Index", new { add = true });
        }

        // GET: Instruments/Edit/5
        public ActionResult Edit(int? id)
        {
            //grabbing the instrument
            Instruments instrument = db.Instruments.SqlQuery("select * from Instruments where instrumentid = @id", new SqlParameter("id", id)).FirstOrDefault();
            if (instrument == null)
            {
                return HttpNotFound();
            }

            return View(instrument);
        }

        // POST: Instruments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int instrumentid, HttpPostedFileBase instrumentimage, string instrumentbrand, string instrumenttitle, string instrumenttype, int instrumentstrings, string instrumentbody,
            string instrumentneck, string instrumentfretboard, string instrumentbridge, string instrumentneckpu, string instrumentbridgepu,
            string instrumentfeatures, string instrumenturl)
        {
            /**BELOW CODE BORROWED FROM CLASS EXAMPLE - https://github.com/christinebittle/PetGroomingMVC/blob/master/PetGrooming/Controllers/PetController.cs**/
            string instrumentPicExtension = "";
            //checking to see if an image was uploaded
            if (instrumentimage != null)
            {
                Debug.WriteLine("File was uploaded...");
                //checking to see if the file size is greater than 0 (bytes)
                if (instrumentimage.ContentLength > 0)
                {
                    Debug.WriteLine("Successfully Identified Image");
                    Debug.WriteLine("Image uploaded was " + instrumentimage.FileName);

                    //file extensioncheck taken from https://www.c-sharpcorner.com/article/file-upload-extension-validation-in-asp-net-mvc-and-javascript/
                    var valtypes = new[] { "jpeg", "jpg", "png", "gif" };
                    var extension = Path.GetExtension(instrumentimage.FileName).Substring(1);

                    if (valtypes.Contains(extension))
                    {
                        try
                        {
                            //file name is the id of the image
                            string fileName = instrumentid + "." + extension;

                            //get a direct file path to ~/Content/Artists/{id}.{extension}
                            string path = Path.Combine(Server.MapPath("~/Content/Instruments/"), Path.GetFileName(fileName));

                            //save the file

                            //****@TODO: delete the old image if it exists, currently it will save new images with different filepaths instead of overwriting


                            instrumentimage.SaveAs(path); //will overwrite any existing file with this name (aka the old artist's image)
                            //if these are all successful then we can set these fields
                            instrumentPicExtension = fileName;
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
                //check if the instrument currently has an image, so that it doesn't get set to null upon update
                Instruments thisInstrument = db.Instruments.SqlQuery("select * from Instruments where instrumentid = @id", new SqlParameter("id", instrumentid)).FirstOrDefault();
                if (thisInstrument == null)
                {
                    Debug.WriteLine("Somehow instrument wasn't found when checking if it had an image?");
                }
                //if this artist's image isn't null (aka it already has an image)
                else if (thisInstrument.instrumentimage != null)
                {
                    instrumentPicExtension = thisInstrument.instrumentimage;
                }
            }


            string query = "update Instruments set instrumentbrand = @instrumentbrand, instrumenttitle = @instrumenttitle, " +
                "instrumenttype = @instrumenttype, instrumentstrings = @instrumentstrings, instrumentbody = @instrumentbody, " +
                "instrumentneck = @instrumentneck, instrumentfretboard = @instrumentfretboard, instrumentbridge = @instrumentbridge, " +
                "instrumentneckpu = @instrumentneckpu, instrumentbridgepu = @instrumentbridgepu, instrumentfeatures = @instrumentfeatures, " +
                "instrumenturl = @instrumenturl, instrumentimage = @instrumentimage where instrumentid=@id";
            SqlParameter[] sqlparams = new SqlParameter[14]; //0,1 pieces of information to add
            //binding parameters
            sqlparams[0] = new SqlParameter("@instrumentbrand", instrumentbrand);
            sqlparams[1] = new SqlParameter("@instrumenttitle", instrumenttitle);
            sqlparams[2] = new SqlParameter("@instrumenttype", instrumenttype);
            sqlparams[3] = new SqlParameter("@instrumentstrings", instrumentstrings);
            sqlparams[4] = new SqlParameter("@instrumentbody", instrumentbody);
            sqlparams[5] = new SqlParameter("@instrumentneck", instrumentneck);
            sqlparams[6] = new SqlParameter("@instrumentfretboard", instrumentfretboard);
            sqlparams[7] = new SqlParameter("@instrumentbridge", instrumentbridge);
            sqlparams[8] = new SqlParameter("@instrumentneckpu", instrumentneckpu);
            sqlparams[9] = new SqlParameter("@instrumentbridgepu", instrumentbridgepu);
            sqlparams[10] = new SqlParameter("@instrumentfeatures", instrumentfeatures);
            sqlparams[11] = new SqlParameter("@instrumenturl", instrumenturl);
            sqlparams[12] = new SqlParameter("@instrumentimage", instrumentPicExtension);
            sqlparams[13] = new SqlParameter("@id", instrumentid);


            Debug.Write("Setting instrumentimage = " + instrumentPicExtension);

            db.Database.ExecuteSqlCommand(query, sqlparams);
            return RedirectToAction("Index", new { update = true });
        }

        

        // POST: Instruments/Delete/5
        public ActionResult Delete(int id)
        {
            /** DELETE ALL RECORDS IN ARTISTSXINSTRUMENTS WITH THIS INSTRUMENT ID**/
            Debug.WriteLine("deleting from InstrumentsArtists with instrumentid = " + id);

            string query = "delete from InstrumentsArtists where Instruments_instrumentid = " + id;
            db.Database.ExecuteSqlCommand(query);

            /**PUT IMAGE FILE DELETION HERE**/
            //grab the instrument record so that you can get the image
            Instruments inst = db.Instruments.SqlQuery("select * from Instruments where instrumentid = @id", new SqlParameter("id", id)).FirstOrDefault();
            if (inst == null)
            {
                return HttpNotFound();
            }
            if (inst.instrumentimage != "")//if this record does have an image file
            {
                try
                {
                    // Check if file exists with its full path    
                    if (System.IO.File.Exists(Path.Combine("~/Content/Instruments/", inst.instrumentimage)))
                    {
                        // If file found, delete it    
                        System.IO.File.Delete(Path.Combine("~/Content/Instruments/", inst.instrumentimage));
                        Debug.WriteLine("File deleted.");
                    }
                    else Debug.WriteLine("File not found");
                }
                catch (IOException ioExp)
                {
                    Debug.WriteLine(ioExp.Message);
                }
            }

            /**NOW, DELETE THE INSTRUMENT**/
            Debug.WriteLine("deleting from Instruments where instrumentid = " + id);

            query = "delete from Instruments where instrumentid = @id";
            SqlParameter[] sqlparams = new SqlParameter[1]; //0,1,2,3 pieces of information to add
            //binding parameters
            sqlparams[0] = new SqlParameter("@id", id);


            //run create, update, and delete sql commands
            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("Index", new { delete = true });
        }

        //***ADDING ARTISTS TO INSTRUMENTS***//
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details(int? instrumentid, int? artistid)
        {
            Debug.WriteLine("Adding InstrumentsArtists record! Instrument ID submitted = " + instrumentid + ". artist ID submitted = " + artistid);
            if (instrumentid == null || artistid == null)
            {
                Debug.WriteLine("instrument or artist id is null");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instruments instruments = db.Instruments.Find(instrumentid);
            Artists artists = db.Artists.Find(artistid);
            if (instruments == null)
            {
                Debug.WriteLine("No instrument found with the given instrumentid");
                return HttpNotFound();
            }
            if (artists == null)
            {
                Debug.WriteLine("No artist found with the given artistid");
                return HttpNotFound();
            }

            string query = "insert into InstrumentsArtists (Instruments_instrumentid, Artists_artistid) values (@instrumentid,@artistid)";
            SqlParameter[] sqlparams = new SqlParameter[2]; 
            //binding parameters
            sqlparams[0] = new SqlParameter("@instrumentid", instrumentid);
            sqlparams[1] = new SqlParameter("@artistid", artistid);
            
            //run create, update, and delete sql commands
            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("Details", new { id = instrumentid });
        }

        public ActionResult DeleteArtistInstrument(int instrumentid, int artistid)
        {
            Debug.WriteLine("Deleting InstrumentsArtists record! Instrument ID submitted = " + instrumentid + ". artist ID submitted = " + artistid);

            string query = "delete from InstrumentsArtists where Instruments_instrumentid = @instrumentid AND Artists_artistid = @artistid";
            SqlParameter[] sqlparams = new SqlParameter[2]; //0,1,2,3 pieces of information to add
            //binding parameters
            sqlparams[0] = new SqlParameter("@instrumentid", instrumentid);
            sqlparams[1] = new SqlParameter("@artistid", artistid);
            //run create, update, and delete sql commands
            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("Details", new { id = instrumentid });
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
