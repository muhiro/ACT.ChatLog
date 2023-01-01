﻿using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ACT.ChatLog
{
    public partial class Overlay : Form
    {
        public event EventHandler<LogLineReadEventArgs> OnLogLineRead;
        private List<ChatLogArgs> ChatLogArgsList = null;
        private int MAX_ROW = 1000;

        public Overlay()
        {
            InitializeComponent();
            this.OnLogLineRead += Overlay_OnLogLineRead;

            this.StartPosition = FormStartPosition.Manual;
            //this.TransparencyKey = this.BackColor;
            this.Opacity = 0.75D;
            this.panel1.BackColor = Color.White;

            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.TopMost = true;

            this.richTextChatLog.BackColor = Color.Black;
            this.richTextChatLog.ForeColor = Color.White;
            this.richTextChatLog.DetectUrls = true;

        }

        public void SetChatLogArgsList(List<ChatLogArgs> argslist)
        {
            this.ChatLogArgsList = argslist;
        }

        const int CS_DROPSHADOW = 0x00020000;
        const int WS_BORDER = 0x00800000;
        protected override CreateParams CreateParams
        {
            get
            {
                //枠だけのフォーム
                CreateParams cp = base.CreateParams;
                if (this.FormBorderStyle != FormBorderStyle.None)
                {
                    cp.Style = cp.Style & (~WS_BORDER);
                }
                return cp;
            }
        }

        public void OnLogLineReadp(object sender, LogLineReadEventArgs args)
        {
            this.OnLogLineRead(sender, args);
        }

        public void Overlay_OnLogLineRead(object sender, LogLineReadEventArgs args)
        {
            if (this.richTextChatLog.Lines.Length > this.MAX_ROW)
            {
                int st_idx = this.richTextChatLog.GetFirstCharIndexFromLine(1);
                int nx_idx = this.richTextChatLog.GetFirstCharIndexFromLine(2);
                this.richTextChatLog.Text = this.richTextChatLog.Text.Remove(st_idx, nx_idx - st_idx);
            }
            var start = this.richTextChatLog.Text.Length;
            var length = args.ChatLogLine.Length;

            Debug.WriteLine(args.ChatLogLine);
            this.richTextChatLog.AppendText(args.ChatLogLine + "\n");
            this.richTextChatLog.Focus();
            this.richTextChatLog.ScrollToCaret();
            this.richTextChatLog.DetectUrls = true;

            try
            {
                // ハイライト化
                var highlighter = new SyntaxHighlighter(this.ChatLogArgsList);
                string highlighterRtf = highlighter.GetRtf(this.richTextChatLog.Text, this.richTextChatLog.Font);
                this.richTextChatLog.Rtf = highlighterRtf;
            }
            finally
            {
                this.richTextChatLog.Select(start, length);
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.richTextChatLog.Clear();
        }

        private void richTextChatLog_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private Point mousePoint;
        /// <summary>
        /// フォーム上のすべてのコントローラーはこれを追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Overlay_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
        }

        /// <summary>
        /// フォーム上のすべてのコントローラーはこれを追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Overlay_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }
        }
    }

    public class LogLineReadEventArgs
    {
        public string ChatLogType { get; set; }
        public string ChatLogLine { get; set; }
        public LogLineEventArgs LogEvent { get; set; }
    }
}
