using MediaInfoLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gepmff
{
    class FFmpegHelper
    {
        string ffmpeg_exe = string.Empty; //@"D:\Downloads\ffmpeg\bin\ffmpeg.exe";
        string ffprobe_exe = string.Empty;// @"D:\Downloads\ffmpeg\bin\ffprobe.exe";
        string vmaf_model = string.Empty;
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
            VideoExt.Add(".rmvb");
            VideoExt.Add(".ts");
            VideoExt.Add(".mts");

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
#if false
            vmaf_model = ffmpeg_exe;
            vmaf_model = Path.GetDirectoryName(vmaf_model);
            try
            {
                while (vmaf_model != string.Empty)
                {
                    string mfile = "vmaf_v0.6.1.json";
                    if (File.Exists(Path.Combine(vmaf_model, "share", "model", mfile)))
                    {
                        vmaf_model = Path.Combine(vmaf_model, "share", "model", mfile);
                        break;
                    }
                    if (File.Exists(Path.Combine(vmaf_model, "model", mfile)))
                    {
                        vmaf_model = Path.Combine(vmaf_model, "model", mfile);
                        break;
                    }
                    vmaf_model = Path.GetDirectoryName(vmaf_model);
                }
                vmaf_model = vmaf_model.Replace('\\', '/').Replace(":", "\\:");
            }
            catch (Exception)
            {
                vmaf_model = string.Empty;
            }
#endif
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

        string[] splitWithParentheses(string s)
        {
            List<string> list = new List<string>();

            int start = 0;
            int end = 0;

            Stack<char> stack = new Stack<char>();
            Dictionary<char, char> cmap = new Dictionary<char, char>();
            HashSet<char> cset = new HashSet<char>();

            cmap[')'] = '(';
            cmap[']'] = '[';
            cmap['}'] = '{';
            cmap['"'] = '"';
            cmap['\''] = '\'';

            cset.Add('(');
            cset.Add('[');
            cset.Add('{');
            cset.Add('"');
            cset.Add('\'');

            while (start >= end)
            {
                char c = s[end];

                if (c == ',' && stack.Count == 0)
                {
                    string sub = s.Substring(start, end - start).Trim();
                    list.Add(sub);
                    start = end;
                    continue;
                }

                if (cset.Contains(c))
                {
                    stack.Push(c);
                    continue;
                }
                else if (cmap[c] == stack.Peek())
                {
                    stack.Pop();
                    continue;
                }
            }

            if (end > start)
            {
                string sub = s.Substring(start, end - start).Trim();
                list.Add(sub);
            }
            return list.ToArray();
        }
        public MediaObject GetMediaObject(string file)
        {
            if (!VideoExt.Contains(Path.GetExtension(file)))
                return null;

            MediaObject mo = new MediaObject();
            mo.FileInfo = new FileInfo(file);

#if false
            var mi = new MediaInfo();
            mi.Open(file);
            var videoInfo = new VideoInfo(mi);
            var audioInfo = new AudioInfo(mi);

            mo.Video = videoInfo.Codec;
            mo.Audio = audioInfo.Codec;
            mo.Duration = videoInfo.Duration;
            mo.Bitrate = (videoInfo.Bitrate / 1024);
            mo.Resolution = videoInfo.Width.ToString() + "x" + videoInfo.Heigth.ToString();
#endif

#if true
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
#endif
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

            try
            {
                ffmpeg_process.PriorityClass = ProcessPriorityClass.BelowNormal;

                Regex progressRegex = new Regex(@"(?<KEY>[a-z]+)=\s*(?<VALUE>[^ ]+)\s*", RegexOptions.Compiled);
                Regex digRegex = new Regex(@"\d+");
                Regex fltRegex = new Regex(@"\d+(\.\d+)?");
                Regex vmafRegex = new Regex(@"VMAF score: (?<VMAF>[0-9\.]+)");
                while (true)
                {
                    var line = ffmpeg_process.StandardError.ReadLine();
                    if (line == null || line == string.Empty)
                        break;

                    MatchCollection mc = progressRegex.Matches(line);
                    if (mc.Count == 0)
                    {
                        if (line.IndexOf("Non-monotonous DTS") > 0)
                            eo.dts_warning++;
                        else if (line.IndexOf("pts has no value") > 0)
                            eo.pts_warning++;
                        Match m = vmafRegex.Match(line);
                        if (m.Success)
                            eo.vmaf_score = m.Groups["VMAF"].Value;
                        continue;
                    }

                    // string[] groups = new string[] { "frame", "fps", "size", "time", "bitrate", "speed", "q" };

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
                    if (info.TryGetValue("q", out v))
                    {
                        Match m = fltRegex.Match(v);
                        if (m.Success)
                            float.TryParse(m.Groups[0].Value, out pi.q);
                    }

                    if (ProgressUpdate != null)
                    {
                        ProgressInfoEventArgs e = new ProgressInfoEventArgs();
                        e.Info = pi;
                        e.EO = eo;
                        ProgressUpdate(this, e);

                        if (e.EO.Status == Status.ErrorMayProduceLargerFile)
                        {
                            Kill();
                            return -1;
                        }
                    }
                }
            }
            finally { }

            if (!ffmpeg_process.WaitForExit(500))
            {
                Kill();
            }
            int exitcode = ffmpeg_process.ExitCode;
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
            string outfilename;

            if (eo.outfile != string.Empty)
                outfilename = eo.outfile;
            else
                outfilename = Path.Combine(eo.outdir, filename) + eo.suffix + ".mp4";

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
                if (eo.codec == "vmaf")
                {
                    StringBuilder arguments = new StringBuilder();
                    string vffps = string.Empty;
                    if (eo.fps.Trim().Length > 1)
                        vffps = eo.fps;
                    else
                        vffps = eo.MO.FPS;

                    string tmpfile = Path.GetTempFileName();
                    arguments.Append(" -i \"").Append(eo.MO.FileInfo.FullName).Append("\"");
                    arguments.Append(" -i \"").Append(eo.vmaffile).Append("\"");
                    arguments.Append(" -lavfi ");
                    arguments.Append("[0:v]framerate=").Append(vffps).Append("[distorted];[1:v]framerate=").Append(vffps).Append("[ref];[distorted][ref]");
                    arguments.Append("libvmaf=\"")
                        // .Append("log_path='").Append(outfilename.Replace('\\', '/').Replace(":", "\\:")).Append("'")
                        .Append("log_path='").Append(tmpfile.Replace('\\', '/').Replace(":", "\\:")).Append("'")
                        // .Append(":psnr=0")
                        .Append(":log_fmt=json:model_path='").Append(vmaf_model);


                    if (eo.bf > 0)
                        arguments.Append(":n_subsample=").Append(eo.bf);

                    arguments.Append("'\"")
                        .Append(" -f null -");

                    exitcode = executeFFmpeg(arguments.ToString(), eo);
                    File.Move(tmpfile, outfilename);
                }
                else if (eo.QType == EncodeObject.QualityEnum.CRF)
                {
                    StringBuilder arguments = new StringBuilder();

                    if (eo.vsync != string.Empty)
                        arguments.Append("-vsync ").Append(eo.vsync);

                    if (eo.hwdec)
                    {
                        foreach (var codecstr in new string[] { "h264", "mjpeg", "mpeg1", "mpeg2", "mpeg4", "vc1", "vp8", "vp9" })
                        {
                            if (eo.MO.Video.ToLower().StartsWith(codecstr))
                                arguments.Append(" -hwaccel cuvid -c:v " + codecstr + "_cuvid ");
                        }
                    }

                    arguments.Append(" -i \"").Append(eo.MO.FileInfo.FullName).Append("\"");
                    if (eo.ss != TimeSpan.MaxValue)
                        arguments.Append(" -ss " + eo.ss.ToString(@"hh\:mm\:ss\.fff"));
                    if (eo.to != TimeSpan.MaxValue)
                        arguments.Append(" -to " + eo.to.ToString(@"hh\:mm\:ss\.fff"));
                    arguments.Append(" -c:v ").Append(eo.codec);
                    if (eo.codec == "hevc_nvenc")
                    {
                        arguments.Append(" -preset slow");
                        arguments.Append(" -profile:v main ");
                        // arguments.Append(" -rc:v vbr_hq ");
                        arguments.Append(" -cq:v ").Append(eo.Q.ToString());
                        // arguments.Append(" -qmin ").Append((eo.Q - 2).ToString());
                        // arguments.Append(" -qmax ").Append((eo.Q + 2).ToString());
                        if (eo.bf != 0)
                            arguments.Append(" -bf:v " + eo.bf.ToString());
                        if (eo.b_ref_mode > 0)
                            arguments.Append(" -b_ref_mode " + eo.b_ref_mode.ToString());
                        arguments.Append(" -rc-lookahead 32");
                        if (eo.temporal_aq)
                            arguments.Append(" -temporal-aq 1");
                        if (eo.spatial_aq)
                            arguments.Append(" -spatial-aq 1");
                        if (eo.multipass >= 0)
                            arguments.Append(" -multipass ").Append(eo.multipass.ToString());
                    }
                    else if (eo.codec == "copy")
                    {
                    }
                    else if (eo.codec == "libx265" || eo.codec == "libx264")
                    {
                        arguments.Append(" -preset slow");
                    }
                    else if (eo.codec == "libsvtav1")
                    {
                        arguments.Append(" -preset 6");
                    }


                    if (eo.codec != "copy")
                    {
                        if (eo.yadif == 1)
                            arguments.Append(" -vf yadif").Append(eo.fps);
                        else if (eo.yadif == 2)
                            arguments.Append(" -vf yadif_cuda").Append(eo.fps);
                        if (eo.fps != string.Empty)
                            arguments.Append(" -vf fps=fps=").Append(eo.fps);
                        if (eo.scale > 0)
                            arguments.Append(" -vf scale=\"'-1':'if(gt(ih,").Append(eo.scale).Append("),").Append(eo.scale).Append(",-1)':flags=lanczos\"");
                        if (!eo.codec.EndsWith("_nvenc"))
                            arguments.Append(" -crf ").Append(eo.Q);
                    }
                    if ((eo.acodec == string.Empty || eo.acodec.Trim().ToLower() == "copy") && eo.ss == TimeSpan.MaxValue && eo.to == TimeSpan.MaxValue)
                        arguments.Append(" -c:a copy ");
                    else
                    {
                        arguments.Append(" -c:a " + eo.acodec + " -b:a " + eo.abitrate);
                    }

                    // arguments.Append(" -t 00:05:00.0 ");
                    arguments.Append(" \"").Append(outfilename).Append("\"");

                    exitcode = executeFFmpeg(arguments.ToString(), eo);
                }
                else if (eo.QType == EncodeObject.QualityEnum.AvgBitrate)
                {
                    StringBuilder arguments = new StringBuilder();

                    string ooodir = Path.GetDirectoryName(outfilename);
                    Environment.CurrentDirectory = ooodir;

                    //
                    // pass 1
                    //
                    bool runPass1 = true;
                    bool runPass2 = true;
                    if (runPass1)
                    {
                        arguments.Append(" -y -i \"").Append(eo.MO.FileInfo.FullName).Append("\"");

                        if (eo.ss != TimeSpan.MaxValue)
                            arguments.Append(" -ss " + eo.ss.ToString(@"hh\:mm\:ss\.fff"));
                        if (eo.to != TimeSpan.MaxValue)
                            arguments.Append(" -to " + eo.to.ToString(@"hh\:mm\:ss\.fff"));

                        arguments.Append(" -c:v ").Append(eo.codec);

                        if (eo.fps != string.Empty)
                            arguments.Append(" -vf fps=fps=").Append(eo.fps);
                        if (eo.scale > 0)
                            arguments.Append(" -vf scale=\"'-1':'if(gt(ih," + eo.scale.ToString() + ")," + eo.scale.ToString() + ",-1)':flags=lanczos\"");
                        //arguments.Append(" -preset medium");
                        arguments.Append(" -b:v ").Append(eo.Q).Append("k");
                        arguments.Append(" -an");

                        if (eo.codec == "libx265" || eo.codec == "libx264")
                        {
                            arguments.Append(" -preset medium");
                            if (eo.codec == "libx265")
                                arguments.Append(" -x265-params pass=1");
                            else if (eo.codec == "libx264")
                                arguments.Append(" -pass 1 ");//-fastfirstpass 1 
                        }
                        else if (eo.codec == "libsvtav1")
                        {
                            // https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/Docs/CommonQuestions.md#improving-decoding-performance
                            // https://gist.github.com/mrintrepide/b3009f5d0f08d437ebbb4c17cbf36e18
                            // Default:
                            // ffmpeg -i "Input.mkv" -c:v libsvtav1 -crf 35 -preset 6 -c:a libopus -b:a 128k -ac 2 -c:s copy Output.mkv

                            // Optimal 8 bit:
                            // ffmpeg -i "Input.mkv" -c:v libsvtav1 -crf 35 -preset 6 -svtav1-params tune=0 -g 240 -c:a libopus -b:a 128k -ac 2 -c:s copy Output.mkv

                            // Optimal with 10 bit:
                            // ffmpeg -i "Input.mkv" -pix_fmt yuv420p10le -c:v libsvtav1 -crf 35 -preset 6 -svtav1-params tune=0 -g 240 -c:a libopus -b:a 128k -ac 2 -c:s copy Output.mkv

                            // first pass
                            arguments.Append(" -preset 4");
                            arguments.Append(" -svtav1-params tune=0:pass=1");
                        }
                        else
                        {
                            throw new Exception("unsupported pass1 codec");
                        }
                        arguments.Append(" -f mp4");
                        arguments.Append(" NUL");  // linux:/dev/null 
                        exitcode = executeFFmpeg(arguments.ToString(), eo);

                        if (exitcode != 0)
                            throw new Exception("pass1");
                    }

                    //
                    // Pass 2
                    //
                    if (runPass2)
                    {
                        arguments.Clear();
                        arguments.Append(" -i \"").Append(eo.MO.FileInfo.FullName).Append("\"");
                        if (eo.ss != TimeSpan.MaxValue)
                            arguments.Append(" -ss " + eo.ss.ToString(@"hh\:mm\:ss\.fff"));
                        if (eo.to != TimeSpan.MaxValue)
                            arguments.Append(" -to " + eo.to.ToString(@"hh\:mm\:ss\.fff"));

                        arguments.Append(" -c:v ").Append(eo.codec);

                        if (eo.fps != string.Empty)
                            arguments.Append(" -vf fps=fps=").Append(eo.fps);
                        if (eo.scale > 0)
                            arguments.Append(" -vf scale=\"'-1':'if(gt(ih," + eo.scale.ToString() + ")," + eo.scale.ToString() + ",-1)':flags=lanczos\"");
                        arguments.Append(" -b:v ").Append(eo.Q).Append("k");

                        if (eo.codec == "libx265" || eo.codec == "libx264")
                        {
                            arguments.Append(" -preset medium");
                            if (eo.codec == "libx265")
                                arguments.Append(" -x265-params pass=2");
                            else
                                arguments.Append(" -pass 2");
                        }
                        else if (eo.codec == "libsvtav1")
                        {
                            arguments.Append(" -preset 4");
                            arguments.Append(" -svtav1-params tune=0:pass=2");
                        }

                        if ((eo.acodec == string.Empty || eo.acodec.Trim().ToLower() == "copy") && eo.ss == TimeSpan.MaxValue && eo.to == TimeSpan.MaxValue)
                            arguments.Append(" -c:a copy ");
                        else
                            arguments.Append(" -c:a " + eo.acodec + " -b:a " + eo.abitrate);

                        arguments.Append(" \"").Append(outfilename).Append("\"");

                        exitcode = executeFFmpeg(arguments.ToString(), eo);

                        if (exitcode != 0)
                            throw new Exception("pass2");
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message == "pass1")
                    eo.Status = Status.ErrorPass1Fail;
                else if (e.Message == "pass2")
                    eo.Status = Status.ErrorPass2Fail;
                else
                   eo.Status = Status.ErrorUnknown;
            }

            if (eo.Status != Status.New)
            {
                // Set by event handler
                Result = eo.Status;
            }
            else if (exitcode == 0 && Result == Status.Done && File.Exists(outfilename))
            {
                eo.NewSize = (new FileInfo(outfilename)).Length;

                // this is to avoid confusion jellyfie :(
                File.SetCreationTime(outfilename, File.GetCreationTime(eo.MO.FileInfo.FullName));
                File.SetLastWriteTime(outfilename, File.GetLastWriteTime(eo.MO.FileInfo.FullName));
            }
            // else if (exitcode != 0 && Result != Status.Done)
            //     Result = Status.ErrorUnknown;

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
                try
                {
                    ffmpeg_process.Kill();
                    ffmpeg_process.Dispose();
                    ffmpeg_process = null;
                }
                catch (Exception)
                {
                }
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
            ErrorPass1Fail,
            ErrorPass2Fail,
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
            public int Qmin = 0;
            public int Qmax = 0;

            public string suffix = string.Empty;
            public string codec = string.Empty;
            public string acodec = string.Empty;
            public string abitrate = string.Empty;
            public string outdir = string.Empty;
            public string outfile = string.Empty;
            public string vmaffile = string.Empty;
            public int scale = -1;
            public TimeSpan ss = TimeSpan.MaxValue;
            public TimeSpan to = TimeSpan.MaxValue;

            public bool hwdec = false;
            public int yadif = 0;

            public string fps = string.Empty;
            public long NewSize = 0;
            public int NewBitrate = 0;
            public string vmaf_score = string.Empty;
            public Status Status = Status.New;
            public int dts_warning = 0;
            public int pts_warning = 0;

            public int b_ref_mode = 0;
            public int bf = -1;
            public int multipass = -1;
            public string vsync = "";
            public bool temporal_aq = false;
            public bool spatial_aq = false;

            public int CRF { set { Q = value; QType = QualityEnum.CRF; } get { return Q; } }
            public int AvgBitrate { set { Q = value; QType = QualityEnum.AvgBitrate; } get { return Q; } }

            public void reset()
            {
                QType = QualityEnum.CRF;
                Q = 0;
                Qmin = 0;
                Qmax = 0;

                suffix = string.Empty;
                codec = string.Empty;
                outdir = string.Empty;
                outfile = string.Empty;
                vmaffile = string.Empty;
                scale = -1;
                ss = TimeSpan.MaxValue;
                to = TimeSpan.MaxValue;

                hwdec = false;
                yadif = 0;

                fps = string.Empty;
                NewSize = 0;
                NewBitrate = 0;
                vmaf_score = string.Empty;
                Status = Status.New;
                dts_warning = 0;
                pts_warning = 0;

                b_ref_mode = 0;
                bf = -1;
                multipass = -1;
                vsync = "";
                temporal_aq = false;
                spatial_aq = false;
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
        public class ProgressInfo
        {
            // frame=   48 fps=6.2 q=-0.0 size=     256kB time=00:00:02.70 bitrate= 776.1kbits/s dup=2 drop=0 speed=0.349x  
            public string frame = string.Empty;
            public float fps = 0;
            public int size = 0;
            public TimeSpan time = TimeSpan.Zero;
            public float bitrate = 0;
            public float speed = 0;
            public float q = 0;
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

        public class VideoInfo
        {
            public string Codec { get; private set; }
            public int Width { get; private set; }
            public int Heigth { get; private set; }
            public double FrameRate { get; private set; }
            public string FrameRateMode { get; private set; }
            public string ScanType { get; private set; }
            public TimeSpan Duration { get; private set; }
            public int Bitrate { get; private set; }
            public string AspectRatioMode { get; private set; }
            public double AspectRatio { get; private set; }

            public VideoInfo(MediaInfo mi)
            {
                Codec = mi.Get(StreamKind.Video, 0, "Format");
                Width = int.Parse(mi.Get(StreamKind.Video, 0, "Width"));
                Heigth = int.Parse(mi.Get(StreamKind.Video, 0, "Height"));
                string durationStr = mi.Get(StreamKind.Video, 0, "Duration").Split('.').First();
                Duration = TimeSpan.FromMilliseconds(int.Parse(durationStr));
                string bitrateStr = mi.Get(StreamKind.Video, 0, "BitRate").Split('/').First().Trim();
                Bitrate = int.Parse(bitrateStr);
                AspectRatioMode = mi.Get(StreamKind.Video, 0, "AspectRatio/String"); //as formatted string
                AspectRatio = double.Parse(mi.Get(StreamKind.Video, 0, "AspectRatio"));
                FrameRate = double.Parse(mi.Get(StreamKind.Video, 0, "FrameRate"));
                FrameRateMode = mi.Get(StreamKind.Video, 0, "FrameRate_Mode");
                ScanType = mi.Get(StreamKind.Video, 0, "ScanType");
            }
        }

        public class AudioInfo
        {
            public string Codec { get; private set; }
            public string CompressionMode { get; private set; }
            public string ChannelPositions { get; private set; }
            public TimeSpan Duration { get; private set; }
            public int Bitrate { get; private set; }
            public string BitrateMode { get; private set; }
            public int SamplingRate { get; private set; }

            public AudioInfo(MediaInfo mi)
            {
                Codec = mi.Get(StreamKind.Audio, 0, "Format");
                string durationStr = mi.Get(StreamKind.Audio, 0, "Duration").Split('.').First();
                Duration = TimeSpan.FromMilliseconds(int.Parse(durationStr));
                string bitrateStr = mi.Get(StreamKind.Audio, 0, "BitRate").Split('/').First().Trim();
                Bitrate = int.Parse(bitrateStr);
                BitrateMode = mi.Get(StreamKind.Audio, 0, "BitRate_Mode");
                CompressionMode = mi.Get(StreamKind.Audio, 0, "Compression_Mode");
                ChannelPositions = mi.Get(StreamKind.Audio, 0, "ChannelPositions");
                SamplingRate = int.Parse(mi.Get(StreamKind.Audio, 0, "SamplingRate"));
            }
        }
    }
}
