namespace SynoAI.Models.DTOs
{
    public class CameraOptionsDto : UpdateDto<CameraOptionsDto>
    {
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
