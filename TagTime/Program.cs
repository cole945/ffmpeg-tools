using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Shell32;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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

        static int delay = 0;

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
                    DateTime newDateTime = DateTime.Parse(value).ToLocalTime();
                    if (mo.CreatedTime == DateTime.MinValue || newDateTime < mo.CreatedTime)
                        mo.CreatedTime = newDateTime;
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

        static void processPicture(string v)
        {
            Image image = new Bitmap(v);
            PropertyItem[] propItems = image.PropertyItems;

            Font font = new Font("Consolas", image.Height / 40);
            Brush fb = new SolidBrush(Color.FromArgb(0xff, 0xff, 0));
            Brush bb = new SolidBrush(Color.FromArgb(30, 30, 30));
            foreach (PropertyItem propItem in propItems)
            {
                string valstr;
                switch (propItem.Id)
                {
                    //case 0x0007: // PropertyTagGpsGpsTime
                    //    valstr = System.Text.Encoding.ASCII.GetString(propItem.Value, 0, propItem.Len);
                    //    break;
                    case 0x0132: // PropertyTagDateTime
                        valstr = System.Text.Encoding.ASCII.GetString(propItem.Value, 0, propItem.Len);
                        break;
                    case 0x5033: // PropertyTagThumbnailDateTime
                        valstr = System.Text.Encoding.ASCII.GetString(propItem.Value, 0, propItem.Len);
                        break;
                    //case 0x829A: // PropertyTagExifExposureTime
                    //    valstr = System.Text.Encoding.ASCII.GetString(propItem.Value, 0, propItem.Len);
                    //    break;
                    default:
                        valstr = "";
                        break;
                }

                if (valstr == string.Empty)
                    continue;
                // Console.WriteLine("{0}: {1}", propItem.Id.ToString("x"), valstr);
                DateTime CreatedTime;
                while (valstr.Last() == '\0')
                    valstr = valstr.Substring(0, valstr.Length - 1);
                if (!DateTime.TryParse(valstr, out CreatedTime))
                {
                    if (!DateTime.TryParseExact(valstr, "yyyy:MM:dd HH:mm:ss",
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 System.Globalization.DateTimeStyles.None, out CreatedTime))
                        break;
                }

                valstr = CreatedTime.ToString("yyyy/MM/dd HH:mm:ss");
                var g = Graphics.FromImage(image);
                SizeF sf = g.MeasureString(valstr, font);

                int x = (int)(image.Width - sf.Width - font.Size);
                int y = 0;
                for (int i = 0; i < 9; i++)
                {
                    int xx = x + ((i % 3) - 1) * 3;
                    int yy = y + ((i / 3) - 1) * 3;

                    g.DrawString(valstr, font, bb, xx, yy);
                }
                g.DrawString(valstr, font, fb, x, y);

                string dir = Path.GetDirectoryName(v);
                string stem = Path.GetFileNameWithoutExtension(v);
                string ext = Path.GetExtension(v);
                string tagedFile = Path.Combine(dir, stem + "_" + CreatedTime.ToString("yyyyMMddHHmmss") + ext);
                image.Save(tagedFile);
                g.Dispose();
                image.Dispose();

                string DateString = CreatedTime.ToString("yyyyMMdd");
                DateString = Path.Combine(Path.GetDirectoryName(v), DateString);
                if (!Directory.Exists(DateString))
                    Directory.CreateDirectory(DateString);
                string prefix = CreatedTime.ToString("MMddHHmmss");

                File.Move(v, Path.Combine(DateString, prefix + "_" + Path.GetFileName(v)));
                File.Move(tagedFile, Path.Combine(DateString, prefix + "_" + Path.GetFileName(tagedFile)));

                break;
            }


        }
        static void processVideo(string v)
        {
            MediaObject mo = new MediaObject();
            mo = GetMediaObject(v, mo);

            // Fix recording time offset issue.
            DateTime CreatedTime = mo.CreatedTime;
            double TotalSeconds = mo.Duration.TotalSeconds;
            CreatedTime = CreatedTime.AddSeconds(-TotalSeconds + delay);

            string DateString = CreatedTime.ToString("yyyyMMdd");
            DateString = Path.Combine(Path.GetDirectoryName(v), DateString);
            if (!Directory.Exists(DateString))
                Directory.CreateDirectory(DateString);

            string prefix = CreatedTime.ToString("yyyyMMddHHmmss");
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


            string SubFilePath = Path.Combine(Path.GetDirectoryName(fn), Path.GetFileNameWithoutExtension(fn)) + ".ass";
            StreamWriter w = new StreamWriter(SubFilePath, false);

            if (template == string.Empty)
            {
                w.WriteLine("[Script Info]");
                w.WriteLine("ScriptType: v4.00+");
                w.WriteLine("Collisions: Normal");
                w.WriteLine("Timer: 100,0000");
                w.WriteLine("");

                // &HAABBGGRR&
                w.WriteLine("[V4+ Styles]");
                w.WriteLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding");

                // https://github.com/tanersener/mobile-ffmpeg/blob/master/src/libass/libass/ass.c
                List<string> styleList = new List<string>();
                // Name
                styleList.Add("Default");
                // Fontname, Fontsize
                styleList.Add("Consolas");
                styleList.Add("8");
                // PrimaryColour, SecondaryColour, OutlineColour, BackColour
                styleList.Add("&H00B0B0B0");
                styleList.Add("&H00B4FCFC");
                styleList.Add("&H00000000");
                styleList.Add("&H0000FF00");
                // Bold, Italic, Underline, StrikeOut
                styleList.Add("-1");
                styleList.Add("0");
                styleList.Add("0");
                styleList.Add("0");
                // ScaleX, ScaleY
                styleList.Add("100.0");
                styleList.Add("100.0");
                // Spacing, Angle
                styleList.Add("0");
                styleList.Add("0");
                // BorderStyle, Outline, Shadow
                styleList.Add("1");
                styleList.Add("0.3");
                styleList.Add("0");
                // Alignment
                styleList.Add("9");
                // MarginL, MarginR, MarginV
                styleList.Add("3");
                styleList.Add("3");
                styleList.Add("1");
                // Encoding
                styleList.Add("0");
                w.WriteLine("Style: " + string.Join(",", styleList));

                // Style:  Default, Consolas, 8,        &H00B0B0B0&,   &H00B4FCFC&,     &H00000000&,   &H0000FF00&,  0,    0,      1,           0.3,     0,      7,         10,      5,       1,       0,          1

                // -1,0,0,0,100,100,0.00,0.00,1,1.00,2.00,2,30,30,30,0
            }
            else
            {
                w.WriteLine(template);
            }


            w.WriteLine("");

            w.WriteLine("[Events]");
            w.WriteLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
            for (int i = 0; i < TotalSeconds + 20; i++)
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

            try
            {

                File.Move(fn, Path.Combine(DateString, Path.GetFileName(fn)));
                File.Move(SubFilePath, Path.Combine(DateString, Path.GetFileName(SubFilePath)));
            }catch (Exception e)
            {
                Console.WriteLine( e.Message);
                }
        }

        static string template = string.Empty;

        static void Main(string[] args)
        {
            string name = System.AppDomain.CurrentDomain.FriendlyName;
            Regex r = new Regex(@"(?<NAME>.*?)((?<SIGN>[+-])(?<NUM>[0-9]+))?\.exe");

            Match m = r.Match(name);
            if (m.Success)
            {
                if (int.TryParse(m.Groups["NUM"].Value, out delay))
                {
                    if (m.Groups["SIGN"].Value == "-")
                    {
                        delay = -delay;
                    }

                }

                string tmpName = string.Empty;
                string mainName = m.Groups["NAME"].Value;
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, mainName + ".ass")))
                {
                    tmpName = Path.Combine(Environment.CurrentDirectory, mainName + ".ass");
                }
                else if (File.Exists(Path.Combine(Environment.CurrentDirectory, mainName + ".ssa")))
                {
                    tmpName = Path.Combine(Environment.CurrentDirectory, mainName + ".ssa");
                }
                if (File.Exists(tmpName))
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(tmpName))
                        {
                            template = sr.ReadToEnd();
                        }
                    }
                    catch (Exception)
                    {
                        template = string.Empty;
                    }
                }
            }

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
                string ext = Path.GetExtension(v).ToLower();
                if (ext == ".jpg")
                    processPicture(v);
                else
                    processVideo(v);
            }

        }
    }
}
