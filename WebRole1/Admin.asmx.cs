using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Services;
using WorkerRole1;

namespace WebRole1
{
    /// <summary>
    /// Summary description for Admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Admin : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        // pass these integers to Queue storage for worker role to sum
        [WebMethod]
        public void WorkerRoleCalculateSum(int a, int b, int c) 
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("integers");
            queue.CreateIfNotExists();

            string x = a.ToString();
            string y = b.ToString();
            string z = c.ToString();

            CloudQueueMessage message = new CloudQueueMessage(x + "," + y + "," + z);
            queue.AddMessage(message);
        
        }

        //read sum from table storage
        [WebMethod]
        public int ReadSumFromTableStorage()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("sum");

            TableOperation retrieveOperation = TableOperation.Retrieve<sum>("result", "sum");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            return ((sum)retrievedResult.Result).result;
        }
    }
}
