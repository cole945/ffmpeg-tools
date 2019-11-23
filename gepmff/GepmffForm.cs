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

        FFmpegHelper ffmpegHelper;
        public GepmffForm()
        {
            InitializeComponent();

            comboBoxCodec.SelectedIndex = 0;

            onChangeSetting(comboBoxCodec.Text, trackBarCRF.Value);
            ffmpegHelper = new FFmpegHelper();
            this.Text = ffmpegHelper.getFFmpegPath();
            if (this.Text == string.Empty)
            {
                MessageBox.Show("FFmpeg now found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ffmpegHelper.ProgressUpdate += FFmpegHelper_ProgressUpdate;
            ffmpegHelper.Completed += FFmpegHelper_Completed;
        }

        void onChangeSetting(string codec, int crf)
        {
            textBoxSuffix.Text = "-" + codec + "-" + crf.ToString();
        }

        private void trackBarCRF_Scroll(object sender, EventArgs e)
        {
            onChangeSetting(comboBoxCodec.Text, trackBarCRF.Value);
            toolTip1.SetToolTip(sender as Control, (sender as TrackBar).Value.ToString());
        }

        private void trackBarBitrate_Scroll(object sender, EventArgs e)
        {
            onChangeSetting(comboBoxCodec.Text, trackBarBitrate.Value);
            toolTip1.SetToolTip(sender as Control, (sender as TrackBar).Value.ToString());
        }

        private void comboBoxCodec_SelectedIndexChanged(object sender, EventArgs e)
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
                FFmpegHelper.MediaObject mo = null;
                mo = lvi.Tag as FFmpegHelper.MediaObject;
                if (mo.Status != FFmpegHelper.Status.New)
                    continue;

                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Maximum = (int)mo.Duration.TotalMilliseconds;
                Task.Factory.StartNew(() => ffmpegHelper.EncodeVideo(mo));
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

            var mo = e.MO;

            // find the MO and set new size

            foreach (ListViewItem lvi in listViewFiles.Items)
            {
                if (lvi.Tag != mo)
                    continue;

                toolStripProgressBar1.Value = 0;
                toolStripStatusLabelConsole.Text = "";
                lvi.SubItems[columnHeaderNewSize.Index].Text = mo.NewSize.ToString("N0");
                lvi.SubItems[columnHeaderNewBitrate.Index].Text = mo.NewBitrate.ToString("N0");
                lvi.SubItems[columnHeaderStatus.Index].Text = Enum.GetName(typeof(FFmpegHelper.Status), mo.Status);
            }

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
            sb.Append(" bitrate=").Append(e.Info.bitrate);
            sb.Append(" size=").Append(e.Info.size);
            sb.Append(" time=").Append(e.Info.time.ToString(@"hh\:mm\:ss\.ff"));
            toolStripStatusLabelConsole.Text = sb.ToString() + "  " + Path.GetFileName(e.MO.Filename);

            {
                int curr = (int)e.Info.time.TotalMilliseconds;
                if (curr > toolStripProgressBar1.Maximum)
                    toolStripProgressBar1.Maximum = curr;
                toolStripProgressBar1.Value = curr;
                // if we have encoding 10min have it seem the bitrate is too high, cancel
                if ((curr / 1000 / 60) > 5
                    && ((float)e.Info.bitrate / e.MO.Bitrate) > 0.75
                    && ((float)e.Info.size * 1024 * e.MO.Duration.TotalMilliseconds / curr / e.MO.FileInfo.Length) > 0.8)
                {
                    e.MO.Status = FFmpegHelper.Status.ErrorMayProduceLargerFile;
                    ffmpegHelper.Kill();
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
                toolStripStatusLabelConsole.Text = Path.GetFileName(f);
                var mo = await Task.Run(new Func<FFmpegHelper.MediaObject>(() => ffmpegHelper.GetMediaObject(f)));
                if (mo == null)
                    continue;

                int Scale;
                if (comboBoxScale.Text != "(none)" && int.TryParse(comboBoxScale.Text, out Scale))
                    mo.scale = Scale;
                else
                    mo.scale = -1;

                mo.suffix = textBoxSuffix.Text;
                mo.codec = comboBoxCodec.Text;
                if (radioButtonCRF.Checked)
                    mo.CRF = trackBarCRF.Value;
                else if (radioButtonBitrate.Checked)
                    mo.AvgBitrate = trackBarBitrate.Value;
                mo.outdir = textBoxOutdir.Text;
                mo.FileInfo = new FileInfo(f);
                mo.NewSize = 0;

                if (mo.Video.StartsWith("hevc"))  /* || mo.Bitrate < trackBarBitrate.Value*/
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
                lvi.SubItems.Add(mo.Q.ToString()); // parameters
                lvi.SubItems.Add(""); // status
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
                FFmpegHelper.MediaObject mox = (x as ListViewItem).Tag as FFmpegHelper.MediaObject;
                FFmpegHelper.MediaObject moy = (y as ListViewItem).Tag as FFmpegHelper.MediaObject;

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
