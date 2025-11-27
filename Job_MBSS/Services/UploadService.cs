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

        public async Task<UploadResult> UploadNew(string accessToken, string filePath, string folderId)
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

            using (var fs = File.OpenRead(filePath))
            {
                var fileContent = new StreamContent(fs);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                multi.Add(fileContent, "file", fileName);
                req.Content = multi;

                var res = await _http.SendAsync(req);
                var json = await res.Content.ReadAsStringAsync();

                var r = ParseUploadResponse(json, (int)res.StatusCode);
                r.LocalModifiedAt = File.GetLastWriteTimeUtc(filePath);
                return r;
            }
        }

        public async Task<UploadResult> UploadNewVersion(string accessToken, string filePath, string boxFileId, string ifMatchETag = null)
        {
            var url = $"https://upload.box.com/api/2.0/files/{boxFileId}/content";
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (!string.IsNullOrEmpty(ifMatchETag))
                req.Headers.TryAddWithoutValidation("If-Match", ifMatchETag); // optional

            var multi = new MultipartFormDataContent();
            using (var fs = File.OpenRead(filePath))
            {
                var fileContent = new StreamContent(fs);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                multi.Add(fileContent, "file", Path.GetFileName(filePath));
                req.Content = multi;

                var res = await _http.SendAsync(req);
                var json = await res.Content.ReadAsStringAsync();

                var r = ParseUploadResponse(json, (int)res.StatusCode);
                if (r.Success && string.IsNullOrEmpty(r.BoxFileId))
                    r.BoxFileId = boxFileId; // fallback
                r.Status = r.Success ? "Versioned" : r.Status;
                r.LocalModifiedAt = File.GetLastWriteTimeUtc(filePath);
                return r;
            }
        }

        private UploadResult ParseUploadResponse(string json, int statusCode)
        {
            var result = new UploadResult { Message = json };

            try
            {
                if (statusCode == 201) // Created (new or version)
                {
                    var root = JObject.Parse(json);
                    var entry = root["entries"]?.First;
                    result.Success = true;
                    result.Status = "Success";
                    result.BoxFileId = entry?["id"]?.ToString();
                    result.ETag = entry?["etag"]?.ToString();
                    result.Sha1 = entry?["sha1"]?.ToString();
                    // sometimes version number may be string or missing
                    int vn;
                    var vnToken = entry?["version_number"];
                    if (vnToken != null && int.TryParse(vnToken.ToString(), out vn))
                        result.VersionNumber = vn;
                    return result;
                }
                else if (statusCode == 409) // Conflict (already exists)
                {
                    // read conflicts -> id
                    var root = JObject.Parse(json);
                    var conflicts = root["context_info"]?["conflicts"];
                    result.Success = true;
                    result.Status = "Exists";
                    result.BoxFileId = conflicts?["id"]?.ToString();
                    result.ETag = conflicts?["etag"]?.ToString();
                    result.Sha1 = conflicts?["sha1"]?.ToString();
                    int vn;
                    var vnToken = conflicts?["version_number"];
                    if (vnToken != null && int.TryParse(vnToken.ToString(), out vn))
                        result.VersionNumber = vn;
                    return result;
                }
                else
                {
                    result.Success = false;
                    result.Status = "Failed";
                    return result;
                }
            }
            catch
            {
                result.Success = statusCode == 201; // best effort
                result.Status = result.Success ? "Success" : "Failed";
                return result;
            }
        }
    }
}
