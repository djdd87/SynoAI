using Microsoft.AspNetCore.Mvc;

namespace SynoAI.Controllers
{
    /// <summary>
    /// Image Controller to get the specific file
    /// </summary>
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
