using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RmfPodcastDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {

            //_rssUrl = "https://www.rmf.fm/rss/podcast/historia-dla-doroslych.xml";
            //_rssUrl = "https://www.rmf.fm/rss/podcast/dorwac-bestie.xml";
            _rssUrl = "https://www.rmf.fm/rss/podcast/sceny-zbrodni.xml";

            try
            {
                string responseBody = await _client.GetStringAsync(_rssUrl);
                XmlSerializer serializer = new XmlSerializer(typeof(rss));
                rss podcasts;


                using (StringReader stringReader = new StringReader(responseBody))
                {
                    // Call the Deserialize method to restore the object's state.
                    podcasts = (rss)serializer.Deserialize(stringReader);
                }

                string path = Path.Combine(_baseDir, podcasts.channel.title);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                foreach (var p in podcasts.channel.item)
                {
                    DateTime podcastDate = DateTime.Parse(p.pubDate);
                    string fileName = CleanFilePath($"{podcastDate:yyyy-MM-dd} - {p.title}.mp3");
                    string filePath = Path.Combine(path, fileName);
                    if (File.Exists(filePath))
                        continue;

                    Console.WriteLine("Pobieram {0}", fileName);
                    using (HttpResponseMessage response = await _client.GetAsync(p.enclosure.url, HttpCompletionOption.ResponseHeadersRead))
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        using (Stream streamToWriteTo = File.Open(filePath, FileMode.Create))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                    }
                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        private static string CleanFilePath(string input)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(input, "");
        }

        static readonly HttpClient _client = new HttpClient();
        static string _rssUrl = "";
        static string _baseDir = @"D:\Rmf";
    }
}
