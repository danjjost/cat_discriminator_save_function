using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace CatDiscriminator.Function
{
    public class BlobStorageUploader
    {
        BlobServiceClient _blobServiceClient;

        public BlobStorageUploader()
        {
            var connectionString = GetConnectionString();
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        string GetConnectionString()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config["AzureWebJobsStorage"];

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Azure Storage connection string is missing.");

            return connectionString;
        }

        public async Task UploadAsync(string category, string base64ImageString)
        {
            var bytesFromBase64 = Convert.FromBase64String(base64ImageString);
            using var memoryStream = new MemoryStream(bytesFromBase64);

            var blobClient = await GetBlobClient(category);
            await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = "image/jpg" });
        }

        async Task<BlobClient> GetBlobClient(string category)
        {
            var blobName = $"{Guid.NewGuid():N}.jpg";

            var containerClient = _blobServiceClient.GetBlobContainerClient(category);
            await containerClient.CreateIfNotExistsAsync();

            return containerClient.GetBlobClient(blobName);
        }
    }
}
