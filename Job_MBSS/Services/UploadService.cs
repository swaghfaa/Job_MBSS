using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Job_MBSS.Models;

namespace Job_MBSS.Services
{
    public class UploadService
    {
        private static readonly HttpClient _http = new HttpClient();

        public async Task<UploadResult> UploadFile(string accessToken, string filePath, string folderId)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);

                var req = new HttpRequestMessage(HttpMethod.Post, "https://upload.box.com/api/2.0/files/content");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var multi = new MultipartFormDataContent();

                var attrs = new JObject
                {
                    ["name"] = fileName,
                    ["parent"] = new JObject { ["id"] = folderId }
                };
                multi.Add(new StringContent(attrs.ToString(), Encoding.UTF8, "application/json"), "attributes");

                var fs = File.OpenRead(filePath);
                var fileContent = new StreamContent(fs);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                multi.Add(fileContent, "file", fileName);
                req.Content = multi;

                var res = await _http.SendAsync(req);
                var json = await res.Content.ReadAsStringAsync();

                fs.Dispose();

                // 201 Created  → success
                // 409 Conflict → file already exists
                var result = new UploadResult();
                if ((int)res.StatusCode == 201)
                {
                    result.Success = true;
                    result.Status = "Success";
                }
                else if ((int)res.StatusCode == 409)
                {
                    result.Success = true;     // anggap OK agar tidak ngulang
                    result.Status = "Exists"; // sudah ada
                }
                else
                {
                    result.Success = false;
                    result.Status = "Failed";
                }

                result.Message = json;
                return result;
            }
            catch (Exception ex)
            {
                return new UploadResult { Success = false, Status = "Failed", Message = ex.ToString() };
            }
        }
    }
}
