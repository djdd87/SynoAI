using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Controllers
{
    /// <summary>
    /// Controller triggered on a motion alert from synology, which will act as a bridge between the Synology camera and DeepStack AI.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CameraController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CameraController> _logger;

        public CameraController(ILogger<CameraController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Called by the Synology motion alert hook.
        /// </summary>
        /// <param name="id">The name of the camera.</param>
        [HttpGet]
        [Route("{id}")]
        public void Get(string id)
        {
            _logger.LogInformation($"{id}: Starting check...");


            _logger.LogInformation($"{id}: Fetching snapshot...");

            _logger.LogInformation($"{id}: Offloading to DeepStackAI...");


            _logger.LogInformation($"{id}: Result received. Threshold result %...");


            _logger.LogInformation($"{id}: Sending alert. Threshold exceeded...");


            _logger.LogInformation($"{id}: Ignoring alert. Threshold not exceded...");


        }
    }
}
