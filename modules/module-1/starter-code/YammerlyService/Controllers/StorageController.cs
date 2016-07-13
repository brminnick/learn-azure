using Microsoft.Azure.Mobile.Server.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Azure;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text;
using System.Diagnostics;
using Yammerly.Service.DataObjects;

namespace Yammerly.Service.Controllers
{
    [Authorize]
    [MobileAppController]
    public class StorageController : ApiController
    {
        public StorageController()
        {
            ConnectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_MS_AzureStorageAccountConnectionString", EnvironmentVariableTarget.Process);
            StorageAccount = CloudStorageAccount.Parse(ConnectionString);
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }

        public string ConnectionString { get; set; }

        public CloudStorageAccount StorageAccount { get; set; }

        public CloudBlobClient BlobClient { get; set; }

        public async Task<StorageToken> Get()
        {
            var container = BlobClient.GetContainerReference("images");

            try
            {
                await container.CreateIfNotExistsAsync();
            }
            catch (StorageException ex)
            {
                Debug.WriteLine($"[GetStorageTokenController] Cannot create container: {ex.Message}");
            }

            // Create a blob URI - based on a GUID
            var blobName = $"{Guid.NewGuid().ToString("N")}.jpg";
            var blob = container.GetBlockBlobReference(blobName);

            // Create a policy for the blob access
            var blobPolicy = new SharedAccessBlobPolicy
            {
                // Set start time to five minutes before now to avoid clock skew.
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                // Allow Access for the next 60 minutes (according to Azure)
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(60),
                // Allow read, write and create permissions
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
            };

            return new StorageToken
            {
                Name = blobName,
                Uri = blob.Uri,
                SasToken = blob.GetSharedAccessSignature(blobPolicy)
            };

        }
    }
}