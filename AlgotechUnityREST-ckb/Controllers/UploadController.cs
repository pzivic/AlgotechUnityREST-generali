// Predrag Živić
// 17.09.2022

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;

using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace AlgotechUnityREST_ckb.Controllers
{
    /// <summary>
    /// Upload datoteke na server
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = false)]
    public class UploadController : ApiController
    {
        #region Metoda 1 - multipart/form-data
        // Metoda 1. https://learn.microsoft.com/en-us/aspnet/web-api/overview/advanced/sending-html-form-data-part-2
        // Metod 1.
        //// Check if the request contains multipart/form-data.
        //if (!Request.Content.IsMimeMultipartContent())
        //{
        //    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //}

        //string root = HttpContext.Current.Server.MapPath("~/App_Data/Uploads/");
        //var provider = new MultipartFormDataStreamProvider(root);

        //try
        //{
        //    // Read the form data.
        //    await Request.Content.ReadAsMultipartAsync(provider);

        //    // This illustrates how to get the file names.
        //    foreach (MultipartFileData file in provider.FileData)
        //    {
        //        Trace.WriteLine(file.Headers.ContentDisposition.FileName);
        //        Trace.WriteLine("Server file path: " + file.LocalFileName);
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, "Datoteke su uspešno prenete na server");
        //}
        //catch (System.Exception e)
        //{
        //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e);
        //}
        #endregion

        #region Metoda 2 - PostFormData
        //2. https://www.c-sharpcorner.com/UploadFile/2b481f/uploading-a-file-in-Asp-Net-web-api/

        /// <summary>
        /// Slanje datoteke na server 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [HttpPost]
        public HttpResponseMessage PostFormData()
        {
            HttpResponseMessage result;

            try
            {
                var uploadFolder = System.Configuration.ConfigurationManager.AppSettings["UploadFolder"];
                string filePath;

                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    var docfiles = new List<string>();
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        filePath = uploadFolder + @"\" + postedFile.FileName;

                        postedFile.SaveAs(filePath);
                        docfiles.Add(filePath);
                    }
                    result = Request.CreateResponse(HttpStatusCode.Created, "Datoteke su uspešno prenete na server.");
                }
                else
                {
                    result = Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

            return result;
        }


        // kod sa gui nazivom fajla
        //public HttpResponseMessage PostFormData()
        //{
        //    HttpResponseMessage result;

        //    try
        //    {
        //        var uploadFolder = System.Configuration.ConfigurationManager.AppSettings["UploadFolder"];
        //        if (!Directory.Exists(uploadFolder))
        //        {
        //            Directory.CreateDirectory(uploadFolder);
        //        }

        //        var httpRequest = HttpContext.Current.Request;
        //        if (httpRequest.Files.Count > 0)
        //        {
        //            var docfiles = new List<string>();
        //            foreach (string file in httpRequest.Files)
        //            {
        //                var postedFile = httpRequest.Files[file];
        //                string uniqueFileName = $"{Path.GetFileNameWithoutExtension(postedFile.FileName)}_{Guid.NewGuid()}{Path.GetExtension(postedFile.FileName)}";
        //                string filePath = Path.Combine(uploadFolder, uniqueFileName);

        //                postedFile.SaveAs(filePath);
        //                docfiles.Add(filePath);
        //            }
        //            result = Request.CreateResponse(HttpStatusCode.Created, "Datoteke su uspešno prenete na server.");
        //        }
        //        else
        //        {
        //            result = Request.CreateResponse(HttpStatusCode.BadRequest, "Nema datoteka za prijenos.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }

        //    return result;
        //}
        #endregion
    }
}
