using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public class Common
{
    public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
    {
        CloudStorageAccount storageAccount;
        try
        {
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
            throw;
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
            Console.ReadLine();
            throw;
        }

        return storageAccount;
    }

    public static async Task<CloudTable> CreateTableAsync(string tableName)
    {
        // Retrieve storage account information from connection string.
        CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString("UseDevelopmentStorage=true;");

        // Create a table client for interacting with the table service
        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        Console.WriteLine("Create a Table for the demo");

        // Create a table client for interacting with the table service 
        CloudTable table = tableClient.GetTableReference(tableName);
        try
        {
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }
        }
        catch (StorageException)
        {
            Console.WriteLine("If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
            Console.ReadLine();
            throw;
        }

        Console.WriteLine();
        return table;
    }
}
