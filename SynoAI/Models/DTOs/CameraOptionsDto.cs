namespace SynoAI.Models.DTOs
{
    /// <summary>
    /// Class for the Camera Options
    /// </summary>
    public class CameraOptionsDto : UpdateDto<CameraOptionsDto>
    {
        /// <summary>
        /// Boolean for camera enablement
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                NotifyPropertyChange();
                _enabled = value;
            }
        }
        private bool _enabled;
    }
}
