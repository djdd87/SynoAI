using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace SynoAI.Controllers
{
    public class ImageController : Controller
    {
        /// <summary>
        /// Return snapshot image as JPEG, either in original size or a scaled down version, if asked.
        //// In order to use System.Drawing.Common
        //// In Terminal, issue: dotnet add SynoAI package System.Drawing.Common
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
