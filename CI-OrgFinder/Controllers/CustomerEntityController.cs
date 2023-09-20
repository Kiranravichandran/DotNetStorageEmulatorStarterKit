using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CI_OrgFinder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerEntityController : ControllerBase
    {
     
        private readonly ILogger<CustomerEntityController> _logger;

        public CustomerEntityController(ILogger<CustomerEntityController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetCustomerEntity")]
        public async Task<IActionResult> GetAsync()
        {
            string tableName = "demo" + Guid.NewGuid().ToString().Substring(0, 5);
            CloudTable table = await Common.CreateTableAsync(tableName);
            CloudTableClient tableClient = table.ServiceClient;
            await BatchInsertOfCustomerEntitiesAsync(table);
            List<CustomerEntity> customers = await ExecuteSimpleQuery(table, "Smith", "0001", "0075");
            Console.WriteLine();

            // Query the same range of data within a partition and return result segments of 50 entities at a time
            Console.WriteLine("Retrieving entities with surname of Smith and first names >= 1 and <= 75");
            await PartitionRangeQueryAsync(table, "Smith", "0001", "0075");
            Console.WriteLine();
            return Ok(customers);
        }

        private static async Task BatchInsertOfCustomerEntitiesAsync(CloudTable table)
        {
            try
            {
                // Create the batch operation. 
                TableBatchOperation batchOperation = new TableBatchOperation();

                // The following code  generates test data for use during the query samples.  
                for (int i = 0; i < 100; i++)
                {
                    batchOperation.InsertOrMerge(new CustomerEntity("Smith", string.Format("{0}", i.ToString("D4")))
                    {
                        Email = string.Format("{0}@esdc.com", i.ToString("D4")),
                        PhoneNumber = string.Format("425-555-{0}", i.ToString("D4"))
                    });
                }

                // Execute the batch operation.
                IList<TableResult> results = await table.ExecuteBatchAsync(batchOperation);
                foreach (var res in results)
                {
                    var customerInserted = res.Result as CustomerEntity;
                    Console.WriteLine("Inserted entity with\t Etag = {0} and PartitionKey = {1}, RowKey = {2}", customerInserted.ETag, customerInserted.PartitionKey, customerInserted.RowKey);
                }
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        private static async Task<List<CustomerEntity>> ExecuteSimpleQuery(CloudTable table, string partitionKey, string startRowKey, string endRowKey)
        {
            List<CustomerEntity> customers = new List<CustomerEntity>();
            try
            {
                // Create the range query using the fluid API 
                TableQuery<CustomerEntity> rangeQuery = new TableQuery<CustomerEntity>().Where(
                    TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                            TableOperators.And,
                            TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startRowKey),
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, endRowKey))));

                // Handle pagination using ExecuteQuerySegmentedAsync
                TableContinuationToken token = null;
                do
                {
                    var queryResult = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);
                    customers.AddRange(queryResult.Results);
                    foreach (CustomerEntity entity in queryResult.Results)
                    {
                        Console.WriteLine("Customer: {0},{1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey, entity.Email, entity.PhoneNumber);
                    }
                    token = queryResult.ContinuationToken;
                } while (token != null);
                return customers;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        private static async Task PartitionRangeQueryAsync(CloudTable table, string partitionKey, string startRowKey, string endRowKey)
        {
            try
            {
                // Create the range query using the fluid API 
                TableQuery<CustomerEntity> rangeQuery = new TableQuery<CustomerEntity>().Where(
                    TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                            TableOperators.And,
                            TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startRowKey),
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, endRowKey))));

                // Request 50 results at a time from the server. 
                TableContinuationToken token = null;
                rangeQuery.TakeCount = 50;
                int segmentNumber = 0;
                do
                {
                    // Execute the query, passing in the continuation token.
                    // The first time this method is called, the continuation token is null. If there are more results, the call
                    // populates the continuation token for use in the next call.
                    TableQuerySegment<CustomerEntity> segment = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);

                    // Indicate which segment is being displayed
                    if (segment.Results.Count > 0)
                    {
                        segmentNumber++;
                        Console.WriteLine();
                        Console.WriteLine("Segment {0}", segmentNumber);
                    }

                    // Save the continuation token for the next call to ExecuteQuerySegmentedAsync
                    token = segment.ContinuationToken;

                    // Write out the properties for each entity returned.
                    foreach (CustomerEntity entity in segment)
                    {
                        Console.WriteLine("\t Customer: {0},{1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey, entity.Email, entity.PhoneNumber);
                    }

                    Console.WriteLine();
                }
                while (token != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
}