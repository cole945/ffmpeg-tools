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

            // find the MO and set new size
            if (e.Result == FFmpegHelper.Status.Done)
            {
            }

            foreach (ListViewItem lvi in listViewFiles.Items)
            {
                if (lvi.Tag != e.EO)
                    continue;

                toolStripProgressBar1.Value = 0;
                toolStripStatusLabelConsole.Text = "";
                lvi.SubItems[columnHeaderNewSize.Index].Text = eo.NewSize.ToString("N0");
                lvi.SubItems[columnHeaderNewBitrate.Index].Text = eo.NewBitrate.ToString("N0");
                lvi.SubItems[columnHeaderStatus.Index].Text = Enum.GetName(typeof(FFmpegHelper.Status), eo.Status);
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
            toolStripStatusLabelConsole.Text = sb.ToString() + "  " + e.EO.MO.FileInfo.Name;

            {
                int curr = (int)e.Info.time.TotalMilliseconds;
                toolStripProgressBar1.Maximum = (int)e.EO.MO.Duration.TotalMilliseconds;
                if (curr >= 0 && curr <= toolStripProgressBar1.Maximum)
                    toolStripProgressBar1.Value = curr;
                // if we have encoding 10min have it seem the bitrate is too high, cancel
                if (curr > 0 && (curr / 1000 / 60) > 5
                    && ((float)e.Info.bitrate / e.EO.MO.Bitrate) > 0.75
                    && ((float)e.Info.size * 1024 * e.EO.MO.Duration.TotalMilliseconds / curr / e.EO.MO.FileInfo.Length) > 0.8)
                {
                    e.EO.Status = FFmpegHelper.Status.ErrorMayProduceLargerFile;
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
            buttonStart.Enabled = true;
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

        void applyParameters(FFmpegHelper.EncodeObject eo, ListViewItem lvi)
        {
            int Scale;
            if (comboBoxScale.Text != "(none)" && int.TryParse(comboBoxScale.Text, out Scale))
                eo.scale = Scale;
            else
                eo.scale = -1;

            eo.suffix = textBoxSuffix.Text;
            eo.codec = comboBoxCodec.Text;
            if (radioButtonCRF.Checked)
                eo.CRF = (int)numericUpDownCRF.Value;
            else if (radioButtonBitrate.Checked)
                eo.AvgBitrate = (int)numericUpDownBitrate.Value;
            eo.outdir = textBoxOutdir.Text;
            eo.NewSize = 0;
            eo.Status = FFmpegHelper.Status.New;

            lvi.SubItems[columnHeaderParameters.Index] = new ListViewItem.ListViewSubItem(lvi, eo.Q.ToString());
            lvi.SubItems[columnHeaderStatus.Index].Text = Enum.GetName(typeof(FFmpegHelper.Status), eo.Status);
        }

        private async Task LoadFiles(string[] FileList)
        {
            // toolStripProgressBar1.Maximum += FileList.Length;
            foreach (string f in FileList)
            {
                // toolStripProgressBar1.Value++;
                toolStripStatusLabelConsole.Text = Path.GetFileName(f);
                var mo = await Task.Run(new Func<FFmpegHelper.MediaObject>(() => ffmpegHelper.GetMediaObject(f)));
                if (mo == null)
                    continue;
                if (mo.Video.StartsWith("hevc")) // FIXME
                    continue;

                FFmpegHelper.EncodeObject eo = new FFmpegHelper.EncodeObject();
                eo.MO = mo;
                ListViewItem lvi = new ListViewItem(Path.GetFileName(f));
                lvi.Tag = eo;
                for (int i = 0;i < listViewFiles.Columns.Count; i++)
                    lvi.SubItems.Add("");
                 
                applyParameters(eo, lvi);

                lvi.SubItems[columnHeaderFolder.Index] = new ListViewItem.ListViewSubItem(lvi, Path.GetDirectoryName(f));
                lvi.SubItems[columnHeaderBitrate.Index] = new ListViewItem.ListViewSubItem(lvi, mo.Bitrate.ToString());
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
            private int col;
            private bool reverse;

            public ListViewItemComparer(int column, bool rev)
            {
                col = column;
                reverse = rev;
            }

            public int Compare(object x, object y)
            {
                FFmpegHelper.EncodeObject eox = (x as ListViewItem).Tag as FFmpegHelper.EncodeObject;
                FFmpegHelper.EncodeObject eoy = (y as ListViewItem).Tag as FFmpegHelper.EncodeObject;

                int result = -1;
                    switch (col)
                {
                    case 2: // bitrate
                        result = eox.MO.Bitrate.CompareTo(eoy.MO.Bitrate);
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

        private void OpenFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = null;
            if (listViewFiles.SelectedItems.Count > 0)
                lvi = listViewFiles.SelectedItems[0];
            if (lvi == null)
                return;

            var eo = lvi.Tag as FFmpegHelper.EncodeObject;
            if (File.Exists(eo.MO.FileInfo.FullName))
                Process.Start("explorer.exe", "/select, " + eo.MO.FileInfo.FullName);
            else
                Process.Start("explorer.exe", eo.MO.FileInfo.DirectoryName);
        }

        private void ApplyNewParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listViewFiles.SelectedItems)
            {
                var eo = lvi.Tag as FFmpegHelper.EncodeObject;
                applyParameters(eo, lvi);

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
    }
}
