using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Constant
{
    public class MediaTypeNames
    {
        public class Application
        {
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Application data is in JSON
            //     format.
            public const string Json = "application/json";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Application data is not interpreted.
            public const string Octet = "application/octet-stream";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Application data is in Portable
            //     Document Format (PDF).
            public const string Pdf = "application/pdf";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Application data is in Rich
            //     Text Format (RTF).
            public const string Rtf = "application/rtf";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Application data is a SOAP
            //     document.
            public const string Soap = "application/soap+xml";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Application data is in XML
            //     format.
            public const string Xml = "application/xml";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Application data is compressed.
            public const string Zip = "application/zip";
        }
        //
        // Summary:
        //     Specifies the type of image data in an email message attachment.
        public class Image
        {
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Image data is in Graphics Interchange
            //     Format (GIF).
            public const string Gif = "image/gif";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Image data is in Joint Photographic
            //     Experts Group (JPEG) format.
            public const string Jpeg = "image/jpeg";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Image data is in Tagged Image
            //     File Format (TIFF).
            public const string Tiff = "image/tiff";
        }
        //
        // Summary:
        //     Specifies the type of text data in an email message attachment.
        public class Text
        {
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Text data is in HTML format.
            public const string Html = "text/html";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Text data is in plain text
            //     format.
            public const string Plain = "text/plain";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Text data is in Rich Text Format
            //     (RTF).
            public const string RichText = "text/richtext";
            //
            // Summary:
            //     Specifies that the System.Net.Mime.MediaTypeNames.Text data is in XML format.
            public const string Xml = "text/xml";
        }
    }
}
