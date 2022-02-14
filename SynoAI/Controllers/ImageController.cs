using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace SynoAI.Controllers
{
    public class ImageController : Controller
    {
        /// <summary>
        /// Returns the file for the specified camera.
        /// </summary>
        [Route("Image/{cameraName}/{filename}")]
        public ActionResult Get(string cameraName, string filename)
        {
            // Get and return the original Snapshot
            string path = Path.Combine(Constants.DIRECTORY_CAPTURES, cameraName, filename);
            byte[] originalSnapshot = System.IO.File.ReadAllBytes(path);
            return File(originalSnapshot, "image/jpeg");
        }
    }
}
