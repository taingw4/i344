using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class sum : TableEntity
    {
        public sum(int a, int b, int c)
        {
            this.PartitionKey = "result";
            this.RowKey = "sum";
            this.result = a + b + c;
        }

        public sum() { }

        public int result { get; set; }

    }
}
