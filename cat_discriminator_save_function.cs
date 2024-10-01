using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace CatDiscriminator.Function
{
    public class CatDiscriminatorSaveFunction
    {
        static string[] VALID_CATEGORIES = ["captain", "bathroom-cat", "control"];

        readonly ILogger<CatDiscriminatorSaveFunction> _logger;
        readonly BlobStorageUploader _blobStorageUploader;

        public CatDiscriminatorSaveFunction(ILogger<CatDiscriminatorSaveFunction> logger, BlobStorageUploader blobStorageUploader)
        {
            _logger = logger;
            _blobStorageUploader = blobStorageUploader;
        }

        [Function("cat_discriminator_save_function")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,
            "post",
            Route = "cats/{category}")]
            HttpRequest req,
            string category
        )
        {
            _logger.LogInformation($"Processing request for category: {category}");

            try
            {
                ValidateCategory(category);

                var bodyAsString = await GetBodyAsString(req);
                await _blobStorageUploader.UploadAsync(category, bodyAsString);

                return new OkObjectResult($"Image saved with category '{category}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the image.");
                return new ObjectResult(GenerateProblemDetails(ex)) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }


        void ValidateCategory(string category)
        {
            if (!VALID_CATEGORIES.Contains(category))
                throw new ArgumentException($"Invalid image category. Valid categories are: {string.Join(", ", VALID_CATEGORIES)}");
        }

        ProblemDetails GenerateProblemDetails(Exception ex) =>
           new ProblemDetails
           {
               Status = StatusCodes.Status500InternalServerError,
               Title = "An error occurred while processing the request",
               Detail = ex.Message
           };

        async Task<string> GetBodyAsString(HttpRequest req)
        {
            using var memoryStream = new MemoryStream();
            return await new StreamReader(req.Body).ReadToEndAsync();
        }
    }
}
