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
using MediaInfoLib;

namespace gepmff
{
    public partial class GepmffForm : Form
    {

        FFmpegHelper ffmpegHelper;

        List<string> VideoExt = new List<string>();
        public GepmffForm()
        {
            InitializeComponent();

            VideoExt.Add(".mp4");
            VideoExt.Add(".mkv");
            VideoExt.Add(".wmv");
            VideoExt.Add(".avi");
            VideoExt.Add(".m4v");
            VideoExt.Add(".mov");
            VideoExt.Add(".mpg");
            VideoExt.Add(".mpeg");
            VideoExt.Add(".rmvb");
            VideoExt.Add(".flv");
            VideoExt.Add(".ts");

            comboBoxCodec.SelectedIndex = 0;

            onChangeSetting(comboBoxCodec.Text, numericUpDownCRF.Value.ToString());
            ffmpegHelper = new FFmpegHelper();
            this.Text = ffmpegHelper.getFFmpegPath();
            if (this.Text == string.Empty)
            {
                MessageBox.Show("FFmpeg now found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ffmpegHelper.ProgressUpdate += FFmpegHelper_ProgressUpdate;
            ffmpegHelper.Completed += FFmpegHelper_Completed;
        }

        void onChangeSetting(string codec, string suffix)
        {
            textBoxSuffix.Text = "-" + codec + "-" + suffix;
        }

        private void comboBoxCodec_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioButtonCRF.Checked)
                onChangeSetting(comboBoxCodec.Text, numericUpDownCRF.Value.ToString());
            else if (radioButtonBitrate.Checked)
                onChangeSetting(comboBoxCodec.Text, numericUpDownBitrate.Value.ToString());
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

        void UpdateButtons(bool isStart)
        {
            if (isStart)
            {
                buttonStart.Enabled = false;
            }
            else
            {
                buttonStart.Enabled = true;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            UpdateButtons(true);
            EncodeNextVideo();
        }

        private void EncodeNextVideo()
        {
            foreach (ListViewItem lvi in listViewFiles.Items)
            {
                FFmpegHelper.EncodeObject eo = null;
                eo = lvi.Tag as FFmpegHelper.EncodeObject;
                if (eo.Status != FFmpegHelper.Status.New)
                    continue;

                toolStripProgressBar1.Value = 0;
                Task.Factory.StartNew(() => ffmpegHelper.EncodeVideo(eo));
                return;
            }

            UpdateButtons(false);
        }
        private void FFmpegHelper_Completed(object sender, FFmpegHelper.CompletedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => FFmpegHelper_Completed(sender, e)));
                return;
            }

            var eo = e.EO;

            string filename = Path.GetFileNameWithoutExtension(eo.MO.FileInfo.FullName);
            string ext = eo.MO.FileInfo.Extension;
            if (eo.outdir == string.Empty)
                eo.outdir = eo.MO.FileInfo.DirectoryName;
            string outfilename = Path.Combine(eo.outdir, filename) + eo.suffix + ".mp4";

            // find the MO and set new size
            if (e.Result == FFmpegHelper.Status.Done && eo.codec != "vmaf")
            {
                if (checkBoxReplaceOldFile.Checked)
                {
                    int i = 0;
                    string mvtofilename = string.Empty;
                    do
                    {
                        string suffix = DateTime.Now.ToString("yyyyMMddhhmmss");
                        if (i > 0)
                            suffix = suffix + "." + i.ToString();
                        mvtofilename = eo.MO.FileInfo.FullName + "." + suffix + ".bak";
                        File.Move(eo.MO.FileInfo.FullName, mvtofilename);
                        break;
                    } while (true);

                    string newname = Path.Combine(eo.MO.FileInfo.DirectoryName, filename + ".mp4");
                    File.Move(outfilename, newname);
                }
            }
            else if (e.Result != FFmpegHelper.Status.ErrorFileAlreadyExists)
            {
                ffmpegHelper.Kill();
                try
                {
                    if (File.Exists(outfilename))
                        File.Delete(outfilename);
                }
                catch (Exception)
                {
                }
            }

            foreach (ListViewItem lvi in listViewFiles.Items)
            {
                if (lvi.Tag != e.EO)
                    continue;

                toolStripProgressBar1.Value = 0;
                toolStripStatusLabelConsole.Text = "";
                if (eo.codec != "vmaf")
                {
                    lvi.SubItems[columnHeaderNewSize.Index].Text = eo.NewSize.ToString("N0");
                    lvi.SubItems[columnHeaderNewBitrate.Index].Text = eo.NewBitrate.ToString("N0");
                    lvi.SubItems[columnHeaderStatus.Index].Text = Enum.GetName(typeof(FFmpegHelper.Status), eo.Status);
                }
                else
                {
                    lvi.SubItems[columnHeaderStatus.Index].Text = eo.vmaf_score;
                }
            }

            toolStripStatusLabelConsole.Text = "";
            toolStripProgressBar1.Value = 0;

            EncodeNextVideo();
        }

        private void FFmpegHelper_ProgressUpdate(object sender, FFmpegHelper.ProgressInfoEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => FFmpegHelper_ProgressUpdate(sender, e)));
                return;
            }

            StringBuilder sb = new StringBuilder();
            // string[] groups = new string[] { "frame", "fps", "size", "time", "bitrate", "speed" };
            sb.Append("frame=").Append(e.Info.frame);
            sb.Append(" fsp=").Append(e.Info.fps.ToString("0.0"));
            sb.Append(" q=").Append(e.Info.q.ToString("0.00"));
            sb.Append(" bitrate=").Append(e.Info.bitrate);
            sb.Append(" size=").Append(e.Info.size);
            sb.Append(" time=").Append(e.Info.time.ToString(@"hh\:mm\:ss\.ff"));
            if (e.EO.pts_warning > 0)
                sb.Append(" PTS warning=" + e.EO.pts_warning.ToString());
            if (e.EO.dts_warning > 0)
                sb.Append(" DTS warning=" + e.EO.dts_warning.ToString());
            toolStripStatusLabelConsole.Text = sb.ToString();
            toolStripStatusLabelFilename.Text = e.EO.MO.FileInfo.Name;

            {
                int curr = (int)e.Info.time.TotalMilliseconds;
                toolStripProgressBar1.Maximum = (int)e.EO.MO.Duration.TotalMilliseconds;
                if (curr >= 0 && curr <= toolStripProgressBar1.Maximum)
                    toolStripProgressBar1.Value = curr;
                // if we have encoding 10min have it seem the bitrate is too high, cancel
                if (checkBoxSkipIfNotSmaller.Checked
                    && curr > 0 && (curr / 1000 / 60) > 5
                    && (e.Info.bitrate >= e.EO.MO.Bitrate)
                    && ((float)e.Info.size * 1024 * (e.EO.MO.Duration.TotalMilliseconds / curr)) > e.EO.MO.FileInfo.Length)
                {
                    e.EO.Status = FFmpegHelper.Status.ErrorMayProduceLargerFile;
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ffmpegHelper.Kill();
        }

        private void listViewFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private async void listViewFiles_DragDrop(object sender, DragEventArgs e)
        {
            bool startStatus = buttonStart.Enabled;
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
            // toolStripProgressBar1.Maximum = 0;
            // toolStripProgressBar1.Value = 0;
            await LoadFiles(FileList.ToArray());

            foreach (string d in DirList)
            {
                toolStripStatusLabelConsole.Text = d;
                await LoadDir(d);
            }

            toolStripStatusLabelConsole.Text = "Load files done.";
            listViewFiles.AllowDrop = true;
            buttonStart.Enabled = startStatus;
        }

        private async Task LoadDir(string dir)
        {
            await LoadFiles(Directory.GetFiles(dir));

            string[] DirList = Directory.GetDirectories(dir);
            foreach (string d in DirList)
            {
                toolStripStatusLabelConsole.Text = d;
                await LoadDir(d);
            }
        }

        string getParamString(FFmpegHelper.EncodeObject eo)
        {
            StringBuilder paramStr = new StringBuilder();

            paramStr.Append(eo.codec);
            if (eo.codec == "copy")
            {
            }
            else if (eo.codec == "vmaf")
            {
                if (eo.bf > 0)
                    paramStr.Append(", n_subsample=" + eo.bf.ToString());
                if (eo.fps != String.Empty)
                    paramStr.Append(", framerate=" + eo.fps);
            }
            else
            {
                paramStr.Append(", Q=").Append(eo.Q);
                if (eo.scale != -1)
                    paramStr.Append(", scale=").Append(eo.scale);
                if (eo.ss != TimeSpan.MaxValue)
                    paramStr.Append(", ss=").Append(eo.ss.ToString(@"hh\:mm\:ss\.fff"));
                if (eo.to != TimeSpan.MaxValue)
                    paramStr.Append(", to=").Append(eo.to.ToString(@"hh\:mm\:ss\.fff"));
                if (eo.b_ref_mode != 0)
                    paramStr.Append(", b_ref_mode=").Append(eo.b_ref_mode);
                if (eo.bf >= 0)
                    paramStr.Append(", bf=").Append(eo.bf);
                if (eo.hwdec)
                    paramStr.Append(", hwdec");
                if (eo.yadif == 1)
                    paramStr.Append(", yadif").Append(eo.fps);
                else if (eo.yadif == 1)
                    paramStr.Append(", yadif_cuda").Append(eo.fps);
                if (eo.fps != string.Empty)
                    paramStr.Append(", fps=").Append(eo.fps);
                if (eo.spatial_aq)
                    paramStr.Append(", spatial_aq");
                if (eo.temporal_aq)
                    paramStr.Append(", temporal_aq");
                // if (eo.multipass >= 0)
                //    paramStr.Append(", multipass=").Append(eo.multipass);
                if (eo.acodec != string.Empty && eo.acodec != "copy")
                    paramStr.Append(", audio=").Append(eo.acodec);
            }

            return paramStr.ToString();
        }

        void applyParameters(FFmpegHelper.EncodeObject eo)
        {
            int Scale;

            eo.reset();

            if (comboBoxScale.Text != "(none)" && int.TryParse(comboBoxScale.Text, out Scale))
                eo.scale = Scale;
            else
                eo.scale = -1;

            eo.suffix = textBoxSuffix.Text;
            eo.codec = comboBoxCodec.Text;

            if (eo.codec != "vmaf")
            {
                if (radioButtonCRF.Checked)
                    eo.CRF = (int)numericUpDownCRF.Value;
                else if (radioButtonBitrate.Checked)
                    eo.AvgBitrate = (int)numericUpDownBitrate.Value;
                eo.outdir = textBoxOutdir.Text;
                eo.NewSize = 0;
                TimeSpan ss;

                eo.ss = TimeSpan.MaxValue;
                if (TimeSpan.TryParse(seekerTextBox.Text, out ss))
                    eo.ss = ss;
                eo.to = TimeSpan.MaxValue;
                if (TimeSpan.TryParse(seekerToTextBox.Text, out ss))
                    eo.to = ss;

                eo.hwdec = checkHwDec.Checked;
                eo.b_ref_mode = comboBRefMode.SelectedIndex;
                if (!int.TryParse(maskedBF.Text.Trim(), out eo.bf))
                    eo.bf = -1;
                eo.fps = maskedTextBoxFPS.Text.Trim().Trim('.');

                eo.vsync = comboBoxVsync.SelectedText.Split(',')[0];
                if (eo.vsync == "-1")
                    eo.vsync = string.Empty;

                eo.yadif = comboBoxYADIF.SelectedIndex;

                eo.temporal_aq = checkBoxTemporalAQ.Checked;
                eo.spatial_aq = checkBoxSpatialAQ.Checked;
                eo.multipass = multipasComboBox.SelectedIndex;
                eo.acodec = comboBoxAudio.Text;
                eo.abitrate = textBoxAudioBitrate.Text;
            }
            else if(eo.codec == "vmaf")
            {
                // try get base file
                string basefile = eo.MO.FileInfo.FullName;

                eo.vmaffile = string.Empty;
                eo.fps = "";
                double dummy = 0.0;
                if (double.TryParse(maskedTextBoxFPS.Text, out dummy))
                    eo.fps = maskedTextBoxFPS.Text.Trim().Trim('.');
                if (!int.TryParse(maskedBF.Text.Trim(), out eo.bf))
                    eo.bf = -1;

                while (true)
                {
                    int i = basefile.LastIndexOf('-');
                    if (i < 0)
                        break;

                    basefile = basefile.Substring(0, i);
                    foreach (string ve in VideoExt)
                    {
                        if (File.Exists(basefile + ve))
                        {
                            basefile = basefile + ve;
                            break;
                        }
                    }


                    if (File.Exists(basefile))
                    {
                        eo.vmaffile = basefile;
                        eo.outfile = eo.MO.FileInfo.FullName + ".log";
                        eo.suffix = Path.GetFileNameWithoutExtension(eo.MO.FileInfo.FullName.Substring(i + 1));
                    }

                }

                if (eo.vmaffile == string.Empty)
                {
                    basefile = eo.MO.FileInfo.FullName;
                    eo.vmaffile = basefile;
                    eo.outfile = eo.MO.FileInfo.FullName + ".log";
                    eo.suffix = "base";
                }

                
            }

            eo.Status = FFmpegHelper.Status.New;
        }

        private async Task LoadFromList(string filename)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();
                    if (line.StartsWith("#"))
                        continue;
                    
                    string[] fields = Regex.Split(line, @"\s+");
                    if (fields.Length < 2)
                        continue;
                    list[fields[0].ToLower()] = fields[1];
                }
            }

            List<string> dirs = new List<string>();
            dirs.Add(Path.GetDirectoryName(filename));
            while (dirs.Count > 0)
            {
                string dir;
                try
                {
                    dir = dirs[0];
                    dirs.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    break;
                }

                try
                {
                    foreach (string d in Directory.GetDirectories(dir))
                        dirs.Add(d);
                }
                catch (Exception)
                {
                    // may not be acessible
                    continue;

                }
                foreach (string f in Directory.GetFiles(dir))
                {
                    string stem = Path.GetFileName(f);
                    string ext = Path.GetExtension(f).ToLower();
                    if (ext == ".jpg" || ext == ".png")
                        continue;

                    string[] ids = Regex.Split(stem, @"\s+");
                    ids[0] = ids[0].ToLower();
                    if (!list.ContainsKey(ids[0]))
                        continue;
                    seekerTextBox.Text = list[ids[0]];

                    await LoadFiles(new string[] { f });

                    // list.Remove(ids[0]);
                }
            }
        }


        Regex timeSpanRegex = new Regex(@"(?<FROM>(\d{2}\.)?\d{2}\.\d{2}\.\d{3})(.(?<TO>(\d{2}\.)?\d{2}\.\d{2}\.\d{3}))?", RegexOptions.Compiled);
        void loadParametersFromFilename(FFmpegHelper.EncodeObject eo, string filename)
        {
            Match m = timeSpanRegex.Match(filename);

            if (!m.Success)
                return;

            string f = m.Groups["FROM"].Value.Replace(".", ":");
            string t = m.Groups["TO"].Value.Replace(".", ":");

            if (f.Length >= 4)
                f = f.Substring(0, f.Length - 4) + "." + f.Substring(f.Length - 3);

            if (t.Length >= 4)
                t = t.Substring(0, t.Length - 4) + "." + t.Substring(t.Length - 3);

            if (f.Length == 9)
                f = "00:" + f;
            if (t.Length == 9)
                t = "00:" + t;

            TimeSpan ss;
            if (TimeSpan.TryParse(f, out ss))
                eo.ss = ss;
            else
                return;

            eo.to = TimeSpan.MaxValue;
            if (t != string.Empty && TimeSpan.TryParse(t, out ss))
                eo.to = ss;
            if (eo.ss != TimeSpan.MinValue || eo.to != TimeSpan.MinValue)
                eo.acodec = "libfdk_aac";
        }

        private async Task LoadFiles(string[] FileList)
        {
            seekerTextBox.Text = "";
            seekerToTextBox.Text = "";

            // toolStripProgressBar1.Maximum += FileList.Length;
            foreach (string f in FileList)
            {
                string ext = Path.GetExtension(f);
                if (ext.ToLower() == ".txt")
                {
                    await LoadFromList(f);
                    continue;
                }
                // toolStripProgressBar1.Value++;
                toolStripStatusLabelConsole.Text = Path.GetFileName(f);

                var mo = await Task.Run(new Func<FFmpegHelper.MediaObject>(() => ffmpegHelper.GetMediaObject(f)));
                if (mo == null)
                    continue;

                TimeSpan ss = TimeSpan.MaxValue;
                if (TimeSpan.TryParse(seekerTextBox.Text, out ss))
                {
                    if (mo.Video.StartsWith("hevc") && comboBoxCodec.SelectedText.StartsWith("hevc")) // FIXME
                        continue;
                }

                FFmpegHelper.EncodeObject eo = new FFmpegHelper.EncodeObject();
                eo.MO = mo;
                ListViewItem lvi = new ListViewItem(Path.GetFileName(f));
                lvi.Tag = eo;
                for (int i = 0;i < listViewFiles.Columns.Count; i++)
                    lvi.SubItems.Add("");
                 
                applyParameters(eo);

                loadParametersFromFilename(eo, Path.GetFileName(f));


                lvi.SubItems[columnHeaderParameters.Index] = new ListViewItem.ListViewSubItem(lvi, getParamString(eo));
                lvi.SubItems[columnHeaderStatus.Index].Text = Enum.GetName(typeof(FFmpegHelper.Status), eo.Status);

                lvi.SubItems[columnHeaderFolder.Index] = new ListViewItem.ListViewSubItem(lvi, Path.GetDirectoryName(f));
                lvi.SubItems[columnHeaderBitrate.Index] = new ListViewItem.ListViewSubItem(lvi, mo.Bitrate.ToString());
                lvi.SubItems[columnHeaderFPS.Index] = new ListViewItem.ListViewSubItem(lvi, mo.FPS);
                lvi.SubItems[columnHeaderDuration.Index] = new ListViewItem.ListViewSubItem(lvi, mo.Duration.ToString());
                lvi.SubItems[columnHeaderVideo.Index] = new ListViewItem.ListViewSubItem(lvi, mo.Video);
                lvi.SubItems[columnHeaderAudio.Index] = new ListViewItem.ListViewSubItem(lvi, mo.Audio);
                lvi.SubItems[columnHeaderSize.Index] = new ListViewItem.ListViewSubItem(lvi, mo.FileInfo.Length.ToString("N0"));
                lvi.SubItems[columnHeaderResolution.Index] = new ListViewItem.ListViewSubItem(lvi, mo.Resolution);
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
            public int col;
            public bool reverse;
            public GepmffForm form;

            public ListViewItemComparer(GepmffForm form, int column, bool rev)
            {
                col = column;
                reverse = rev;
                this.form = form;
            }

            public int Compare(object x, object y)
            {
                FFmpegHelper.EncodeObject eox = (x as ListViewItem).Tag as FFmpegHelper.EncodeObject;
                FFmpegHelper.EncodeObject eoy = (y as ListViewItem).Tag as FFmpegHelper.EncodeObject;

                int result = -1;
                if (col == form.columnHeaderBitrate.Index ||
                    col == form.columnHeaderNewBitrate.Index ||
                    col == form.columnHeaderSize.Index ||
                    col == form.columnHeaderNewSize.Index)
                {
                    long lhs = 0;
                    long rhs = 0;

                    long.TryParse(((ListViewItem)x).SubItems[col].Text.Replace(",", ""), out lhs);
                    long.TryParse(((ListViewItem)y).SubItems[col].Text.Replace(",", ""), out rhs);
                    result = lhs.CompareTo(rhs);
                }
                else if (col == form.columnHeaderFPS.Index)
                {
                    double lhs = 0;
                    double rhs = 0;

                    double.TryParse(((ListViewItem)x).SubItems[col].Text, out lhs);
                    double.TryParse(((ListViewItem)y).SubItems[col].Text, out rhs);
                    result = lhs.CompareTo(rhs);
                }
                else if (col == form.columnHeaderDuration.Index)
                {
                    result = eox.MO.Duration.CompareTo(eoy.MO.Duration);
                }
                else
                {
                    // text
                    result = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
                }
#if false
                switch (col)
                {
                    case form.: // bitrate
                        break;
                    case 3: // duration
                        if (eox.MO.Duration < eoy.MO.Duration)
                            result = -1;
                        else if (eox.MO.Duration == eoy.MO.Duration)
                            result = 0;
                        else
                            result = 1;
                        break;
                    case 6:
                        result = eox.MO.FileInfo.Length.CompareTo(eoy.MO.FileInfo.Length);
                        break;
                    case 7:
                        result = eox.NewSize.CompareTo(eoy.NewSize);
                        break;
                    case 8: // resolution
                        for (int i = 0; i < 2; i++)
                        {
                            int vx = int.Parse(eox.MO.Resolution.Split('x')[i]);
                            int vy = int.Parse(eoy.MO.Resolution.Split('x')[i]);
                            result = vx.CompareTo(vy);
                            if (result != 0)
                                break;
                        }
                        break;
                    default:
                        result = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
                        break;
                }
#endif

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
                listViewFiles.ListViewItemSorter = new ListViewItemComparer(this, e.Column, false);
                SortCol = e.Column;
            }
            else
            {
                if (listViewFiles.Sorting == SortOrder.Ascending)
                    listViewFiles.Sorting = SortOrder.Descending;
                else
                    listViewFiles.Sorting = SortOrder.Ascending;

                listViewFiles.ListViewItemSorter = new ListViewItemComparer(this, e.Column, listViewFiles.Sorting == SortOrder.Descending);
            }

        }

        private void OpenFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = null;
            if (listViewFiles.SelectedItems.Count > 0)
                lvi = listViewFiles.SelectedItems[0];
            if (lvi == null)
                return;

            var eo = lvi.Tag as FFmpegHelper.EncodeObject;
            if (File.Exists(eo.MO.FileInfo.FullName))
                Process.Start("explorer.exe", "/select, \"" + eo.MO.FileInfo.FullName + "\"");
            else
                Process.Start("explorer.exe", "\"" + eo.MO.FileInfo.DirectoryName + "\"");
        }

        private void ApplyNewParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listViewFiles.SelectedItems)
            {
                var eo = lvi.Tag as FFmpegHelper.EncodeObject;
                applyParameters(eo);

                lvi.SubItems[columnHeaderParameters.Index] = new ListViewItem.ListViewSubItem(lvi, getParamString(eo));
                lvi.SubItems[columnHeaderStatus.Index].Text = Enum.GetName(typeof(FFmpegHelper.Status), eo.Status);
            }
        }

        private void numericUpDownCRF_ValueChanged(object sender, EventArgs e)
        {
            radioButtonCRF.Checked = true;
            onChangeSetting(comboBoxCodec.Text, numericUpDownCRF.Value.ToString());
        }

        private void numericUpDownBitrate_ValueChanged(object sender, EventArgs e)
        {
            radioButtonBitrate.Checked = true;
            onChangeSetting(comboBoxCodec.Text, numericUpDownBitrate.Value.ToString());
        }

        private void resetStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {

            foreach (ListViewItem lvi in listViewFiles.SelectedItems)
            {
                var eo = lvi.Tag as FFmpegHelper.EncodeObject;
                eo.Status = FFmpegHelper.Status.New;
                // lvi.SubItems[columnHeaderParameters.Index] = new ListViewItem.ListViewSubItem(lvi, paramStr);
                lvi.SubItems[columnHeaderStatus.Index].Text = Enum.GetName(typeof(FFmpegHelper.Status), eo.Status);

            }
        }

        void selectItemByValue(ComboBox cb, string val)
        {
            for (int i = 0; i < cb.Items.Count; i++)
            {
                if (val != cb.Items[i].ToString())
                    continue;
                cb.SelectedIndex = i;
                return;
            }
            cb.SelectedIndex = -1;
        }
        void loadParameters(FFmpegHelper.EncodeObject eo)
        {
            textBoxSuffix.Text = eo.suffix;

            if (eo.QType == FFmpegHelper.EncodeObject.QualityEnum.CRF)
                numericUpDownCRF.Value = eo.Q;
            else if (eo.QType == FFmpegHelper.EncodeObject.QualityEnum.AvgBitrate)
                numericUpDownBitrate.Value = eo.Q;

            radioButtonCRF.Checked = eo.QType == FFmpegHelper.EncodeObject.QualityEnum.CRF;
            radioButtonBitrate.Checked = eo.QType == FFmpegHelper.EncodeObject.QualityEnum.AvgBitrate;

            selectItemByValue(comboBoxCodec, eo.codec);
            selectItemByValue(comboBoxScale, eo.scale.ToString());
            //selectItemByValue(comboBRefMode, eo.b_ref_mode.ToString());
            comboBRefMode.SelectedIndex = eo.b_ref_mode;

            if (eo.ss != TimeSpan.MaxValue)
                seekerTextBox.Text = eo.ss.ToString(@"hh\:mm\:ss\.fff");
            else
                seekerTextBox.Text = "";

            if (eo.to != TimeSpan.MaxValue)
                seekerToTextBox.Text = eo.to.ToString(@"hh\:mm\:ss\.fff");
            else
                seekerToTextBox.Text = "";

            checkHwDec.Checked = eo.hwdec;
            maskedBF.Text = eo.bf.ToString();
            checkBoxTemporalAQ.Checked = eo.temporal_aq;
            checkBoxSpatialAQ.Checked = eo.spatial_aq;
            maskedTextBoxFPS.Text = eo.fps.ToString();

            comboBoxYADIF.SelectedIndex = eo.yadif;

            comboBoxAudio.Text = eo.acodec;
            textBoxAudioBitrate.Text = eo.abitrate;
        }

        private void listViewFiles_DoubleClick(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count != 1)
                return;

            foreach (ListViewItem lvi in listViewFiles.SelectedItems)
            {
                var eo = lvi.Tag as FFmpegHelper.EncodeObject;
                loadParameters(eo);
            }
        }
    }
}
