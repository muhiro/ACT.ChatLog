using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace ACT.ChatLog
{
    public partial class PluginBase : UserControl, IActPluginV1
    {
        Overlay formOverlay = null;

        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\ChatLogPlugin.config.xml");
        SettingsSerializer xmlSettings;
        string TextLogName;


        private List<ChatLogArgs> ChatLogArgsList = new List<ChatLogArgs>()
        {
            { new ChatLogArgs("000A", "SAY  ", Color.White, true) },
            { new ChatLogArgs("000B", "SHOUT", Color.Orange, true) },
            { new ChatLogArgs("000C", "TELL ", Color.Pink, true) },
            { new ChatLogArgs("000D", "TELL ", Color.Pink, true) },
            { new ChatLogArgs("000E", "PT   ", Color.LightSteelBlue, true) },
            { new ChatLogArgs("000F", "ALLIA", Color.DarkOrange, true) },
            { new ChatLogArgs("001E", "YELL ", Color.Yellow, true) },
            { new ChatLogArgs("0010", "LS1  ", Color.LightGreen, true) },
            { new ChatLogArgs("0011", "LS2  ", Color.LightGreen, true) },
            { new ChatLogArgs("0012", "LS3  ", Color.LightGreen, true) },
            { new ChatLogArgs("0013", "LS4  ", Color.LightGreen, true) },
            { new ChatLogArgs("0014", "LS5  ", Color.LightGreen, true) },
            { new ChatLogArgs("0015", "LS6  ", Color.LightGreen, true) },
            { new ChatLogArgs("0016", "LS7  ", Color.LightGreen, true) },
            { new ChatLogArgs("0017", "LS8  ", Color.LightGreen, true) },
            { new ChatLogArgs("0018", "FC   ", Color.LightSteelBlue, true) },
            { new ChatLogArgs("001C", "EMOTE", Color.White, true) }, //カスタムエモート
            { new ChatLogArgs("001D", "EMOTE", Color.White, true) },
            { new ChatLogArgs("0025", "CWLS1", Color.LightGreen, true) },
            { new ChatLogArgs("0065", "CWLS2", Color.LightGreen, true) },
            { new ChatLogArgs("0066", "CWLS3", Color.LightGreen, true) },
            { new ChatLogArgs("0067", "CWLS4", Color.LightGreen, true) },
            { new ChatLogArgs("0068", "CWLS5", Color.LightGreen, true) },
            { new ChatLogArgs("0069", "CWLS6", Color.LightGreen, true) },
            { new ChatLogArgs("006A", "CWLS7", Color.LightGreen, true) },
            { new ChatLogArgs("006B", "CWLS8", Color.LightGreen, true) },
            { new ChatLogArgs("2245", "FC   ", Color.White, true) }, //FCのシステム情報とか
            { new ChatLogArgs("2246", "SYS  ", Color.White, true) }  //DICEとかIN、OUTとか
        };


        public PluginBase()
        {
            InitializeComponent();


            foreach (ChatLogArgs chatlog in ChatLogArgsList)
            {
                FlowLayoutPanel flowPanelLogType = new FlowLayoutPanel();
                CheckBox checkboxLogType = new CheckBox();
                PictureBox pictureLogColor = new PictureBox();

                flowPanelLogType.FlowDirection = FlowDirection.LeftToRight;
                flowPanelLogType.AutoSize = true;
                flowPanelLogType.Padding = new Padding(15, 0, 0, 0);
                flowPanelLogType.Margin = new Padding(0, 0, 0, 0);


                checkboxLogType.Name = "checkbox" + chatlog.Key;
                checkboxLogType.Text = chatlog.Initials;
                checkboxLogType.Width = 70;
                checkboxLogType.Padding = new Padding(0, 0, 0, 0);
                checkboxLogType.Margin = new Padding(0, 0, 0, 0);
                checkboxLogType.Checked = chatlog.Checked;
                checkboxLogType.CheckStateChanged += new System.EventHandler(checkBox_CheckStateChanged);


                pictureLogColor.Name = "picture" + chatlog.Key;
                pictureLogColor.BackColor = Color.White;
                pictureLogColor.Padding = new Padding(0, 0, 0, 0);
                pictureLogColor.Margin = new Padding(0, 0, 0, 0);
                pictureLogColor.Size = new Size(20, 20);
                pictureLogColor.BackColor = chatlog.Color;
                pictureLogColor.Click += new System.EventHandler(pictureBox_Click);

                flowPanelLogType.Controls.Add(checkboxLogType);
                flowPanelLogType.Controls.Add(pictureLogColor);

                flowLayoutPanel1.Controls.Add(flowPanelLogType);
            }
        }

        public void DeInitPlugin()
        {
            ActGlobals.oFormActMain.OnLogLineRead -= OnLogLineRead;
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginScreenSpace.Controls.Add(this);
            this.Dock = DockStyle.Fill;

            xmlSettings = new SettingsSerializer(this);
            LoadSettings();

            ActGlobals.oFormActMain.OnLogLineRead += OnLogLineRead;

            TextLogName = $"{DateTime.Now:yyyy_MM_dd_HH.mm.ss}.txt";
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = textSaveDirectory.Text,
                FileName = "Folder Selection",
                Filter = "Folder|.",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textSaveDirectory.Text = Path.GetDirectoryName(ofd.FileName);
            }
            ofd.Dispose();
            SaveSettings();
        }

        private void checkboxOverlay_CheckStateChanged(object sender, EventArgs e)
        {
            if (((CheckBox)(sender)).CheckState == CheckState.Checked)
            {
                //LoadSettingsよりこっちが先に動く
                if (formOverlay == null)
                {
                    formOverlay = new Overlay();
                    if (!(textXpos1.Text.Equals(String.Empty)))
                    {
                        var position = new Point(int.Parse(textXpos1.Text), int.Parse(textYpos1.Text));
                        formOverlay.Location = position;
                    }
                    if (!(textHsize1.Text.Equals(String.Empty)))
                    {
                        formOverlay.Size = new Size(int.Parse(textWsize1.Text), int.Parse(textHsize1.Text));
                    }
                    formOverlay.TopMost = checkBoxFront.Checked;

                    formOverlay.ResizeEnd += new EventHandler(formOverlay_ResizeEnd);
                    formOverlay.SetChatLogArgsList(this.ChatLogArgsList);
                    formOverlay.Show();
                }
            }
            else
            {
                if (formOverlay != null)
                {
                    formOverlay.Close();
                    formOverlay = null;
                }
            }
            SaveSettings();
        }

        private void checkBoxFront_CheckStateChanged(object sender, EventArgs e)
        {
            if (formOverlay != null)
            {
                if (((CheckBox)(sender)).CheckState == CheckState.Checked)
                {
                    formOverlay.TopMost = true;
                }
                else
                {
                    formOverlay.TopMost = false;
                }
            }
            SaveSettings();
        }


        private void formOverlay_ResizeEnd(object sender, EventArgs e)
        {
            textXpos1.Text = formOverlay.Left.ToString();
            textYpos1.Text = formOverlay.Top.ToString();

            textWsize1.Text = formOverlay.Width.ToString();
            textHsize1.Text = (formOverlay.Height - SystemInformation.CaptionHeight).ToString();
            SaveSettings();
        }

        private void OnLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            if (isImport) return;

            if ( (logInfo.logLine.IndexOf("ChatLog") != -1)
                && (logInfo.detectedType == 0) )
            {
                string[] logType = logInfo.logLine.Split(' ');
                string chatLogType = logType[2].Substring(3, 4);

                LogLineReadEventArgs logReadInfo = new LogLineReadEventArgs();
                logReadInfo.LogEvent = logInfo;
                logReadInfo.ChatLogType = chatLogType;
                string logLine = logInfo.logLine;

                ChatLogArgs result = ChatLogArgsList.Find(x => x.Key == logReadInfo.ChatLogType);
                if ((result != null) && (result.Checked == false)) { return; }

                MatchCollection matches = Regex.Matches(logLine, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
                foreach (Match match in matches)
                {
                    logLine = Regex.Replace(logLine, match.Value, " " + match.Value + " ");
                }

                if (result != null)
                {
                    logReadInfo.ChatLogLine = "[" + result.Initials + "] " + logLine;
                }
                else
                {
                    Debug.WriteLine(logLine);
                    return;
                }

                formOverlay.OnLogLineReadp(this, logReadInfo);

                if (this.textSaveDirectory.Text.Equals(string.Empty)) { return; }
                try
                {
                    StreamWriter sw = new StreamWriter(this.textSaveDirectory.Text + "\\" + TextLogName, true, Encoding.GetEncoding("UTF-8"));
                    try
                    {
                        sw.WriteLine(logReadInfo.ChatLogLine);
                    }
                    finally
                    {
                        sw.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex);
                }
            }
        }



        void LoadSettings()
        {
            xmlSettings.AddControlSetting(textXpos1.Name, textXpos1);
            xmlSettings.AddControlSetting(textYpos1.Name, textYpos1);
            xmlSettings.AddControlSetting(textWsize1.Name, textWsize1);
            xmlSettings.AddControlSetting(textHsize1.Name, textHsize1);
            xmlSettings.AddControlSetting(textSaveDirectory.Name, textSaveDirectory);
            xmlSettings.AddControlSetting(checkboxOverlay.Name, checkboxOverlay);
            xmlSettings.AddControlSetting(checkBoxFront.Name, checkBoxFront);

            foreach (ChatLogArgs chatlog in ChatLogArgsList)
            {
                xmlSettings.AddControlSetting("checkbox" + chatlog.Key, this.flowLayoutPanel1.Controls.Find("checkbox" + chatlog.Key, true)[0]);
                xmlSettings.AddControlSetting("picture" + chatlog.Key, this.flowLayoutPanel1.Controls.Find("picture" + chatlog.Key, true)[0]);
            }

            if (File.Exists(settingsFile))
            {
                FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlTextReader xReader = new XmlTextReader(fs);

                try
                {
                    while (xReader.Read())
                    {
                        if (xReader.NodeType == XmlNodeType.Element)
                        {
                            if (xReader.LocalName == "SettingsSerializer")
                            {
                                xmlSettings.ImportFromXml(xReader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex);
                }
                xReader.Close();
            }

            foreach (ChatLogArgs chatlog in ChatLogArgsList)
            {
                PictureBox pictureArgs = (PictureBox)this.flowLayoutPanel1.Controls.Find("picture" + chatlog.Key, true)[0];
                CheckBox checkboxArgs = (CheckBox)this.flowLayoutPanel1.Controls.Find("checkbox" + chatlog.Key, true)[0];

                this.ChatLogArgsList.Find(x => x.Key == chatlog.Key).Color = pictureArgs.BackColor;
                this.ChatLogArgsList.Find(x => x.Key == chatlog.Key).Checked = checkboxArgs.Checked;
            }
        }
        void SaveSettings()
        {
            FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8);
            xWriter.Formatting = Formatting.Indented;
            xWriter.Indentation = 1;
            xWriter.IndentChar = '\t';
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");    // <Config>
            xWriter.WriteStartElement("SettingsSerializer");    // <Config><SettingsSerializer>
            xmlSettings.ExportToXml(xWriter);   // Fill the SettingsSerializer XML
            xWriter.WriteEndElement();  // </SettingsSerializer>
            xWriter.WriteEndElement();  // </Config>
            xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
            xWriter.Flush();    // Flush the file buffer to disk
            xWriter.Close();
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                ((PictureBox)sender).BackColor = colorDialog1.Color;
                this.ChatLogArgsList.Find(x => x.Key == ((PictureBox)sender).Name.Replace("picture","")).Color = colorDialog1.Color;
                SaveSettings();
            }
        }

        private void checkBox_CheckStateChanged(object sender, EventArgs e)
        {
            this.ChatLogArgsList.Find(x => x.Key == ((CheckBox)sender).Name.Replace("checkbox", "")).Checked = ((CheckBox)sender).Checked;
            SaveSettings();
        }

    }
}
