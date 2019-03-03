using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using MainApp.Implementations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace MainApp.Controllers
{
    public class LoadedController : Controller
    {
        private readonly ApplicationPartManager _partManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        public LoadedController(
            ApplicationPartManager partManager,
            IHostingEnvironment env)
        {
            _partManager = partManager;
            _hostingEnvironment = env;
        }

        public IActionResult RegisterControllerAtRuntime()
        {
            string assemblyPath = @"C:\Users\user\source\repos\TestNetCoreController\TestNetCoreController\bin\Debug\netcoreapp2.1\TestNetCoreController.dll";
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            if (assembly != null)
            {
                _partManager.ApplicationParts.Add(new AssemblyPart(assembly));
                // Notify change
                MyActionDescriptorChangeProvider.Instance.HasChanged = true;
                MyActionDescriptorChangeProvider.Instance.TokenSource.Cancel();
                return Content("1");
            }
            return Content("0");
        }
    }
}
