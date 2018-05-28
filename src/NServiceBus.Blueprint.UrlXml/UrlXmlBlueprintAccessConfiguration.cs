using System;

namespace NServiceBus
{
    using Blueprint;
    /// <summary>
    /// Configures file-based managed routing.
    /// </summary>
    public class UrlXmlBlueprintAccessConfiguration : BlueprintAccessConfiguration
    {
        TimeSpan interval = TimeSpan.FromSeconds(30);
        Uri fileUri = UriHelper.FilePathToUri("endpoints.xml");

        /// <summary>
        /// Specifies the interval in which the file is checked.
        /// </summary>
        public void UpdateInterval(TimeSpan updateInterval)
        {
            interval = updateInterval;
        }

        /// <summary>
        /// Specifies the path for the routing file.
        /// </summary>
        public void FilePath(string routingFilePath)
        {
            fileUri = UriHelper.FilePathToUri(routingFilePath);
        }

        /// <summary>
        /// Specifies the URI for the routing file.
        /// </summary>
        /// <param name="routingFileUri"></param>
        public void FileUri(Uri routingFileUri)
        {
            fileUri = routingFileUri;
        }

        public override IBlueprintAccess Create()
        {
            return new UrlXmlBlueprintAccess(fileUri, interval);
        }
    }
}