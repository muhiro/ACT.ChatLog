using System.Drawing;
using System.Reflection.Emit;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ACT.ChatLog
{
    public class ChatLogArgs
    {
        public ChatLogArgs(string key, string initials, Color color, bool check = false) {
            Key = key;
            Initials = initials;
            Color = color;
            Checked = check;
        }
        public string Key { get; set; }
        public string Initials { get; set; }
        public Color Color { get; set; }
        public bool Checked { get; set; }
    }
}
