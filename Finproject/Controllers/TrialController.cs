using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagePredictionController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ImagePredictionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { Error = "Invalid image file." });
            }

            // FastAPI endpoint for prediction
            string apiUrl = "https://chestmodelpublishapi.onrender.com/predict";

            try
            {
                // Create an HttpClient instance
                var httpClient = _httpClientFactory.CreateClient();

                // Prepare the multipart/form-data content
                using var content = new MultipartFormDataContent();

                // Convert IFormFile to byte array
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                byte[] imageData = ms.ToArray();

                // Create ByteArrayContent with the image data
                var byteArrayContent = new ByteArrayContent(imageData);
                byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(imageFile.ContentType ?? "application/octet-stream");

                // Add the file to the multipart content
                content.Add(byteArrayContent, "file", imageFile.FileName);

                // Send the POST request to FastAPI
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                // Handle response
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return Ok(new { Predictions = result });
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { Error = errorContent });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
