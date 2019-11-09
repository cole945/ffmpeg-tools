using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Shell32;
using System.Text.RegularExpressions;
using System.IO;

namespace TagTime
{
    class Program
    {
        static string ffmpeg_exe = string.Empty; //@"D:\Downloads\ffmpeg\bin\ffmpeg.exe";
        static string ffprobe_exe = string.Empty;// @"D:\Downloads\ffmpeg\bin\ffprobe.exe";

        class MediaObject
        {
            public int CRF;
            public string suffix;
            public string codec;
            public string outdir;

            public string Filename;
            public string Video = string.Empty;
            public string Audio = string.Empty;
            public string Resolution = string.Empty;

            public TimeSpan Duration = TimeSpan.Zero;
            public int Bitrate = 0;

            public DateTime CreatedTime = DateTime.Now;

        }

        static string[] StreamStringSplit(string s)
        {
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

        static MediaObject GetMediaObject(string file, MediaObject mo)
        {
            mo.Filename = file;
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


            // Regex durationRegex = new Regex(@"Duration: (?<DURATION>[^ ]+),.*bitrate: (?<BITRATE>[0-9]+)", RegexOptions.Compiled);
            // Regex videoStream = new Regex(@"Stream .*Video: (?<INFO>.*)", RegexOptions.Compiled);
            // Regex audioStream = new Regex(@"Stream .*Audio: (?<INFO>.*)", RegexOptions.Compiled);
            process.Start();

            while (true)
            {
                var line = process.StandardError.ReadLine();
                if (line == null || line == string.Empty)
                    break;

                int sp = line.IndexOf(":");
                if (sp == -1)
                    continue;

                string key = line.Substring(0, sp).Trim();
                string value = line.Substring(sp + 1).Trim();

                if (key == "creation_time")
                {
                    mo.CreatedTime = DateTime.Parse(value).ToLocalTime();
                }
                else if (key == "Duration")
                {
                    // Duration: 00:01:49.44, start: 0.000000, bitrate: 17688 kb/s
                    mo.Duration = TimeSpan.Parse(value.Split(',')[0]);
                }

                /*
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
                    int bitrate;
                    if (int.TryParse(VideoStreamInfo[3].Trim().Split(' ')[0], out bitrate) && bitrate < mo.Bitrate)
                        mo.Bitrate = bitrate;
                    mo.Resolution = VideoStreamInfo[2].Split(' ')[0];
                }

                m = audioStream.Match(line);
                if (m.Success)
                    mo.Audio = m.Groups["INFO"].Value.Split(',')[0];
                    */
            }

            process.Dispose();
            return mo;
        }

        static void Main(string[] args)
        {

            // initial ffmpeg
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
                else if (File.Exists(Path.Combine(d, "ffmpeg", "bin", "ffmpeg.exe")) &&
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
                Environment.Exit(1);
            }


            foreach (string v in args)
            {
                MediaObject mo = new MediaObject();
                mo = GetMediaObject(v, mo);

                // Fix recording time offset issue.
                DateTime CreatedTime = mo.CreatedTime;
                double TotalSeconds = mo.Duration.TotalSeconds;
                CreatedTime = CreatedTime.AddSeconds(-TotalSeconds);

                string DateString = CreatedTime.ToString("yyyyMMdd");
                DateString = Path.Combine(Path.GetDirectoryName(v), DateString);
                if (!Directory.Exists(DateString))
                    Directory.CreateDirectory(DateString);

                string prefix = CreatedTime.ToString("MMddHHmmss");
                string fn = "";
                if (!Path.GetFileName(v).StartsWith(prefix))
                {
                    // fn = Path.Combine(Path.GetDirectoryName(v), prefix + Path.GetFileName(v));
                    fn = Path.Combine(Path.GetDirectoryName(v), prefix + Path.GetExtension(v));
                    File.Move(v, fn);
                }
                else
                {
                    fn = v;
                }


                string SubFilePath = Path.Combine(Path.GetDirectoryName(fn), Path.GetFileNameWithoutExtension(fn)) + ".ssa";
                StreamWriter w = new StreamWriter(SubFilePath, false);

                w.WriteLine("[Script Info]");
                w.WriteLine("ScriptType: v4.00");
                w.WriteLine("Collisions: Normal");
                w.WriteLine("Timer: 100,0000");
                w.WriteLine("");

                w.WriteLine("[V4 Styles]");
                w.WriteLine("Format: Name,    Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour,  Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding");
                w.WriteLine("Style:  Default, Consolas, 12,       65535,         11861244,        11861244,       -2147483640, 0,    0,      1,           1,       0,      7,         10,      10,      10,      0,          1");
                w.WriteLine("");

                w.WriteLine("[Events]");
                w.WriteLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
                for (int i = 0; i < TotalSeconds + 1; i++)
                {
                    // Console.WriteLine(mo.CreatedTime.ToLocalTime());
                    // 1
                    // 00:00:00,000-- > 00:00:01,000
                    // {\a7} 111

                    w.WriteLine(String.Format("Dialogue: 0,{0}.00,{1}.00,Default, NTP,0000,0000,0000,,{2}",
                        TimeSpan.FromSeconds(i).ToString(@"hh\:mm\:ss"),
                        TimeSpan.FromSeconds(i + 1).ToString(@"hh\:mm\:ss"),
                        CreatedTime.AddSeconds(i).ToString("yyyy/MM/dd  HH:mm:ss")));
                    /*
                    w.WriteLine(i);
                    w.WriteLine(
                        TimeSpan.FromSeconds(i).ToString(@"hh\:mm\:ss") + ",000" +
                        " --> " +
                        TimeSpan.FromSeconds(i + 1).ToString(@"hh\:mm\:ss") + ",000");
                    w.WriteLine("{\\a7} <font face=\"Consolas\" size=\"12\">" + mo.CreatedTime.AddSeconds(i).ToString("yyyy/MM/dd  HH:mm:ss") + "</font>");
                    w.WriteLine("");
                    */
                }
                w.Close();

                File.Move(fn, Path.Combine(DateString, Path.GetFileName(fn)));
                File.Move(SubFilePath, Path.Combine(DateString, Path.GetFileName(SubFilePath)));
            }

        }
    }
}
