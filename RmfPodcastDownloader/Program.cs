using MediaDevices;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static RmfPodcastDownloader.Program;


namespace RmfPodcastDownloader {
    class Program {
        static async Task Main(string[] args) {
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf.fm/rss/podcast/historia-dla-doroslych.xml", true, true));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf.fm/rss/podcast/dorwac-bestie.xml", true, false));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf.fm/rss/podcast/sceny-zbrodni.xml", true, true));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf.fm/rss/podcast/zakazana-historia-radia.xml", true, false));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf.fm/rss/podcast/misja-specjalna.xml", true, true));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf.fm/rss/podcast/bajki-dla-doroslych.xml", true, true));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf.fm/rss/podcast/kryminatorium.xml", false, false));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf.fm/rss/podcast/ostatnie-dni-legendy.xml", true, false));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf.fm/rss/podcast/rockandrollowa-historia-swiata.xml", true, false));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Rmf, "https://www.rmf24.pl/podcast/naukowo-rzecz-ujmujac/feed", true, false));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Niebezpiecznik, "https://www.spreaker.com/show/2621972/episodes/feed", true, false));
            _podcasts.Add(new Podcast(Podcast.PodcastType.KCW, "https://podkasty.info/RSS/klcw.rss", true, false));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Panoptykon, "https://panoptykon.org/podcasty/rss.xml", true, false));
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://www.spreaker.com/show/3318547/episodes/feed", true, false)); //Piąte nie zabijaj
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://anchor.fm/s/6e6e1568/podcast/rss", true, false)); //Kryminalne historie
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://anchor.fm/s/606807e4/podcast/rss", true, false)); //Olga Herring
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://feeds.castos.com/d4vo5", true, false)); //Nauka To Lubię
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://anchor.fm/s/2ec7f2f8/podcast/rss", true, false)); //Zagadki Kryminalne - Karolina Anna
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://www.podcastone.com/podcast?categoryID2=2243", true, false)); //The Prosecutors
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://feeds.megaphone.fm/QCD6696793622", true, false)); //Big Mad True Crime
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://www.spreaker.com/show/4215515/episodes/feed", true, false)); //PatoArchitekci
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://rss.art19.com/the-vanished-podcast-wondery", true, false)); //The Vanished
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://hanselminutes.com/subscribe", true, false)); //The Hanselminutes podcast
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://feeds.simplecast.com/XA_851k3", true, false)); //The Stack Overflow Podcast
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "http://devtalk.pl/feed", true, false)); //DevTalk
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://www.podcastone.com/podcast?categoryID2=1983", true, false)); //Court Junkie
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://www.podcastone.com/podcast?categoryID2=2292", true, false)); //Court Junkie CIVIL
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://rss.art19.com/mrballens-medical-mysteries", true, false)); //MrBallen’s Medical Mysteries
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://pod.link/1482669176.rss", true, false)); //DUST
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://feeds.acast.com/public/shows/d1b39068-c10f-5817-8324-88ba173183cd", true, false)); //We're alive
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://feeds.megaphone.fm/SBP4341397353", true, false)); //DERELICT
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://feeds.megaphone.fm/wolf359radio", true, false)); //Wolf 359
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://feeds.megaphone.fm/SBP5252339511", true, false)); //The Leviathan Chronicles
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://feeds.megaphone.fm/thestrata", true, false)); //The Strata
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://podcasts.files.bbci.co.uk/p090t9cl.rss", true, false)); //BBC The Cipher
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://podcasts.files.bbci.co.uk/p0d34733.rss", true, false)); //BBC Restart
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://podcasts.files.bbci.co.uk/p06tqsg3.rss", true, false)); //BBC Forest 404
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://podcasts.files.bbci.co.uk/p09hbsqb.rss", true, false)); //BBC Limelight
            _podcasts.Add(new Podcast(Podcast.PodcastType.Unknown, "https://feed.passengerlist.org/", true, false)); //Passenger List

        





            Task[] tasks = new Task[_podcasts.Count];
            for (int i = 0; i < _podcasts.Count; i++) {
                int index = i;
                tasks[index] = Task.Run(() => DownloadPodcast(_podcasts[index]));
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

                    //var dirs = Directory.GetDirectories(_baseDir);
                    var dirs = _podcasts.Where(x => x.Sync).Select(x => x.Path);
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

        private static async Task DownloadPodcast(Podcast podcast) {
            rss podcasts = null;
            rssChannelItem podcastItem = null;
            try {
                if (!podcast.Download)
                    return;

                string responseBody = await _client.GetStringAsync(podcast.Url);
                XmlSerializer serializer = new XmlSerializer(typeof(rss));

                using (StringReader stringReader = new StringReader(responseBody)) {
                    // Call the Deserialize method to restore the object's state.
                    podcasts = (rss)serializer.Deserialize(stringReader);
                }

                string path = Path.Combine(_baseDir, CleanFileName(podcasts.channel.title));
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                podcast.Name = podcasts.channel.title;
                podcast.Path = path;

                string coverUrl = podcasts.channel.image1?.href;
                if (string.IsNullOrWhiteSpace(coverUrl))
                    coverUrl = podcasts.channel.image?.url;

                string coverFile = SaveCover(path, coverUrl).Result;

                int count = 0;
                int errorCount = 0;
                for (int i = 0; i < podcasts.channel.item.Length; i++) {
                    podcastItem = podcasts.channel.item[i];
                    count++;
                    DateTime podcastDate = DateTime.Parse(podcastItem.pubDate.Replace("PDT", "").Replace("PST", "")); // I don't care about TimeZone. I just need Date
                    string fileName = CleanFileName($"{podcastDate:yyyy-MM-dd} - {podcastItem.title?.Trim()}.mp3");
                    string filePath = Path.Combine(path, fileName);
                    if (File.Exists(filePath))
                        continue;

                    try {
                        _logger.Debug("[{0}] Downloading #{1} - {2}{3}", podcasts.channel.title, count, fileName, errorCount > 0 ? errorCount.ToString("' [error='##']'") : "");
                        using (HttpResponseMessage response = await _client.GetAsync(podcastItem.enclosure.url, HttpCompletionOption.ResponseHeadersRead))
                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
                            using (Stream streamToWriteTo = File.Open(filePath, FileMode.Create)) {
                                await streamToReadFrom.CopyToAsync(streamToWriteTo);
                            }
                        }
                    } catch (Exception ex) {
                        _logger.Error(ex, "[{0}] Exception downloading podcast Url={1}, FilePath={2}", podcasts.channel.title, podcastItem.enclosure.url, filePath);
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

                    try {
                        SetTags(filePath, fileName, podcasts.channel.title, podcasts.channel.author, (uint)podcastDate.Year, coverFile);
                    } catch (Exception ex) {
                        _logger.Error(ex, "[{0}] Exception setting mp3 tags Url={1}, FilePath={2}", podcasts.channel.title, podcastItem.enclosure.url, filePath);
                        continue;
                    }

                    errorCount = 0;
                }
                _logger.Info("[{0}] Download finished. Episodes downloaded: {1}", podcasts.channel.title, count);
            } catch (HttpRequestException e1) {
                _logger.Error(e1, "[{0}] Http Request Exception downloading Url={1}", podcasts?.channel?.title, podcastItem?.enclosure?.url);
            } catch (Exception e2) {
                _logger.Error(e2, "[{0}] General Exception downloading Url={1}", podcasts?.channel?.title, podcastItem?.enclosure?.url);
            }
        }

        /// <summary>
        /// Saves and resizes podcast cover
        /// </summary>
        /// <param name="path">File where to save cover</param>
        /// <param name="imageUrl">Url where covers is located</param>
        /// <returns></returns>
        private static async Task<string> SaveCover(string path, string imageUrl) {
            string filePath = string.Empty;
            try {
                string fileName = CleanFileName(Path.GetFileNameWithoutExtension(imageUrl.Split(new[] { '?' })[0])) + ".jpg";
                filePath = Path.Combine(path, fileName);
                using (HttpResponseMessage response = await _client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
                    using (Image image = Image.Load(streamToReadFrom)) {
                        if (image.Width > 1000) {
                            image.Mutate(x => x
                                 .Resize(1000, 0)
                                 );
                            image.Save(filePath);
                        }
                    }
                }
            } catch (Exception ex) {
                _logger.Error(ex, "Error dwonloading cover filePath={0}, imageUrl={1}", filePath, imageUrl);
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

        static readonly HttpClient _client = new HttpClient();
        static string _baseDir = @"D:\Temp\Rmf";
        static string _deviceName = "DarkGalaxy";
        static string _deviceBaseDir = @"\Phone\Audiobooks";
        static List<Podcast> _podcasts = new List<Podcast>();
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public class Podcast {
            public Podcast(PodcastType type, string url, bool download, bool sync) {
                Type = type;
                Url = url;
                Download = download;
                Sync = sync;

            }
            public enum PodcastType {
                Unknown = 0,
                Rmf = 1,
                Niebezpiecznik = 2,
                KCW = 3,
                Panoptykon = 4
            }
            public PodcastType Type { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string Path { get; set; }
            public bool Download { get; set; }
            public bool Sync { get; set; }

        }
    }
}
