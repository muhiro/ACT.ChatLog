using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ACT.ChatLog
{
    internal class SyntaxHighlighter
    {
        /// <summary>RTF 生成用コントロール</summary>
        private readonly RichTextBox control = new RichTextBox();
        private List<ChatLogArgs> ChatLogArgsList = null;

        /// <summary>
        /// リソース解放済みフラグを取得します。
        /// </summary>
        public bool IsDisposed { get; private set; }

        public SyntaxHighlighter(List<ChatLogArgs> args)
        {
            this.ChatLogArgsList = args;
        }

        /// <summary>
        /// リソースを解放します。
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを解放します。
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.control.Dispose();
                }
                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// リッチテキスト形式のテキストを取得します。
        /// </summary>
        /// <param name="ritchtext"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        public string GetRtf(string ritchtext, Font font)
        {
            int index = 0, length = 0;
            this.control.Clear();
            this.control.Text = ritchtext;
            if (!string.IsNullOrEmpty(ritchtext))
            {
                string[] textarr = ritchtext.Split('\n');
                foreach (string text in textarr)
                {
                    length = text.Length;
                    // 各パターンに一致する文字色を設定
                    foreach (var syntax in this.ChatLogArgsList)
                    {
                        if (text.IndexOf(syntax.Initials) >= 0)
                        {
                            this.control.Select(index, length);
                            this.control.SelectionColor = syntax.Color;
                            break;
                        }
                        else
                        {
                            this.control.Select(index, length);
                            this.control.SelectionColor = Color.White;
                        }
                    }
                    index += length + 1;
                }
            }
            // フォントを設定
            this.control.SelectAll();
            this.control.SelectionFont = font;

            return this.control.Rtf;
        }
    }
}
