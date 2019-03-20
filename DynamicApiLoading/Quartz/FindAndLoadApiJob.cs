using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Quartz;

namespace DynamicApiLoading.Quartz
{
    public class FindAndLoadApiJob: IJob
    {
        private readonly ApplicationPartManager _partManager;
        private readonly MyActionDescriptorChangeProvider _actionDescriptorChangeProvider;

        public FindAndLoadApiJob(ApplicationPartManager partManager, MyActionDescriptorChangeProvider actionDescriptorChangeProvider)
        {
            _partManager = partManager;
            _actionDescriptorChangeProvider = actionDescriptorChangeProvider;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var pluginAssembly = Assembly.LoadFrom(new FileInfo(@"..\PluginController\bin\Debug\netcoreapp2.2\PluginController.dll").FullName);
            if (_partManager.ApplicationParts.Any(p => p.Name == pluginAssembly.GetName().Name))
                return Task.CompletedTask;
            _partManager.ApplicationParts.Add(new AssemblyPart(pluginAssembly));

            _actionDescriptorChangeProvider.HasChanged = true;
            _actionDescriptorChangeProvider.TokenSource.Cancel();

//            var controllerFeature = new ControllerFeature();
//            _partManager.PopulateFeature(controllerFeature);

            return Task.CompletedTask;
        }
    }
}
