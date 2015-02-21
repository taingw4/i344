using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;


namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {

            //read message from Queue
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
               ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("integers");
            queue.CreateIfNotExists();

            queue.FetchAttributes();
            int? cachedMessageCount = queue.ApproximateMessageCount;

            while (cachedMessageCount >= 1)
            {
                CloudQueueMessage message = queue.GetMessage();
                queue.DeleteMessage(message);
                string numbers = message.AsString;
                string[] split = numbers.Split(',');
                int a = Convert.ToInt32(split[0]);
                int b = Convert.ToInt32(split[1]);
                int c = Convert.ToInt32(split[2]);
                

                //sleep 10 seconds
                Thread.Sleep(10);

                //calculate sum
                sum result = new sum(a, b, c);

                //put result in table storage
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("sum");
                table.CreateIfNotExists();

                TableOperation retrieveOperation = TableOperation.Retrieve<sum>("result", "sum");
                TableResult retrievedResult = table.Execute(retrieveOperation);
                sum deleteEntity = (sum)retrievedResult.Result;
                if (deleteEntity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                    table.Execute(deleteOperation);
                }

                TableOperation insertOperation = TableOperation.Insert(result);
                table.Execute(insertOperation);
            }


            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
