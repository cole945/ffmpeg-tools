using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Shell32;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using System.Windows.Shell;

namespace gepmff
{
    public partial class GepmffForm : Form
    {
        string ffmpeg_exe = string.Empty; //@"D:\Downloads\ffmpeg\bin\ffmpeg.exe";
        string ffprobe_exe = string.Empty;// @"D:\Downloads\ffmpeg\bin\ffprobe.exe";
        HashSet<string> VideoExt;

        public GepmffForm()
        {
            InitializeComponent();

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
            comboBoxCodec.SelectedIndex = 0;

            onChangeSetting(comboBoxCodec.Text, trackBarCRF.Value);

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
                Environment.Exit(1);
            }
            this.Text = ffmpeg_exe;
        }

        void onChangeSetting(string codec, int crf)
        {
            label1.Text = crf.ToString();
            textBoxSuffix.Text = "-" + codec + "-" + crf.ToString();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            onChangeSetting(comboBoxCodec.Text, trackBarCRF.Value);
            toolTip1.SetToolTip(sender as Control, (sender as TrackBar).Value.ToString());
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(sender as Control, (sender as TrackBar).Value.ToString());
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            onChangeSetting(comboBoxCodec.Text, trackBarCRF.Value);
        }

        void DeleteFile(string filename, int retry)
        {
            while (retry > 0)
            {
                try
                {
                    File.Delete(filename);
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    retry--;
                    Thread.Sleep(500);
                }
                catch (System.IO.IOException)
                {
                    return;
                }
            }
        }

        Process ffmpeg_process = null;
        enum EncodingResult
        {
            OK,
            FileNotFound,
            FileAlreadyExists,
            MayProduceLargerFile,
            OutpuFileIsLarger,
            UnknownError
        }
        EncodingResult encodeVideo(MediaObject mo)
        {
            EncodingResult Result = EncodingResult.OK;
            if (!File.Exists(mo.Filename))
                return EncodingResult.FileNotFound;

            string filename = Path.GetFileNameWithoutExtension(mo.Filename);
            string ext = Path.GetExtension(mo.Filename);
            StringBuilder arguments = new StringBuilder();
            // -i 101S102_CT01V01.mp4 -c:v libx265 -vf scale=-1:720 -preset medium -crf 23 -acodec copy 101S102_CT01V01-x265.mp4
            // nvenc_hevc  hevc_qsv

            // \"'-1':'if(gt(a,720),720,-1)'\"
            if (mo.outdir == string.Empty)
                mo.outdir = Path.GetDirectoryName(mo.Filename);
            string outfilename = Path.Combine(mo.outdir, filename) + mo.suffix + ".mp4";

            if (File.Exists(outfilename))
                return EncodingResult.FileAlreadyExists;

            // arguments.Append(" -ss 00:00:00.0 -t 5 ");
            arguments.Append(" -i \"").Append(mo.Filename).Append("\" -c:v ").Append(mo.codec);
            arguments.Append(" -vf scale=\"'-1':'if(gt(ih,720),720,-1)':flags=lanczos\"  -preset medium -crf ").Append(mo.CRF);
            if (mo.Audio.StartsWith("aac"))
                arguments.Append(" -acodec copy ");
            arguments.Append(" \"").Append(outfilename).Append("\"");


            textBoxLog.Invoke(new Action(() => textBoxLog.Text += ffmpeg_exe + " " + arguments.ToString() + "\r\n"));

            var tcs = new TaskCompletionSource<bool>();

            ffmpeg_process = new Process
            {
                StartInfo = {
                    FileName = ffmpeg_exe,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    Arguments = arguments.ToString(),
                    CreateNoWindow = true
                },
            };

            ffmpeg_process.Start();
            // ffmpeg_process.PriorityClass = ProcessPriorityClass.BelowNormal;

            Regex progressRegex = new Regex(@"(?<KEY>[a-z]+)=\s*(?<VALUE>[^ ]+)\s*", RegexOptions.Compiled);
            Regex digRegx = new Regex(@"[0-9]+");
            int bitrate = 0;
            int newsize = 0;
            while (true)
            {
                var line = ffmpeg_process.StandardError.ReadLine();
                if (line == null || line == string.Empty || Result != EncodingResult.OK)
                    break;

                statusStrip1.Invoke(new Action(() =>
                {
                    MatchCollection mc = progressRegex.Matches(line);
                    if (mc.Count == 0)
                        return;

                    string[] groups = new string[] { "frame", "fps", "size", "time", "bitrate", "speed" };

                    Dictionary<string, string> info = new Dictionary<string, string>();
                    foreach (Match m in mc)
                        info[m.Groups["KEY"].Value] = m.Groups["VALUE"].Value;

                    toolStripStatusLabelConsole.Text = "";
                    foreach (string g in groups)
                    {
                        if (!info.ContainsKey(g))
                            continue;
                        toolStripStatusLabelConsole.Text += g + "=" + info[g] + " ";
                    }
                    toolStripStatusLabelConsole.Text += ": " + Path.GetFileName(mo.Filename);

                    if (info.ContainsKey("bitrate"))
                    {
                        Match m = digRegx.Match(info["bitrate"]);
                        if (m.Success)
                            int.TryParse(m.Groups[0].Value, out bitrate);
                    }
                    if (info.ContainsKey("size"))
                    {
                        Match m = digRegx.Match(info["size"]);
                        if (m.Success)
                            int.TryParse(m.Groups[0].Value, out newsize);
                    }

                    TimeSpan TSTime;
                    if (info.ContainsKey("time") && TimeSpan.TryParse(info["time"], out TSTime))
                    {
                        int curr = (int)TSTime.TotalMilliseconds;
                        if (curr > toolStripProgressBar1.Maximum)
                            toolStripProgressBar1.Maximum = curr;
                        toolStripProgressBar1.Value = curr;
                        // if we have encoding 10min have it seem the bitrate is too high, cancel
                        if ((curr / 1000 / 60) > 5
                            && ((float)bitrate / mo.Bitrate) > 0.75
                            && ((float)newsize * 1024 * mo.Duration.TotalMilliseconds / curr / mo.FileInfo.Length) > 0.8)
                        {
                            Result = EncodingResult.MayProduceLargerFile;
                        }
                    }
                }));

            }

            if (!ffmpeg_process.WaitForExit(500))
            {
                ffmpeg_process.Kill();
                ffmpeg_process.WaitForExit();
            }
            int exitcode = ffmpeg_process.ExitCode;
            ffmpeg_process.Dispose();
            ffmpeg_process = null;

            if (exitcode == 0 && Result == EncodingResult.OK)
            {
                mo.NewSize = (new FileInfo(outfilename)).Length;
                mo.NewBitrate = bitrate;
                if (mo.NewSize > 0 && checkBoxDeleteWhenDone.Checked
                    && ((double)(new FileInfo(outfilename).Length) / (new FileInfo(mo.Filename).Length)) < 0.65)
                {
                    DeleteFile(mo.Filename, 5);
                }
            }
            else if (exitcode != 0 && Result != EncodingResult.OK)
                Result = EncodingResult.UnknownError;

            if (Result != EncodingResult.OK)
                DeleteFile(outfilename, 5);

            textBoxLog.Invoke(new Action(() =>
            {
                toolStripProgressBar1.Value = 0;
                toolStripStatusLabelConsole.Text = "";
                textBoxLog.Text += exitcode.ToString() + "\r\n";
            }));

            return Result;
        }

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
            public FileInfo FileInfo;
            public long NewSize = 0;
            public int NewBitrate = 0;

        }

        string[] StreamStringSplit(string s)
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

        MediaObject GetMediaObject(string file, MediaObject mo)
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

        string scaling;
        private async void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;

            scaling = string.Empty;
            if (comboBoxScale.Text != "(none)")
            {
                string s = comboBoxScale.Text;
                scaling = "scale=\"'-1':'if(gt(ih," + s + ")," + s + ",-1)':flags=lanczos\"";
            }

            await EncodingVideo();
            buttonStart.Enabled = true;
        }

        private async Task EncodingVideo()
        {
            do
            {
                MediaObject mo = null;
                ListViewItem lvi = null;

                for (int i = 0; i < listViewFiles.Items.Count; i++)
                {
                    mo = listViewFiles.Items[i].Tag as MediaObject;
                    if (mo.NewSize == 0)
                        break;
                }
                if (mo == null || mo.NewSize != 0)
                    break;

                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Maximum = (int)mo.Duration.TotalMilliseconds;

                EncodingResult result = await Task<EncodingResult>.Run(new Func<EncodingResult>(() => encodeVideo(mo)));
                if (result != EncodingResult.OK)
                    mo.NewSize = ((int)result + 1);
                //if (mo.NewSize < 0)
                //    continue;

                // find the MO and set new size
                lvi = null;
                for (int i = 0; i < listViewFiles.Items.Count; i++)
                {
                    if (listViewFiles.Items[i].Tag != mo)
                        continue;
                    lvi = listViewFiles.Items[i];
                    break;
                }
                if (lvi == null)
                    continue;
                lvi.SubItems[7].Text = mo.NewSize.ToString("N0");
                lvi.SubItems[9].Text = mo.NewBitrate.ToString("N0");
            } while (true);


        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ffmpeg_process != null)
            {
                ffmpeg_process.Kill();
                ffmpeg_process.Dispose();
            }
        }

        private void listViewFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private async void listViewFiles_DragDrop(object sender, DragEventArgs e)
        {
            buttonStart.Enabled = false;
            listViewFiles.AllowDrop = false;
            List<string> FileList = new List<string>();
            List<string> DirList = new List<string>();
            foreach (string f in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (Directory.Exists(f))
                    DirList.Add(f);
                else if (File.Exists(f))
                    FileList.Add(f);
            }
            await LoadFiles(FileList.ToArray());

            foreach (string d in DirList)
            {
                toolStripStatusLabelConsole.Text = d;
                await LoadDir(d);
            }


            toolStripStatusLabelConsole.Text = "Load files done.";
            listViewFiles.AllowDrop = true;
            buttonStart.Enabled = true;
        }

        private async Task LoadDir(string dir)
        {
            await LoadFiles(Directory.GetFiles(dir));
            foreach (string d in Directory.GetDirectories(dir))
            {
                toolStripStatusLabelConsole.Text = d;
                await LoadDir(d);
            }
        }

        private async Task LoadFiles(string[] FileList)
        {
            // toolStripProgressBar1.Maximum += FileList.Length;
            foreach (string f in FileList)
            {
                // toolStripProgressBar1.Value++;

                if (!VideoExt.Contains(Path.GetExtension(f)))
                    continue;

                MediaObject mo = new MediaObject();
                mo.suffix = textBoxSuffix.Text;
                mo.codec = comboBoxCodec.Text;
                mo.CRF = trackBarCRF.Value;
                mo.outdir = textBoxOutdir.Text;
                mo.FileInfo = new FileInfo(f);
                mo.NewSize = 0;

                toolStripStatusLabelConsole.Text = Path.GetFileName(f);
                mo = await Task.Run(new Func<MediaObject>(() => GetMediaObject(f, mo)));
                if (mo == null)
                    continue;

                if (mo.Bitrate < trackBarBitrate.Value || mo.Video.StartsWith("hevc"))
                    continue;

                ListViewItem lvi = new ListViewItem(Path.GetFileName(f));
                lvi.Tag = mo;
                lvi.SubItems.Add(Path.GetDirectoryName(f));
                lvi.SubItems.Add(mo.Bitrate.ToString());
                lvi.SubItems.Add(mo.Duration.ToString());
                lvi.SubItems.Add(mo.Video);
                lvi.SubItems.Add(mo.Audio);
                lvi.SubItems.Add(mo.FileInfo.Length.ToString("N0"));
                lvi.SubItems.Add(""); // new size
                lvi.SubItems.Add(mo.Resolution); // resolution
                lvi.SubItems.Add(""); // new bitrate
                lvi.SubItems.Add(mo.CRF.ToString()); // parameters
                listViewFiles.Items.Add(lvi);
            }
        }

        private void listViewFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listViewFiles.SelectedItems.Count == 0)
                    return;
                foreach (var item in listViewFiles.SelectedItems)
                    listViewFiles.Items.Remove(item as ListViewItem);
            }
        }

        class ListViewItemComparer : IComparer
        {
            private int col;
            private bool reverse;

            public ListViewItemComparer(int column, bool rev)
            {
                col = column;
                reverse = rev;
            }

            public int Compare(object x, object y)
            {
                MediaObject mox = (x as ListViewItem).Tag as MediaObject;
                MediaObject moy = (y as ListViewItem).Tag as MediaObject;

                int result = -1;
                switch (col)
                {
                    case 2: // bitrate
                        result = mox.Bitrate.CompareTo(moy.Bitrate);
                        break;
                    case 3: // duration
                        if (mox.Duration < moy.Duration)
                            result = -1;
                        else if (mox.Duration == moy.Duration)
                            result = 0;
                        else
                            result = 1;
                        break;
                    case 6:
                        result = mox.FileInfo.Length.CompareTo(moy.FileInfo.Length);
                        break;
                    case 7:
                        result = mox.NewSize.CompareTo(moy.NewSize);
                        break;
                    case 8: // resolution
                        for (int i = 0; i < 2; i++)
                        {
                            int vx = int.Parse(mox.Resolution.Split('x')[i]);
                            int vy = int.Parse(moy.Resolution.Split('x')[i]);
                            result = vx.CompareTo(vy);
                            if (result != 0)
                                break;
                        }
                        break;
                    default:
                        result = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
                        break;
                }

                if (reverse)
                    result = result * -1;
                return result;

            }
        }

        int SortCol = -1;
        private void listViewFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (SortCol != e.Column)
            {
                listViewFiles.Sorting = SortOrder.Ascending;
                listViewFiles.ListViewItemSorter = new ListViewItemComparer(e.Column, false);
                SortCol = e.Column;
            }
            else
            {
                if (listViewFiles.Sorting == SortOrder.Ascending)
                    listViewFiles.Sorting = SortOrder.Descending;
                else
                    listViewFiles.Sorting = SortOrder.Ascending;

                listViewFiles.ListViewItemSorter = new ListViewItemComparer(e.Column, listViewFiles.Sorting == SortOrder.Descending);
            }

        }
    }
}
