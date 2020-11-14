using ApplicationManager.Storage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace X_Server.Pages
{
    public class ApplicationsModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ApplicationInfo> Applications { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        private readonly ApplicationManager.Storage.Manager _storage;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="storage"></param>
        public ApplicationsModel(ApplicationManager.Storage.Manager storage)
        {
            _storage = storage;
        }


        /// <summary>
        /// 
        /// </summary>
        public void OnGet()
        {
            Applications = _storage.GetApplications();
        }


        /// <summary>
        /// 
        /// </summary>
        public async Task OnPostInstallAsync(string name)
        {
            await _storage.InstallAsync(name);
            Applications = _storage.GetApplications();
        }


        /// <summary>
        /// 
        /// </summary>
        public async Task OnPostUninstallAsync(string name)
        {
            await _storage.UninstallAsync(name);
            Applications = _storage.GetApplications();
        }
    }
}