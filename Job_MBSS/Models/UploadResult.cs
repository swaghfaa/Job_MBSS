using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_MBSS.Models
{
    public class UploadResult
    {
        public bool Success;
        public string Status;   // Success | Exists | Versioned | SkipNotModified | Failed
        public string Message;  // raw JSON

        // parsed essentials for DB upsert
        public string BoxFileId;
        public string ETag;
        public string Sha1;
        public int? VersionNumber;
        public System.DateTime? LocalModifiedAt; // we send back the local file mtime we used
    }
}
