using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure.WebJobs.ServiceBus;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace MyFrameworkWebjob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            string storageConnection = ConfigurationManager.AppSettings["AzureWebJobsStorage"];
            string sbConn = ConfigurationManager.AppSettings["ServiceBusConnection"];


            JobHostConfiguration config = new JobHostConfiguration
            {
                DashboardConnectionString = storageConnection,
                JobActivator = new JobActivator(SetupContainer())

            };

            if (config.IsDevelopment)
                config.UseDevelopmentSettings();

            config.UseServiceBus(new ServiceBusConfiguration()
            {
                ConnectionString = sbConn,
                MessageOptions = new OnMessageOptions()
                {
                    MaxConcurrentCalls = int.Parse(ConfigurationManager.AppSettings["MaxCalls"]),
                }
            });

            JobHost host = new JobHost(config);

            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
        public static Container SetupContainer()
        {
            Container container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Options.AllowOverridingRegistrations = true;

            //DomainClassRegistrations.Register(container);

            container.Verify();

            return container;
        }

    }
    public class JobActivator : IJobActivator
    {
        private Container _container;

        public JobActivator(Container container)
        {
            _container = container;
        }

        public T CreateInstance<T>()
        {
            return (T)_container.GetInstance(typeof(T));
        }
    }
}
