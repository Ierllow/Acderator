using System;

namespace Intense.Asset
{
    static class FileSizeExtensions
    {
        private const double One_KB = 1024d;
        private const long One_MB = 1024L * 1024;
        private const long One_GB = 1024L * 1024 * 1024;

        public static (double Value, string Unit) ToDisplayFileSize(this long fileSize) => fileSize switch
        {
            < One_MB => (Math.Round(fileSize / One_KB, 2), "KB"),
            < One_GB => (Math.Round((double)fileSize / One_MB, 2), "MB"),
            _ => (Math.Round((double)fileSize / One_GB, 2), "GB")
        };
    }
}