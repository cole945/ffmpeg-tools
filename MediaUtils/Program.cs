using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaUtils
{
    internal class Program
    {
        static string rootdir = @"E:\Video\xxx\VR.J";
        static string loDir = rootdir + @"\@@lo-res";
        static string miDir = rootdir + @"\@@mi-res";
        static string hiDir = rootdir + @"\@@hi-res";

        static string ffmpeg_exe = string.Empty;
        static string ffprobe_exe = string.Empty;

        static Regex idRegex = new Regex("^[^ ]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static void moveTo(string dir, string id, string dest)
        {
            var files = Directory.GetFiles(dir, id + "*");
            string subdir = Path.GetFileName(dir);

            foreach (string f in files)
            {
                string destFile = Path.Combine(dest, subdir, Path.GetFileName(f));
                Directory.CreateDirectory(Path.Combine(dest, subdir));

                File.Move(f, destFile, false);
            }
            return;
        }
        static void processFileGroup(string dir, string id)
        {
            var mediaFiles = Directory.GetFiles(dir, id + " *").Where(file => file.ToLower().EndsWith("mp4") ||
                                                                   file.ToLower().EndsWith("mkv") ||
                                                                   file.ToLower().EndsWith("wmv"));

            float bitrate = 0.0f;
            float maxBitrate = 0.0f;
            foreach (var m in mediaFiles)
            {
                MediaObject mo = GetMediaObject(m);
                bitrate += mo.Bitrate;
                maxBitrate = Math.Max(maxBitrate, mo.Bitrate);
            }

            // bitrate /= mediaFiles.Count();
            bitrate = maxBitrate;

            if (bitrate < 10000)
            {
                moveTo(dir, id, loDir);
            }
            else if(bitrate < 20000)
            {
                moveTo(dir, id, miDir);
            }
            else
            {
                moveTo(dir, id, hiDir);
            }

            return;
        }

        static Regex resRegex = new Regex(@"\b[0-9]+p\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static void renameWithSuffix(string file, string suffix)
        {
            string dir = Path.GetDirectoryName(file);
            string filebase = Path.GetFileNameWithoutExtension(file);

            var files = Directory.GetFiles(dir, filebase + "*");

            foreach (string f in files)
            {
                string ff = Path.GetFileName(f);

                ff = filebase + "." + suffix + ff.Substring(filebase.Length);

                File.Move(f, Path.Combine(dir, ff), false);
            }
            return;
        }
        static void renameRes(string f)
        {
            MediaObject mo = GetMediaObject(f);

            string basename = Path.GetFileName(f);
            Match m = resRegex.Match(basename);

            if (m.Success)
                return;

            var res = mo.Resolution.Split("x");
            if (res.Length < 2)
                return;
            string h = res[1];

            renameWithSuffix(f, h + "p");

            return;
        }

        static bool skipPath(string path)
        {
            if (path.ToLower().StartsWith(loDir.ToLower()))
                return true;
            if (path.ToLower().StartsWith(miDir.ToLower()))
                return true;
            if (path.ToLower().StartsWith(hiDir.ToLower()))
                return true;
            return false;

        }
        static void processDir(string dir)
        {

#if false
            if (skipPath(dir))
                return;
            foreach (string d in Directory.GetDirectories(dir))
            {
                processDir(d);
            }

            foreach (string f in Directory.GetFiles(dir).Where(file => file.ToLower().EndsWith("mp4") ||
                                                                       file.ToLower().EndsWith("mkv") ||
                                                                       file.ToLower().EndsWith("wmv")
                                                                       )
                                                        .ToList())
            {
                if (!File.Exists(f))
                    continue;
                string filename = Path.GetFileName(f);
                Match m = idRegex.Match(filename);
                string videoId = m.Groups[0].Value;

                processFileGroup(dir, videoId);
            }
#endif


            foreach (string d in Directory.GetDirectories(dir))
            {
                processDir(d);
            }
            foreach (string f in Directory.GetFiles(dir).Where(file => file.ToLower().EndsWith("mp4") ||
                                                                       file.ToLower().EndsWith("mkv") ||
                                                                       file.ToLower().EndsWith("wmv")
                                                                       )
                                                        .ToList())
            {
                if (!File.Exists(f))
                    continue;

                renameRes(f);
            }

        }
        static void Main(string[] args)
        {
            string[] ffmpeg_dir = {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).TrimEnd('\\'),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).TrimEnd('\\'),
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                };


            foreach (string d in ffmpeg_dir)
            {
                if (File.Exists(Path.Combine(d, "ffmpeg", "ffmpeg.exe")) &&
                    File.Exists(Path.Combine(d, "ffmpeg", "ffprobe.exe")))
                {
                    ffmpeg_exe = Path.Combine(d, "ffmpeg", "ffmpeg.exe");
                    ffprobe_exe = Path.Combine(d, "ffmpeg", "ffprobe.exe");
                    break;
                }
                else
                if (File.Exists(Path.Combine(d, "ffmpeg", "bin", "ffmpeg.exe")) &&
                    File.Exists(Path.Combine(d, "ffmpeg", "bin", "ffprobe.exe")))
                {
                    ffmpeg_exe = Path.Combine(d, "ffmpeg", "bin", "ffmpeg.exe");
                    ffprobe_exe = Path.Combine(d, "ffmpeg", "bin", "ffprobe.exe");
                    break;
                }
            }

            if (ffmpeg_exe == string.Empty || ffprobe_exe == string.Empty)
            {
                Console.WriteLine("Cannot find ffmpeg");
            }


            rootdir = @"H:\Video\Movie.Erotic";
            processDir(rootdir);


        }

        static string[] StreamStringSplit(string s)
        {
            // Stream #0:0(eng): Audio: aac (LC) (mp4a / 0x6134706D), 48000 Hz, stereo, fltp, 152 kb/s (default)
            List<string> list = new List<string>();

            int bracket = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                sb.Append(s[i]);
                switch (s[i])
                {
                    case '(':
                        bracket++;
                        break;
                    case ')':
                        bracket--;
                        break;
                    case ',':
                        if (bracket == 0)
                        {
                            sb.Remove(sb.Length - 1, 1);
                            list.Add(sb.ToString().Trim());
                            sb.Clear();
                        }
                        break;
                }
            }
            if (sb.Length > 0)
                list.Add(sb.ToString().Trim());
            return list.ToArray();
        }


        static public MediaObject GetMediaObject(string file)
        {
            MediaObject mo = new MediaObject();
            mo.FileInfo = new FileInfo(file);

            Process process = new Process
            {
                StartInfo = {
                    FileName = ffprobe_exe,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = " \"" + file + "\""
                },
                EnableRaisingEvents = true
            };

            Regex durationRegex = new Regex(@"Duration: (?<DURATION>[^ ]+),.*bitrate: (?<BITRATE>[0-9]+)", RegexOptions.Compiled);
            Regex videoStream = new Regex(@"Stream .*Video: (?<INFO>.*)", RegexOptions.Compiled);
            Regex audioStream = new Regex(@"Stream .*Audio: (?<INFO>.*)", RegexOptions.Compiled);
            Regex resRegex = new Regex("[0-9]+x[0-9]+", RegexOptions.Compiled);
            process.Start();

            while (true)
            {
                var line = process.StandardError.ReadLine();
                if (line == null || line == string.Empty)
                    break;
                if (line.IndexOf("attached pic") >= 0)
                    continue;

                Match m;
                m = durationRegex.Match(line);
                if (m.Success)
                {
                    mo.Duration = TimeSpan.Parse(m.Groups["DURATION"].Value);
                    mo.Bitrate = int.Parse(m.Groups["BITRATE"].Value);
                }

                m = videoStream.Match(line);
                if (m.Success)
                {
                    string[] VideoStreamInfo = StreamStringSplit(m.Groups["INFO"].Value);
                    // mo.Video = m.Groups["INFO"].Value.Split(',')[0].Trim();
                    mo.Video = VideoStreamInfo[0];
                    // int bitrate;
                    // if (int.TryParse(VideoStreamInfo[3].Trim().Split(' ')[0], out bitrate) && bitrate < mo.Bitrate)
                    //     mo.Bitrate = bitrate;
                    // mo.Resolution = VideoStreamInfo[2].Split(' ')[0];
                    foreach (string vsi in VideoStreamInfo)
                    {
                        Match mm;
                        mm = resRegex.Match(vsi);
                        if (mm.Success)
                        {
                            mo.Resolution = mm.Groups[0].Value;
                        }
                        string[] ss = vsi.Trim().Split(' ');
                        if (ss.Length >= 2 && ss[1] == "kb/s")
                            int.TryParse(ss[0], out mo.Bitrate);
                        else if (ss.Length >= 2 && ss[1] == "fps")
                            mo.FPS = ss[0];

                    }
                }

                m = audioStream.Match(line);
                if (m.Success)
                    mo.Audio = m.Groups["INFO"].Value.Split(',')[0];
            }

            process.Dispose();
            if (mo.Duration == TimeSpan.Zero || mo.Bitrate == 0 || mo.Video == string.Empty)
                return null;
            return mo;
        }

    }

    public class MediaObject
    {
        public string Video = string.Empty;
        public string Audio = string.Empty;
        public string Resolution = string.Empty;
        public string FPS = string.Empty;
        public TimeSpan Duration = TimeSpan.Zero;
        public int Bitrate = 0;
        public FileInfo FileInfo;
    }


}