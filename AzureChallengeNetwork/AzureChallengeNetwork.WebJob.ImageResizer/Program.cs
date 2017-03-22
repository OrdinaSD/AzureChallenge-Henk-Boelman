using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace AzureChallengeNetwork.WebJob.ImageResizer
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage AzureWebJobsServiceBus
        static void Main()
        {
         //   string connectionString = ConfigurationManager.ConnectionStrings["AzureWebJobsServiceBus"].ConnectionString;
        //    string queueName = "nspas";
        //    ServiceBusHelper serviceBusHelper = new ServiceBusHelper(connectionString, queueName);
          //  serviceBusHelper.EnsureSubscriptionExists("Notifier"); //Maybe extend this function with a filter?

            JobHostConfiguration config = new JobHostConfiguration();
            config.UseServiceBus();
            JobHost host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}
