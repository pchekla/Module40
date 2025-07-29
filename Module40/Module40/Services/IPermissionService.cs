using System.Threading.Tasks;

namespace Module40.Services
{
    public interface IPermissionService
    {
        Task<bool> CheckAndRequestStoragePermissionAsync();
        bool CanRequestPermission();
        void OpenAppSettings();
    }
}
