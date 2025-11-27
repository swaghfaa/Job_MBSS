using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_MBSS.Models
{
    public class BoxFileInfo
    {
        public int Id;
        public string BoxFileId;
        public string BoxFolderId;
        public string FileName;
        public string FullPath;
        public System.DateTime LocalModifiedAt;
        public string ETag;
        public string Sha1;
        public int? VersionNumber;
    }
}
