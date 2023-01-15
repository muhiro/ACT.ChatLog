using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace ACT.ChatLog
{
    public partial class PluginBase : UserControl, IActPluginV1
    {
        Overlay formOverlay = null;

#if DEBUG
        string settingsFile = null;
#else //DEBUG
        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\" + Assembly.GetExecutingAssembly().GetName().Name + ".config.xml");
#endif //DEBUG
        SettingsSerializer xmlSettings;
        string TextLogName;


        private List<ChatLogArgs> ChatLogArgsList = new List<ChatLogArgs>()
        {
            { new ChatLogArgs("000A", "SAY   ", Color.White, true) },
            { new ChatLogArgs("000B", "SHOUT ", Color.Orange, true) },
            { new ChatLogArgs("000C", "TELL  ", Color.Pink, true) },
            { new ChatLogArgs("000D", "TELL  ", Color.Pink, true) },
            { new ChatLogArgs("000E", "PT    ", Color.LightSteelBlue, true) },
            { new ChatLogArgs("000F", "ALLIAN", Color.DarkOrange, true) },

            { new ChatLogArgs("0010", "LS1   ", Color.LightGreen, true) },
            { new ChatLogArgs("0011", "LS2   ", Color.LightGreen, true) },
            { new ChatLogArgs("0012", "LS3   ", Color.LightGreen, true) },
            { new ChatLogArgs("0013", "LS4   ", Color.LightGreen, true) },
            { new ChatLogArgs("0014", "LS5   ", Color.LightGreen, true) },
            { new ChatLogArgs("0015", "LS6   ", Color.LightGreen, true) },
            { new ChatLogArgs("0016", "LS7   ", Color.LightGreen, true) },
            { new ChatLogArgs("0017", "LS8   ", Color.LightGreen, true) },

            { new ChatLogArgs("0018", "FC    ", Color.LightSteelBlue, true) },

            { new ChatLogArgs("001B", "BEGINN", Color.LightGreen, true) },

            { new ChatLogArgs("001C", "EMOTEc", Color.White, true) }, //カスタムエモート
            { new ChatLogArgs("001D", "EMOTE ", Color.White, true) },
            { new ChatLogArgs("001E", "YELL  ", Color.Yellow, true) },

            { new ChatLogArgs("0039", "SYS   ", Color.White, true) },
            { new ChatLogArgs("0839", "SYSCon", Color.White, true) }, //コンテンツ開始/終了 リテイナーベンチャー コンテンツ関連
            { new ChatLogArgs("0C39", "ITEMTr", Color.White, true) }, //アイテム取引
            { new ChatLogArgs("083E", "ITEMGe", Color.White, true) }, //アイテムの取得

            { new ChatLogArgs("003D", "NPC   ", Color.White, true) }, //NPC
            { new ChatLogArgs("0044", "NPCBtt", Color.White, true) }, //NPC バトル

            { new ChatLogArgs("0048", "PTRecu", Color.White, true) }, //パーティー募集

            { new ChatLogArgs("0025", "CWLS1 ", Color.LightGreen, true) },

            { new ChatLogArgs("0065", "CWLS2 ", Color.LightGreen, true) },
            { new ChatLogArgs("0066", "CWLS3 ", Color.LightGreen, true) },
            { new ChatLogArgs("0067", "CWLS4 ", Color.LightGreen, true) },
            { new ChatLogArgs("0068", "CWLS5 ", Color.LightGreen, true) },
            { new ChatLogArgs("0069", "CWLS6 ", Color.LightGreen, true) },
            { new ChatLogArgs("006A", "CWLS7 ", Color.LightGreen, true) },
            { new ChatLogArgs("006B", "CWLS8 ", Color.LightGreen, true) },

            { new ChatLogArgs("0239", "TRADE ", Color.White, true) }, //トレード

            //{ new ChatLogArgs("0843", "SYS   ", Color.White, true) }, //コンテンツアクションの開始/中断 取得
            //{ new ChatLogArgs("082B", "SYS   ", Color.White, true) }, //アクションの開始/中断
            //{ new ChatLogArgs("08AB", "SYS   ", Color.White, true) }, //アクションの開始/中断 構え
            //{ new ChatLogArgs("0AA9", "SYS   ", Color.White, true) }, //ダメージを与えた
            //{ new ChatLogArgs("28A9", "SYS   ", Color.White, true) }, //ダメージを受けた

            //{ new ChatLogArgs("08AE", "SYS   ", Color.White, true) }, //効果
            //{ new ChatLogArgs("08B0", "SYS   ", Color.White, true) }, //効果切れ

            { new ChatLogArgs("0840", "COMPLE", Color.White, true) }, //コンプリートタイム
            { new ChatLogArgs("0B3A", "CONTEN", Color.White, true) }, //討伐情報


            //{ new ChatLogArgs("0BB9", "SYS   ", Color.White, true) }, //クエスト受注

            //{ new ChatLogArgs("2040", "SYS   ", Color.White, true) }, //成長/修得情報

            { new ChatLogArgs("0245", "FCINF ", Color.White, true) }, //FCアナウンス
            { new ChatLogArgs("2245", "FCSYS ", Color.White, true) }, //FCシステム情報とか
            { new ChatLogArgs("2246", "SYSIO ", Color.White, true) }  //DICEとかIN、OUTとか
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
                checkboxLogType.Width = 78;
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
            formOverlay.Close();
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginScreenSpace.Controls.Add(this);
            this.Dock = DockStyle.Fill;

            xmlSettings = new SettingsSerializer(this);
            LoadSettings();

            TextLogName = $"{DateTime.Now:yyyy_MM_dd_HH.mm.ss}.txt";

#if !DEBUG
            ActGlobals.oFormActMain.OnLogLineRead += OnLogLineRead;
#endif //DEBUG
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

                    formOverlay.Move += new EventHandler(formOverlay_Move);
                    formOverlay.Resize += new EventHandler(formOverlay_Resize);
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

        private void formOverlay_Move(object sender, EventArgs e)
        {
            textXpos1.Text = formOverlay.Left.ToString();
            textYpos1.Text = formOverlay.Top.ToString();

            textWsize1.Text = formOverlay.Width.ToString();
            textHsize1.Text = formOverlay.Height.ToString();
            SaveSettings();
        }

        private void formOverlay_Resize(object sender, EventArgs e)
        {
            textXpos1.Text = formOverlay.Left.ToString();
            textYpos1.Text = formOverlay.Top.ToString();

            textWsize1.Text = formOverlay.Width.ToString();
            textHsize1.Text = formOverlay.Height.ToString();
            SaveSettings();
        }

        public void OnLogLineReadp(bool isImport, LogLineEventArgs logInfo)
        {
            this.OnLogLineRead(isImport, logInfo);
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

                if (this.textSaveDirectory.Text.Equals(string.Empty) || !Directory.Exists(this.textSaveDirectory.Text)) { return; }
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
            if (File.Exists(settingsFile))
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
