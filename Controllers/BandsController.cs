using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _5204_PassionProject.Models;
using _5204_PassionProject.Models.ViewModels;

namespace _5204_PassionProject.Controllers
{
    public class BandsController : Controller
    {
        private _5204_PassionProjectContext db = new _5204_PassionProjectContext();

        // GET: Bands
        public ActionResult Index()
        {
            List<Bands> bands = db.Bands.SqlQuery("select * from Bands").ToList();
            return View(bands);
        }

        // GET: Bands/Details/5
        //Details page will show a list of authors in that band
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //getting the band
            Bands band = db.Bands.SqlQuery("select * from Bands where bandid = @id", new SqlParameter("id", id)).FirstOrDefault();
            if (band == null)
            {
                return HttpNotFound();
            }
            //getting the artists in this band (if any)
            string query = "select * from Artists where bandid=" + id;
            List<Artists> artists = db.Artists.SqlQuery(query).ToList();

            BandDetails viewModel = new BandDetails();
            viewModel.band = band;
            viewModel.artists = artists;

            return View(viewModel);
        }

        // GET: Bands/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Bands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string bandname)
        {
            string query = "insert into Bands (bandname, bandimage) values (@bandname,@bandimage)";
            SqlParameter[] sqlparams = new SqlParameter[2]; //0,1 pieces of information to add
            //binding parameters
            sqlparams[0] = new SqlParameter("@bandname", bandname);
            sqlparams[1] = new SqlParameter("@bandimage", ""); //empty because of no file upload in the creation stage

            //run create, update, and delete sql commands
            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("Index", new { add = true });
        }

        // GET: Bands/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }            

            //grabbing the band
            Bands band = db.Bands.SqlQuery("select * from Bands where bandid = @id", new SqlParameter("id", id)).FirstOrDefault(); ;
            if (band == null)
            {
                return HttpNotFound();
            }

            return View(band);
        }

        // POST: Bands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int bandid, string bandname, HttpPostedFileBase bandimage)
        {
            //BOOTLEG VALIDATION - use .net's built in validation next time
            if(bandname == "")
            {
                ViewBag.Message = 1; //band name field empty
                Bands band = db.Bands.SqlQuery("select * from Bands where bandid = @id", new SqlParameter("id", bandid)).FirstOrDefault(); ;
                return View(band);
            }
            


            /**BELOW CODE BORROWED FROM CLASS EXAMPLE - https://github.com/christinebittle/PetGroomingMVC/blob/master/PetGrooming/Controllers/PetController.cs**/
            string bandPicExtension = "";
            //checking to see if an image was uploaded
            if (bandimage != null)
            {
                Debug.WriteLine("File was uploaded...");
                //checking to see if the file size is greater than 0 (bytes)
                if (bandimage.ContentLength > 0)
                {
                    Debug.WriteLine("Successfully Identified Image");
                    Debug.WriteLine("Image uploaded was " + bandimage.FileName);

                    //file extensioncheck taken from https://www.c-sharpcorner.com/article/file-upload-extension-validation-in-asp-net-mvc-and-javascript/
                    var valtypes = new[] { "jpeg", "jpg", "png", "gif" };
                    var extension = Path.GetExtension(bandimage.FileName).Substring(1);

                    if (valtypes.Contains(extension))
                    {
                        try
                        {
                            //file name is the id of the image
                            string fileName = bandid + "." + extension;

                            //get a direct file path to ~/Content/Artists/{id}.{extension}
                            string path = Path.Combine(Server.MapPath("~/Content/Bands/"), Path.GetFileName(fileName));

                            //save the file

                            //****@TODO: delete the old image if it exists, currently it will save new images with different filepaths instead of overwriting


                            bandimage.SaveAs(path); //will overwrite any existing file with this name (aka the old artist's image)
                            //if these are all successful then we can set these fields
                            bandPicExtension = fileName;
                            Debug.WriteLine("Saving image at " + path);


                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Artist Image was not saved successfully.");
                            Debug.WriteLine("Exception:" + ex);
                        }



                    }
                    else //*VALIDATION* else, wrong filetype uploaded - reload form with error message
                    {
                        ViewBag.Message = 2; //wrong file type
                        Bands band = db.Bands.SqlQuery("select * from Bands where bandid = @id", new SqlParameter("id", bandid)).FirstOrDefault(); ;
                        return View(band);
                    }
                }
            }//else, no file was uploaded. Don't save anything new.
            else
            {
                //check if the band currently has an image, so that it doesn't get set to null upon update
                Bands thisBand = db.Bands.SqlQuery("select * from Bands where bandid = @id", new SqlParameter("id", bandid)).FirstOrDefault();
                if (thisBand == null)
                {
                    Debug.WriteLine("Somehow band wasn't found when checking if it had an image?");
                }
                //if this band's image isn't null (aka it already has an image)
                else if (thisBand.bandimage != null)
                {
                    bandPicExtension = thisBand.bandimage;
                }
            }



            string query = "update Bands set bandname=@name, bandimage=@image where bandid=@id";
            SqlParameter[] sqlparams = new SqlParameter[3];
            sqlparams[0] = new SqlParameter("@name", bandname);
            sqlparams[1] = new SqlParameter("@image", bandPicExtension);
            sqlparams[2] = new SqlParameter("@id", bandid);

            Debug.Write("Setting bandimage = " + bandPicExtension);

            db.Database.ExecuteSqlCommand(query, sqlparams);
            return RedirectToAction("Index", new { update = true });
        }

        // GET: Bands/Delete/5
        public ActionResult Delete(int id)
        {
            /**ALSO DELETE ALL ARTISTS IN THIS BAND AND THEIR LINKS TO THEIR INSTRUMENTS**/

            //selecting all artists with this bandid  in order to get all of their artistids
            string query = "select * from Artists where bandid = " + id;
            List<Artists> artists = db.Artists.SqlQuery(query).ToList();
            //if there are artists in that band
            if(artists.Any())
            {
                /**DELETE ALL ARTISTSxINSTRUMENTS RECORDS WITH THE ARTIST IDS THAT ARE IN THIS BAND**/
                string artistIds = "";
                foreach (var a in artists)
                {
                    artistIds += a.artistid.ToString() + ",";
                }
                artistIds = artistIds.Remove(artistIds.Length - 1); //removing the last comma in the list
                //deleting all artistids from the bridging table in the list
                Debug.WriteLine("deleting from Artists and InstrumentsArtists with artistids = " + artistIds);

                query = "delete from InstrumentsArtists where Artists_artistid IN (" + artistIds + ")";
                db.Database.ExecuteSqlCommand(query);


                /**DELETE ALL ARTISTS WITH THIS BAND TOO**/
                query = "delete from Artists where artistid IN (" + artistIds + ")";
                db.Database.ExecuteSqlCommand(query);

            }

            /**PUT IMAGE FILE DELETION HERE**/
            //grab the band record so that you can get the image
            Bands band = db.Bands.SqlQuery("select * from Bands where band = @id", new SqlParameter("id", id)).FirstOrDefault();
            if (band == null)
            {
                return HttpNotFound();
            }
            if (band.bandimage != "")//if this record does have an image file
            {
                try
                {
                    // Check if file exists with its full path    
                    if (System.IO.File.Exists(Path.Combine("~/Content/Bands/", band.bandimage)))
                    {
                        // If file found, delete it    
                        System.IO.File.Delete(Path.Combine("~/Content/Bands/", band.bandimage));
                        Debug.WriteLine("File deleted.");
                    }
                    else Debug.WriteLine("File not found");
                }
                catch (IOException ioExp)
                {
                    Debug.WriteLine(ioExp.Message);
                }
            }

            /**NOW DELETE THE BAND**/

            Debug.WriteLine("deleting from Bands where bandid = " + id);

            query = "delete from Bands where bandid = @id";
            SqlParameter[] sqlparams = new SqlParameter[1]; //0,1,2,3 pieces of information to add
            //binding parameters
            sqlparams[0] = new SqlParameter("@id", id);


            //run create, update, and delete sql commands
            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("Index", new { delete = true });
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
