using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Folder_Locker;

namespace Folder_Lock_Byte_Getter
{
    public partial class Form1 : Form
    {
        int next = 1;
        public const int BYTE_LEN = 1;
        public const int INT32_LEN = 4;
        public const int SHORT16_LEN = 2;
        public Form1()
        {
            InitializeComponent();
        }

        public void WriteByte(ref byte[] _bytes, ref int i, byte bt)
        {
            // 將byte寫入byte陣列
            _bytes[i] = bt;
            // 體長增加
            i += BYTE_LEN;
        }


        // 寫布林型
        public void WriteBool(ref byte[] _bytes, ref int i, bool flag)
        {
            // bool型實際是發送一個byte的值,判斷是true或false
            byte b = (byte)'1';
            if (!flag)
                b = (byte)'0';
            _bytes[i] = b;
            i += BYTE_LEN;
        }

        // 寫整型
        public void WriteInt(ref byte[] _bytes, ref int i, int number)
        {
            byte[] bs = System.BitConverter.GetBytes(number);
            bs.CopyTo(_bytes, i);
            i += INT32_LEN;
        }

        // 寫短整型
        public void WriteUShort(ref byte[] _bytes, ref int i, ushort number)
        {
            byte[] bs = System.BitConverter.GetBytes(number);
            bs.CopyTo(_bytes, i);
            i += SHORT16_LEN;
        }

        // 寫字串
        public void WriteString(ref byte[] _bytes, ref int i, string str)
        {
            ushort len = (ushort)System.Text.Encoding.UTF8.GetByteCount(str);
            this.WriteUShort(ref _bytes, ref i, len);
            System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, _bytes, i);
            i += len;
        }

        public void ReadByte(byte[] _bytes, ref int i, out byte bt)
        {
            bt = 0;
            bt = _bytes[i];
            i += BYTE_LEN;
        }

        // 讀 bool
        public void ReadBool(byte[] _bytes, ref int i, out bool flag)
        {
            flag = false;
            try
            {
                byte bt = _bytes[i];
                if (bt == (byte)'1')
                    flag = true;
                else
                    flag = false;
            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (IndexOutOfRangeException)
            {

            }
            i += BYTE_LEN;
        }

        // 讀 int
        public void ReadInt(byte[] _bytes, ref int i, out int number)
        {
            number = 0;
            try
            {
                number = System.BitConverter.ToInt32(_bytes, i);
            }
            catch (ArgumentOutOfRangeException)
            {

            }
            i += INT32_LEN;
        }

        // 讀 ushort
        public void ReadUShort(byte[] _bytes, ref int i, out ushort number)
        {
            number = 0;
            try
            {
                number = System.BitConverter.ToUInt16(_bytes, i);
            }
            catch (ArgumentOutOfRangeException)
            {

            }
            i += SHORT16_LEN;
        }

        // 讀取一個字串
        public void ReadString(byte[] _bytes, ref int i, out string str)
        {
            str = "";
            ushort len = 0;
            ReadUShort(_bytes, ref i, out len);
            try
            {
                str = Encoding.UTF8.GetString(_bytes, i, (int)len);
            }
            catch (ArgumentOutOfRangeException)
            {

            }
            i += len;
        }

        public static string SerializeToStream(object UnserializeObj)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, UnserializeObj);
            byte[] a = stream.ToArray();
            string hexString = string.Empty;
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < a.Length; i++)
            {
                str.Append(a[i].ToString("X2"));
            }
            hexString = str.ToString();
            return hexString;
        }

        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }

        public static object DeserializeFromStream(string a)
        {
            int byteLength = a.Length / 2;
            byte[] bytes = new byte[byteLength];
            string hex;
            int j = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hex = new String(new Char[] { a[j], a[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }
            MemoryStream stream = new MemoryStream(bytes);
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object UnserializeObj = formatter.Deserialize(stream);
            return UnserializeObj;
        }

        public void ReadAll()
        {
            string _ba = File.ReadAllText(textBox1.Text);
            _bytes bytes = (_bytes)DeserializeFromStream(_ba);
            textBox2.Text = bytes._name;
            textBox3.Text = bytes._password;
            textBox4.Text = bytes._turn.ToString();
            checkBox1.Checked = bytes._Lock;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Dat檔|*.dat|所有檔案|*.*";
            openFileDialog1.Title = "選擇你要開啟的檔案";
            openFileDialog1.ShowDialog();
            textBox1.Text = openFileDialog1.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            next--;
            if (next == 1)
            {
                label1.Text = "請輸入檔案位置";
                label1.Location = new System.Drawing.Point(190, 10);
                button2.Visible = false;
                button3.Enabled = true;
                panel1.Visible = true;
                panel2.Visible = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            next++;
            if (next == 2)
            {
                if (textBox1.Text == "")
                {
                    next--;
                    MessageBox.Show("請輸入檔案位置。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    label1.Text = "資料內容";
                    label1.Location = new System.Drawing.Point(210, 10);
                    button2.Visible = true;
                    button3.Enabled = false;
                    panel1.Visible = false;
                    panel2.Visible = true;
                    ReadAll();
                }
            }
            if (next == 3)
            {
                if (textBox2.Text == "" || textBox3.Text == "" || textBox4.Text == "")
                {
                    next--;
                    MessageBox.Show("請輸入資料。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _bytes bytes = new _bytes();
                    bytes._name = textBox2.Text;
                    bytes._password = textBox3.Text;
                    bytes._turn = Convert.ToInt32(textBox4.Text);
                    bytes._Lock = checkBox1.Checked;
                    string _ba = SerializeToStream(bytes);
                    FileInfo f = new FileInfo(textBox1.Text);
                    f.Delete();
                    File.WriteAllText(textBox1.Text, _ba);
                    f.Attributes = FileAttributes.Hidden;
                    ReadAll();
                    checkBox2.Checked = false;
                    next = 2;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ReadAll();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                button3.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
            }
        }
    }
}
