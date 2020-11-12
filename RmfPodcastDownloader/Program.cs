using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace RmfPodcastDownloader
{
   class Program
   {
      static async Task Main(string[] args) {
         _urls.Add("https://www.rmf.fm/rss/podcast/historia-dla-doroslych.xml");
         _urls.Add("https://www.rmf.fm/rss/podcast/dorwac-bestie.xml");
         _urls.Add("https://www.rmf.fm/rss/podcast/sceny-zbrodni.xml");
         foreach (var url in _urls)
            await DownloadPodcast(url);
      }

      private static async Task DownloadPodcast(string rssUrl) {
         try {
            string responseBody = await _client.GetStringAsync(rssUrl);
            XmlSerializer serializer = new XmlSerializer(typeof(rss));
            rss podcasts;

            using (StringReader stringReader = new StringReader(responseBody)) {
               // Call the Deserialize method to restore the object's state.
               podcasts = (rss)serializer.Deserialize(stringReader);
            }

            string path = Path.Combine(_baseDir, podcasts.channel.title);
            if (!Directory.Exists(path))
               Directory.CreateDirectory(path);

            string coverFile = SaveCover(path, podcasts.channel.image1.href).Result;

            foreach (var p in podcasts.channel.item) {
               DateTime podcastDate = DateTime.Parse(p.pubDate);
               string fileName = CleanFilePath($"{podcastDate:yyyy-MM-dd} - {p.title}.mp3");
               string filePath = Path.Combine(path, fileName);
               if (File.Exists(filePath))
                  continue;

               Console.WriteLine("Pobieram {0}", fileName);
               using (HttpResponseMessage response = await _client.GetAsync(p.enclosure.url, HttpCompletionOption.ResponseHeadersRead))
               using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
                  using (Stream streamToWriteTo = File.Open(filePath, FileMode.Create)) {
                     await streamToReadFrom.CopyToAsync(streamToWriteTo);
                  }
               }
               SetTags(filePath, fileName, podcasts.channel.title, "RMF FM", (uint)podcastDate.Year, coverFile);
            }

         } catch (HttpRequestException e) {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
         }
      }

      private static async Task<string> SaveCover(string path, string imageUrl) {
         string filePath = Path.Combine(path, Path.GetFileName(imageUrl));
         using (HttpResponseMessage response = await _client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead))
         using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
            using (Stream streamToWriteTo = File.Open(filePath, FileMode.Create)) {
               await streamToReadFrom.CopyToAsync(streamToWriteTo);
            }
         }
         return filePath;
      }

      private static void SetTags(string filePath, string title, string album, string artist, uint year, string coverPath) {
         var tag = TagLib.File.Create(filePath);
         tag.Tag.Title = title;
         tag.Tag.Album = album;
         tag.Tag.Artists = new string[] { artist };
         tag.Tag.Year = year;
         tag.Tag.Track = 1;
         //tag.Tag.TrackCount = 12;
         var pictures = new TagLib.Picture[1];
         pictures[0] = new TagLib.Picture(coverPath);
         tag.Tag.Pictures = pictures;
         tag.Save();
      }

      private static string CleanFilePath(string input) {
         string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
         Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
         return r.Replace(input, "");
      }

      static readonly HttpClient _client = new HttpClient();
      static string _baseDir = @"D:\Rmf";
      static List<string> _urls = new List<string>();
   }
}
