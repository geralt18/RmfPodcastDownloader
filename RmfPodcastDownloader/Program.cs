using MediaDevices;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
         _urls.Add("https://www.rmf.fm/rss/podcast/zakazana-historia-radia.xml");
         _urls.Add("https://www.rmf.fm/rss/podcast/misja-specjalna.xml");
         _urls.Add("https://www.rmf.fm/rss/podcast/bajki-dla-doroslych.xml");
         //_urls.Add("https://www.rmf.fm/rss/podcast/kryminatorium.xml");
         _urls.Add("https://www.rmf.fm/rss/podcast/ostatnie-dni-legendy.xml");
         _urls.Add("https://www.rmf.fm/rss/podcast/rockandrollowa-historia-swiata.xml");
         _urls.Add("https://www.rmf24.pl/podcast/naukowo-rzecz-ujmujac/feed");

         Task[] tasks = new Task[_urls.Count];
         for (int i = 0; i < _urls.Count; i++) {
            int index = i;
            tasks[index] = Task.Run(() => DownloadPodcast(_urls[index]));
         }
         Task.WaitAll(tasks);

         SyncFiles();
      }

      private static void SyncFiles() {
         var devices = MediaDevice.GetDevices();
         var device = devices.FirstOrDefault(d => d.FriendlyName.Contains(_deviceName));
         if (device == null)
            return;

         using (device) {
            try {
               device.Connect();

               var dirs = Directory.GetDirectories(_baseDir);
               foreach (var dir in dirs) {
                  _logger.Debug("Syncing directory {0}", dir);
                  var files = Directory.GetFiles(dir, "*.mp3");
                  foreach (var file in files) {
                     var fi = new FileInfo(file);
                     if (fi.Length == 0)
                        return;

                     var deviceDirPath = dir.Replace(_baseDir, _deviceBaseDir);
                     var deviceFilePath = file.Replace(_baseDir, _deviceBaseDir);
                     
                     if (!device.DirectoryExists(deviceDirPath))
                        device.CreateDirectory(deviceDirPath);

                     if (!device.FileExists(deviceFilePath)) {
                        _logger.Info("Copying file {0}", file);
                        device.UploadFile(file, deviceFilePath);
                     }
                  }
               }
            } catch (Exception ex) {
               _logger.Error(ex, "Exepcion during syncing files with device");
            } finally {
               device.Disconnect();
            }
         }
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

            int count = 0;
            int errorCount = 0;
            for (int i = 0; i < podcasts.channel.item.Length; i++) {
               rssChannelItem p = podcasts.channel.item[i];
               count++;
               DateTime podcastDate = DateTime.Parse(p.pubDate);
               string fileName = CleanFilePath($"{podcastDate:yyyy-MM-dd} - {p.title?.Trim()}.mp3");
               string filePath = Path.Combine(path, fileName);
               if (File.Exists(filePath))
                  continue;

               try {
                  _logger.Debug("[{0}] Downloading #{1} - {2}{3}", podcasts.channel.title, count, fileName, errorCount > 0 ? errorCount.ToString("' [error='##']'") : "" );
                  using (HttpResponseMessage response = await _client.GetAsync(p.enclosure.url, HttpCompletionOption.ResponseHeadersRead))
                  using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
                     using (Stream streamToWriteTo = File.Open(filePath, FileMode.Create)) {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                     }
                  }
               }
               catch (Exception ex) {
                  _logger.Error(ex, "Exception downloading podcast");
                  if (File.Exists(filePath))
                     File.Delete(filePath);
                  continue;
               }

               var fi = new FileInfo(filePath);
               if (fi.Length == 0) {
                  File.Delete(filePath);
                  errorCount++;
                  i--;
                  count--;
                  Task.Delay(errorCount * 1000).Wait();
                  continue;
               }
               
               SetTags(filePath, fileName, podcasts.channel.title, "RMF FM", (uint)podcastDate.Year, coverFile);
               errorCount = 0;
            }
            _logger.Info("[{0}] Download finished. Podcasts downloaded: {1}", podcasts.channel.title, count);
         } catch (HttpRequestException e1) {
            _logger.Error(e1, "Http Request Exception");
         } catch (Exception e2) {
            _logger.Error(e2, "General Exception");
         }
      }

      /// <summary>
      /// Saves and resizes podcast cover
      /// </summary>
      /// <param name="path">File where to save cover</param>
      /// <param name="imageUrl">Url where covers is located</param>
      /// <returns></returns>
      private static async Task<string> SaveCover(string path, string imageUrl) {
         string fileName = CleanFileName(Path.GetFileNameWithoutExtension(imageUrl.Split(new[] { '?' })[0]))+ ".jpg";
         string filePath = Path.Combine(path, fileName);
         using (HttpResponseMessage response = await _client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead))
            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
                using (Image image = Image.Load(streamToReadFrom)) {
                    if (image.Width > 1000)
                    {
                        image.Mutate(x => x
                             .Resize(1000, 0)
                             );
                        image.Save(filePath);
                    }
                }
            }
         return filePath;
      }

      static string CleanFileName(string name) {
         foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "");
         return name;
      }

      private static void SetTags(string filePath, string title, string album, string artist, uint year, string coverPath) {
         var fi = new FileInfo(filePath);
         if (fi.Length == 0)
            return;

         using (var tag = TagLib.File.Create(filePath)) {
            tag.Tag.Title = title;
            tag.Tag.Album = album;
            tag.Tag.Artists = new string[] { artist };
            tag.Tag.Year = year;
            tag.Tag.Track = 1;
            //tag.Tag.TrackCount = 12;

            if (File.Exists(coverPath)) {
               var pictures = new TagLib.Picture[1];
               pictures[0] = new TagLib.Picture(coverPath);
               tag.Tag.Pictures = pictures;
            }

            tag.Save();
         }
      }

      private static string CleanFilePath(string input) {
         string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
         Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
         return r.Replace(input, "");
      }

      static readonly HttpClient _client = new HttpClient();
      static string _baseDir = @"D:\Temp\Rmf";
      static string _deviceName = "DarkGalaxy";
      static string _deviceBaseDir = @"\Phone\Audiobooks";
      static List<string> _urls = new List<string>();
      private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
   }
}
