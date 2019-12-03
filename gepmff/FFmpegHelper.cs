﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gepmff
{
    class FFmpegHelper
    {
        string ffmpeg_exe = string.Empty; //@"D:\Downloads\ffmpeg\bin\ffmpeg.exe";
        string ffprobe_exe = string.Empty;// @"D:\Downloads\ffmpeg\bin\ffprobe.exe";
        HashSet<string> VideoExt;
        Process ffmpeg_process = null;
        public string getFFmpegPath()
        {
            return ffmpeg_exe;
        }
        public string getFFprobePath()
        {
            return ffprobe_exe;
        }

        public FFmpegHelper()
        {
            VideoExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            VideoExt.Add(".avi");
            VideoExt.Add(".mpg");
            VideoExt.Add(".mpeg");
            VideoExt.Add(".mp4");
            VideoExt.Add(".wmv");
            VideoExt.Add(".m4v");
            VideoExt.Add(".mkv");
            VideoExt.Add(".flv");
            VideoExt.Add(".mov");

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
        }

        string[] StreamStringSplit(string s)
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
        public MediaObject GetMediaObject(string file)
        {
            if (!VideoExt.Contains(Path.GetExtension(file)))
                return null;

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
            process.Start();

            while (true)
            {
                var line = process.StandardError.ReadLine();
                if (line == null || line == string.Empty)
                    break;

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
            }

            process.Dispose();
            if (mo.Duration == TimeSpan.Zero || mo.Bitrate == 0 || mo.Video == string.Empty)
                return null;
            return mo;
        }

        private int executeFFmpeg(string args, EncodeObject eo)
        {

            ffmpeg_process = new Process
            {
                StartInfo = {
                    FileName = ffmpeg_exe,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    Arguments = args,
                    CreateNoWindow = true
                },
            };

            ffmpeg_process.Start();
            ffmpeg_process.PriorityClass = ProcessPriorityClass.BelowNormal;

            try
            {
                Regex progressRegex = new Regex(@"(?<KEY>[a-z]+)=\s*(?<VALUE>[^ ]+)\s*", RegexOptions.Compiled);
                Regex digRegex = new Regex(@"\d+");
                Regex fltRegex = new Regex(@"\d+(\.\d+)?");
                while (true)
                {
                    var line = ffmpeg_process.StandardError.ReadLine();
                    if (line == null || line == string.Empty)
                        break;

                    MatchCollection mc = progressRegex.Matches(line);
                    if (mc.Count == 0)
                        continue;

                    string[] groups = new string[] { "frame", "fps", "size", "time", "bitrate", "speed" };

                    Dictionary<string, string> info = new Dictionary<string, string>();
                    foreach (Match m in mc)
                        info[m.Groups["KEY"].Value] = m.Groups["VALUE"].Value;

                    Debug.WriteLine(line);
                    ProgressInfo pi = new ProgressInfo();
                    string v;

                    info.TryGetValue("frame", out pi.frame);
                    if (info.TryGetValue("fps", out v))
                    {
                        Match m = fltRegex.Match(v);
                        if (m.Success)
                            float.TryParse(m.Groups[0].Value, out pi.fps);
                    }

                    if (info.TryGetValue("size", out v))
                    {
                        Match m = digRegex.Match(v);
                        if (m.Success)
                            int.TryParse(m.Groups[0].Value, out pi.size);
                    }
                    if (info.TryGetValue("time", out v))
                    {
                        TimeSpan.TryParse(v, out pi.time);
                    }
                    if (info.TryGetValue("bitrate", out v))
                    {
                        Match m = fltRegex.Match(v);
                        if (m.Success)
                        {
                            float.TryParse(m.Groups[0].Value, out pi.bitrate);
                            eo.NewBitrate = (int)pi.bitrate;
                        }
                    }
                    if (info.TryGetValue("speed", out v))
                    {
                        Match m = fltRegex.Match(v);
                        if (m.Success)
                            float.TryParse(m.Groups[0].Value, out pi.speed);
                    }

                    if (ProgressUpdate != null)
                    {
                        ProgressInfoEventArgs e = new ProgressInfoEventArgs();
                        e.Info = pi;
                        e.EO = eo;
                        ProgressUpdate(this, e);
                    }
                }
            }
            finally { }

            if (!ffmpeg_process.WaitForExit(500))
            {
                ffmpeg_process.Kill();
                ffmpeg_process.WaitForExit();
            }
            int exitcode = ffmpeg_process.ExitCode;
            ffmpeg_process.Dispose();
            return exitcode;
        }

        public Status EncodeVideo(EncodeObject eo)
        {
            Status Result = Status.Done;
            if (!File.Exists(eo.MO.FileInfo.FullName))
            {
                if (Completed != null)
                {
                    CompletedEventArgs e = new CompletedEventArgs();
                    e.Result = Status.ErrorFileNotFound;
                    eo.Status = Status.ErrorFileNotFound;
                    e.EO = eo;
                    Completed(this, e);
                }
            }

            string filename = Path.GetFileNameWithoutExtension(eo.MO.FileInfo.FullName);
            string ext = eo.MO.FileInfo.Extension;
            // -i 101S102_CT01V01.mp4 -c:v libx265 -vf scale=-1:720 -preset medium -crf 23 -acodec copy 101S102_CT01V01-x265.mp4
            // nvenc_hevc  hevc_qsv

            // \"'-1':'if(gt(a,720),720,-1)'\"
            if (eo.outdir == string.Empty)
                eo.outdir = eo.MO.FileInfo.DirectoryName;
            string outfilename = Path.Combine(eo.outdir, filename) + eo.suffix + ".mp4";

            if (File.Exists(outfilename))
            {
                if (Completed != null)
                {
                    CompletedEventArgs e = new CompletedEventArgs();
                    e.Result = Status.ErrorFileAlreadyExists;
                    eo.Status = Status.ErrorFileAlreadyExists;
                    e.EO = eo;
                    Completed(this, e);
                }
                return Status.ErrorFileAlreadyExists;
            }

            int exitcode = -1;
            try
            {
                if (eo.QType == EncodeObject.QualityEnum.CRF)
                {
                    StringBuilder arguments = new StringBuilder();
                    arguments.Append(" -i \"").Append(eo.MO.FileInfo.FullName).Append("\"");
                    arguments.Append(" -c:v ").Append(eo.codec);
                    if (eo.scale > 0)
                        arguments.Append(" -vf scale=\"'-1':'if(gt(ih," + eo.scale.ToString() + ")," + eo.scale.ToString() + ",-1)':flags=lanczos\"");
                    arguments.Append(" -preset medium");
                    arguments.Append(" -crf ").Append(eo.Q);
                    if (eo.MO.Audio.StartsWith("aac"))
                        arguments.Append(" -acodec copy ");
                    arguments.Append(" \"").Append(outfilename).Append("\"");

                    exitcode = executeFFmpeg(arguments.ToString(), eo);
                }
                else if (eo.QType == EncodeObject.QualityEnum.AvgBitrate)
                {
                    StringBuilder arguments = new StringBuilder();

                    if (eo.codec == "libx264" || eo.codec == "libx265")
                    {
                        // first pass
                        arguments.Append(" -y -i \"").Append(eo.MO.FileInfo.FullName).Append("\"");
                        arguments.Append(" -c:v ").Append(eo.codec);
                        if (eo.scale > 0)
                            arguments.Append(" -vf scale=\"'-1':'if(gt(ih," + eo.scale.ToString() + ")," + eo.scale.ToString() + ",-1)':flags=lanczos\"");
                        arguments.Append(" -preset medium");
                        arguments.Append(" -b:v ").Append(eo.Q).Append("k");
                        arguments.Append(" -an");
                        if (eo.codec == "libx265")
                            arguments.Append(" -x265-params no-slow-firstpass=1:pass=1");
                        else if (eo.codec == "libx264")
                            arguments.Append(" -pass 1 -fastfirstpass 1 ");
                        arguments.Append(" -f mp4");
                        arguments.Append(" NUL");
                        exitcode = executeFFmpeg(arguments.ToString(), eo);
                    }
                    else
                        exitcode = 0;
                    if (exitcode == 0)
                    {
                        arguments.Clear();
                        arguments.Append(" -i \"").Append(eo.MO.FileInfo.FullName).Append("\"");
                        arguments.Append(" -c:v ").Append(eo.codec);
                        if (eo.scale > 0)
                            arguments.Append(" -vf scale=\"'-1':'if(gt(ih," + eo.scale.ToString() + ")," + eo.scale.ToString() + ",-1)':flags=lanczos\"");
                        arguments.Append(" -preset medium");
                        arguments.Append(" -b:v ").Append(eo.Q).Append("k");
                        if (eo.codec == "libx265")
                            arguments.Append(" -x265-params pass=2");
                        else
                            arguments.Append(" -pass 2");
                        if (eo.MO.Audio.StartsWith("aac"))
                            arguments.Append(" -acodec copy ");
                        arguments.Append(" \"").Append(outfilename).Append("\"");

                        exitcode = executeFFmpeg(arguments.ToString(), eo);
                    }
                }
            } catch(Exception e)
            {
                eo.Status = Status.ErrorUnknown;
            }

            if (eo.Status != Status.New)
            {
                // Set by event handler
                Result = eo.Status;
            }
            else if (exitcode == 0 && Result == Status.Done)
            {
                eo.NewSize = (new FileInfo(outfilename)).Length;
            }
            else if (exitcode != 0 && Result != Status.Done)
                Result = Status.ErrorUnknown;

            if (Completed != null)
            {
                CompletedEventArgs e = new CompletedEventArgs();
                e.Result = Result;
                eo.Status = Result;
                e.EO = eo;
                Completed(this, e);
            }

            return Result;
        }

        public void Kill()
        {
            if (ffmpeg_process != null)
            {
                ffmpeg_process.Kill();
                ffmpeg_process.Dispose();
            }
        }
        public enum Status
        {
            New,
            Done,
            ErrorFileNotFound,
            ErrorFileAlreadyExists,
            ErrorMayProduceLargerFile,
            ErrorOutpuFileIsLarger,
            ErrorUnknown
        }

        public class EncodeObject
        {
            public MediaObject MO;
            public enum QualityEnum
            {
                CRF,
                AvgBitrate
            }

            public QualityEnum QType = QualityEnum.CRF;
            public int Q = 0;

            public string suffix = string.Empty;
            public string codec = string.Empty;
            public string outdir = string.Empty;
            public string outfile = string.Empty;
            public int scale = -1;

            public long NewSize = 0;
            public int NewBitrate = 0;
            public Status Status = Status.New;

            public int CRF { set { Q = value; QType = QualityEnum.CRF; } get { return Q; } }
            public int AvgBitrate { set { Q = value; QType = QualityEnum.AvgBitrate; } get { return Q; } }
        }
        public class MediaObject
        {
            public string Video = string.Empty;
            public string Audio = string.Empty;
            public string Resolution = string.Empty;
            public TimeSpan Duration = TimeSpan.Zero;
            public int Bitrate = 0;
            public FileInfo FileInfo;
        }
        public class ProgressInfo
        {
            // frame=   48 fps=6.2 q=-0.0 size=     256kB time=00:00:02.70 bitrate= 776.1kbits/s dup=2 drop=0 speed=0.349x  
            public string frame = string.Empty;
            public float fps = 0;
            public int size = 0;
            public TimeSpan time = TimeSpan.Zero;
            public float bitrate = 0;
            public float speed = 0;
        }

        public event EventHandler<ProgressInfoEventArgs> ProgressUpdate;
        public event EventHandler<CompletedEventArgs> Completed;

        public class ProgressInfoEventArgs : EventArgs
        {
            public ProgressInfo Info { get; set; }
            public EncodeObject EO { get; set; }
        }
        public class CompletedEventArgs : EventArgs
        {
            public Status Result { get; set; }
            public EncodeObject EO { get; set; }
        }
    }
}
