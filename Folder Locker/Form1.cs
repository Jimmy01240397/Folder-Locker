using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Folder_Locker
{
    public partial class Form1 : Form
    {
        int m;
        int RadioButton1 = 1;
        int next = 1;
        string o;
        public const int BYTE_LEN = 1;
        public const int INT32_LEN = 4;
        public const int SHORT16_LEN = 2;
        public Form1()
        {
            InitializeComponent();
            label1.Text = "建立一個私密資料夾在目標位置。";
            textBox1.Text = System.Environment.CurrentDirectory;
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
            byte bt = _bytes[i];
            if (bt == (byte)'1')
                flag = true;
            else
                flag = false;
            i += BYTE_LEN;
        }

        // 讀 int
        public void ReadInt(byte[] _bytes, ref int i, out int number)
        {
            number = 0;
            number = System.BitConverter.ToInt32(_bytes, i);
            i += INT32_LEN;
        }

        // 讀 ushort
        public void ReadUShort(byte[] _bytes, ref int i, out ushort number)
        {
            number = 0;
            number = System.BitConverter.ToUInt16(_bytes, i);
            i += SHORT16_LEN;
        }

        // 讀取一個字串
        public void ReadString(byte[] _bytes, ref int i, out string str)
        {
            str = "";
            ushort len = 0;
            ReadUShort(_bytes, ref i, out len);
            str = Encoding.UTF8.GetString(_bytes, i, (int)len);
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

        private string MakeNewFolder(string dirPath, int x)
        {
            string a = "";
            for (int i = 0; i < x; i++)
            {
                a = dirPath;
                Directory.CreateDirectory(a);
                if (i == 0)
                {
                    _bytes bytes = new _bytes();
                    bytes._name = Path.GetFileName(a);
                    bytes._password = textBox3.Text;
                    bytes._turn = x;
                    bytes._Lock = false;
                    string _ba = SerializeToStream(bytes);
                    File.WriteAllText(a + @"\abc.dat", _ba);
                    FileInfo fd = new FileInfo(a + @"\abc.dat");
                    fd.Attributes = FileAttributes.Hidden;
                }
                dirPath = dirPath + @"\" + textBox2.Text;
            }
            return a;
        }

        private void FolderLock(string path)
        {
            string y = Path.GetFileName(path);
            string _ba = File.ReadAllText(path + @"\abc.dat");
            _bytes bytes = (_bytes)DeserializeFromStream(_ba);
            bytes._Lock = true;
            _ba = SerializeToStream(bytes);
            FileInfo fi = new FileInfo(path + @"\abc.dat");
            fi.Delete();
            File.WriteAllText(path + @"\abc.dat", _ba);
            fi.Attributes = FileAttributes.Hidden;
            int r = y.Length * 11;
            string t = "此資料夾已被封鎖(" + y;
            while (t.Length < r - 1)
            {
                t += "))";
            }
            t += ")";
            int x = bytes._turn;
            for (int i = 0; i < x - 1; i++)
            {
                path = path + @"\" + y;
            }
            for (int i = 0; i < x; i++)
            {
                try
                {
                    Directory.Move(path, Path.GetDirectoryName(path) + @"\" + t);
                    path = Path.GetDirectoryName(path);
                }
                catch (IOException ex)
                {
                    i--;
                }
            }
        }

        private void FolderOpen(string path)
        {
            string y = Path.GetFileName(path);
            string _ba = File.ReadAllText(path + @"\abc.dat");
            _bytes bytes = (_bytes)DeserializeFromStream(_ba);
            bytes._Lock = false;
            _ba = SerializeToStream(bytes);
            FileInfo fi = new FileInfo(path + @"\abc.dat");
            fi.Delete();
            File.WriteAllText(path + @"\abc.dat", _ba);
            fi.Attributes = FileAttributes.Hidden;
            int x = bytes._turn;
            for (int i = 0; i < x - 1; i++)
            {
                try
                {
                    Directory.Move(path, Path.GetDirectoryName(path) + @"\" + bytes._name);
                    path = Path.GetDirectoryName(path) + @"\" + bytes._name + @"\" + y;
                }
                catch (IOException ex)
                {
                    i--;
                }
            }
            Directory.Move(path, Path.GetDirectoryName(path) + @"\" + bytes._name);
            path = Path.GetDirectoryName(path) + @"\" + bytes._name;
            o = path;
            m = x;
        }
        private bool IsProcessExist(string path)
        {
            string _ba = File.ReadAllText(path + @"\abc.dat");
            _bytes bytes = (_bytes)DeserializeFromStream(_ba);
            try
            {
                Directory.Move(path, Path.GetDirectoryName(path) + @"\此資料夾已被封鎖(" + bytes._name + ")");
            }
            catch (IOException ex)
            {
                return true;
            }
            Directory.Move(Path.GetDirectoryName(path) + @"\此資料夾已被封鎖(" + bytes._name + ")", path);
            return false;
        }
        private int RoundDown(double d)
        {
            int digits = 0;
            if (d == Double.NaN || d == 0)
                return 0;

            string s = "";
            if (d.ToString().IndexOf(".") != -1)
            {
                if (digits == 0)
                    s = d.ToString().Substring(0, d.ToString().IndexOf("."));
                else
                {
                    int length = digits + d.ToString().IndexOf(".") + 1;
                    if (d.ToString().Length < length)
                        s = d.ToString().PadRight(length, '0');
                    else
                        s = d.ToString().Substring(0, digits + d.ToString().IndexOf(".") + 1);
                }
            }
            else
                return Convert.ToInt32(d);
            return Int32.Parse(s);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            next--;
            if (next == 1)
            {
                label2.Text = "請選擇執行的程序";
                textBox1.Text = System.Environment.CurrentDirectory;
                label2.Location = new System.Drawing.Point(220, 10);
                button1.Visible = false;
                panel2.Visible = true;
                panel3.Visible = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            next++;
            if (next == 2)
            {
                button1.Visible = true;
                panel2.Visible = false;
                if (RadioButton1 == 1)
                {
                    label2.Text = "請輸入檔名,目標位置與密碼";
                    label2.Location = new System.Drawing.Point(180, 10);
                    panel3.Visible = true;
                    label5.Visible = true;
                    textBox2.Visible = true;
                    label7.Visible = true;
                    textBox3.Visible = true;
                }
                if (RadioButton1 == 2)
                {
                    label2.Text = "請輸入目標資料夾與密碼";
                    label2.Location = new System.Drawing.Point(185, 10);
                    panel3.Visible = true;
                    label5.Visible = false;
                    textBox2.Visible = false;
                    label7.Visible = true;
                    textBox3.Visible = true;
                }
                if (RadioButton1 == 3)
                {
                    label2.Text = "請輸入目標資料夾";
                    label2.Location = new System.Drawing.Point(220, 10);
                    panel3.Visible = true;
                    label5.Visible = false;
                    textBox2.Visible = false;
                    label7.Visible = false;
                    textBox3.Visible = false;
                }
            }
            if (next == 3)
            {
                if (RadioButton1 == 1)
                {
                    if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
                    {
                        next--;
                        MessageBox.Show("請輸入檔名,目標位置與密碼。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (Path.GetExtension(textBox1.Text) == "")
                        {
                            int r = textBox2.Text.Length * 11;
                            string t = "此資料夾已被封鎖(" + textBox2.Text;
                            while (t.Length < r - 1)
                            {
                                t += "))";
                            }
                            t += ")";
                            FileInfo f = new FileInfo(textBox1.Text + @"\" + textBox2.Text + @"\abc.dat");
                            FileInfo ff = new FileInfo(textBox1.Text + @"\" + t + @"\abc.dat");
                            if (f.Exists || ff.Exists)
                            {
                                next--;
                                MessageBox.Show("資料夾已存在。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBox1.Text = System.Environment.CurrentDirectory;
                            }
                            else
                            {
                                o = MakeNewFolder(textBox1.Text + @"\" + textBox2.Text, RoundDown(54 / textBox2.Text.Length));
                                label8.Text = "資料夾已建立完成，請按完成建開啟您的資料夾並關閉程式。";
                                button3.Text = "完成";
                                panel3.Visible = false;
                                button1.Visible = false;
                                button2.Visible = false;
                                label2.Text = "完成";
                                label2.Location = new System.Drawing.Point(280, 10);
                                panel5.Visible = true;
                            }
                        }
                        else
                        {
                            next--;
                            MessageBox.Show("你不能選擇檔案。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            textBox1.Text = System.Environment.CurrentDirectory;
                        }
                    }
                }
                if (RadioButton1 == 2)
                {
                    if (textBox1.Text == "")
                    {
                        next--;
                        MessageBox.Show("請輸入目標資料夾與密碼。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (Path.GetExtension(textBox1.Text) == "")
                        {
                            FileInfo f = new FileInfo(textBox1.Text + @"\abc.dat");
                            if (f.Exists)
                            {
                                string _ba = File.ReadAllText(textBox1.Text + @"\abc.dat");
                                _bytes bytes = (_bytes)DeserializeFromStream(_ba);
                                if (textBox3.Text == bytes._password)
                                {
                                    if (!bytes._Lock)
                                    {
                                        textBox1.Text = Path.GetDirectoryName(textBox1.Text) + @"\" + bytes._name;
                                        string dd = textBox1.Text;
                                        for (int i = 0; i < bytes._turn - 1; i++)
                                        {
                                            dd = dd + @"\" + bytes._name;
                                        }
                                        o = dd;
                                        m = bytes._turn;
                                        label2.Text = "其他設定";
                                        label2.Location = new System.Drawing.Point(265, 10);
                                        panel3.Visible = false;
                                        button1.Visible = false;
                                        panel4.Visible = true;
                                    }
                                    else
                                    {
                                        FolderOpen(textBox1.Text);
                                        textBox1.Text = Path.GetDirectoryName(textBox1.Text) + @"\" + bytes._name;
                                        m = bytes._turn;
                                        label2.Text = "其他設定";
                                        label2.Location = new System.Drawing.Point(265, 10);
                                        panel3.Visible = false;
                                        button1.Visible = false;
                                        panel4.Visible = true;
                                    }
                                }
                                else
                                {
                                    next--;
                                    MessageBox.Show("密碼錯誤。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            else
                            {
                                next--;
                                MessageBox.Show("此資料夾不是私密資料夾。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBox1.Text = System.Environment.CurrentDirectory;
                            }
                            f = null;
                        }
                        else
                        {
                            next--;
                            MessageBox.Show("你不能選擇檔案，請選擇資料夾。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            textBox1.Text = System.Environment.CurrentDirectory;
                        }
                    }
                }
                if (RadioButton1 == 3)
                {
                    if (textBox1.Text == "")
                    {
                        next--;
                        MessageBox.Show("請輸入目標資料夾。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (Path.GetExtension(textBox1.Text) == "")
                        {
                            FileInfo f = new FileInfo(textBox1.Text + @"\abc.dat");
                            if (f.Exists)
                            {
                                string _ba = File.ReadAllText(textBox1.Text + @"\abc.dat");
                                _bytes bytes = (_bytes)DeserializeFromStream(_ba);
                                if (bytes._Lock)
                                {
                                    next--;
                                    MessageBox.Show("此資料夾已被封鎖。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    textBox1.Text = System.Environment.CurrentDirectory;
                                }
                                else
                                {
                                    if (IsProcessExist(textBox1.Text))
                                    {
                                        next--;
                                        MessageBox.Show("請關閉此資料夾。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        FolderLock(textBox1.Text);
                                        textBox1.Text = Path.GetDirectoryName(textBox1.Text) + @"\" + bytes._name;
                                        label8.Text = "資料夾封鎖完成，請按完成建關閉程式。";
                                        button3.Text = "完成";
                                        panel3.Visible = false;
                                        button1.Visible = false;
                                        button2.Visible = false;
                                        label2.Text = "完成";
                                        label2.Location = new System.Drawing.Point(280, 10);
                                        checkBox1.Checked = true;
                                        panel5.Visible = true;
                                    }
                                }
                            }
                            else
                            {
                                next--;
                                MessageBox.Show("此資料夾不是私密資料夾。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBox1.Text = System.Environment.CurrentDirectory;
                            }
                            f = null;
                        }
                        else
                        {
                            next--;
                            MessageBox.Show("你不能選擇檔案，請選擇資料夾。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            textBox1.Text = System.Environment.CurrentDirectory;
                        }
                    }
                }
            }
            if (next == 4)
            {
                if (RadioButton1 == 2)
                {
                    string w = o;
                    for (int i = 0; i < m - 1; i++)
                    {
                        w = Path.GetDirectoryName(w);
                    }
                    FileInfo ft = new FileInfo(w + @"\abc.dat");
                    string _ba = File.ReadAllText(w + @"\abc.dat");
                    _bytes bytes = (_bytes)DeserializeFromStream(_ba);
                    if (checkBox2.Checked)
                    {
                        if (textBox4.Text == "")
                        {
                            next--;
                            MessageBox.Show("請輸入密碼。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            ft.Delete();
                            _ba = null;
                            bytes._password = textBox4.Text;
                            _ba = SerializeToStream(bytes);
                            File.WriteAllText(textBox1.Text + @"\abc.dat", _ba);
                            ft.Attributes = FileAttributes.Hidden;
                            bytes._password = textBox4.Text;
                        }
                    }
                    if (checkBox3.Checked)
                    {
                        if (textBox5.Text.Length == bytes._name.Length)
                        {
                            if (textBox5.Text == bytes._name)
                            {
                                next--;
                                MessageBox.Show("新名字不能跟舊名字一樣。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                string ww = Path.GetDirectoryName(w);
                                int r = textBox5.Text.Length * 11;
                                string t = "此資料夾已被封鎖(" + textBox5.Text;
                                while (t.Length < r - 1)
                                {
                                    t += "))";
                                }
                                t += ")";
                                FileInfo f = new FileInfo(ww + @"\" + textBox5.Text + @"\abc.dat");
                                FileInfo ff = new FileInfo(ww + @"\" + t + @"\abc.dat");
                                if (f.Exists || ff.Exists)
                                {
                                    next--;
                                    MessageBox.Show("資料夾已存在。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    if (IsProcessExist(w))
                                    {
                                        next--;
                                        MessageBox.Show("請關閉此資料夾。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    else
                                    {
                                        ft.Delete();
                                        string a = bytes._name;
                                        bytes._name = textBox5.Text;
                                        _ba = SerializeToStream(bytes);
                                        File.WriteAllText(textBox1.Text + @"\abc.dat", _ba);
                                        ft.Attributes = FileAttributes.Hidden;
                                        for (int i = 0; i < m - 1; i++)
                                        {
                                            try
                                            {
                                                Directory.Move(w, Path.GetDirectoryName(w) + @"\" + textBox5.Text);
                                                w = Path.GetDirectoryName(w) + @"\" + textBox5.Text + @"\" + a;
                                            }
                                            catch (IOException ex)
                                            {
                                                i--;
                                            }
                                        }
                                        Directory.Move(w, Path.GetDirectoryName(w) + @"\" + textBox5.Text);
                                        w = Path.GetDirectoryName(w) + @"\" + textBox5.Text;
                                        o = w;
                                        label8.Text = "資料夾解鎖完成，請按完成建開啟您的資料夾並關閉程式。";
                                        button3.Text = "完成";
                                        panel4.Visible = false;
                                        button1.Visible = false;
                                        button2.Visible = false;
                                        label2.Text = "完成";
                                        label2.Location = new System.Drawing.Point(280, 10);
                                        panel5.Visible = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            next--;
                            MessageBox.Show("新名字的字數要一樣。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        label8.Text = "資料夾解鎖完成，請按完成建開啟您的資料夾並關閉程式。";
                        button3.Text = "完成";
                        panel4.Visible = false;
                        button1.Visible = false;
                        button2.Visible = false;
                        label2.Text = "完成";
                        label2.Location = new System.Drawing.Point(280, 10);
                        panel5.Visible = true;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (RadioButton1 == 1 || RadioButton1 == 2)
            {
                if (button3.Text == "完成")
                {
                    System.Diagnostics.Process.Start(o);
                }
            }
            if (checkBox1.Checked && button3.Text == "完成")
            {
                next = 1;
                label2.Text = "請選擇執行的程序";
                label2.Location = new System.Drawing.Point(220, 10);
                button3.Text = "取消";
                textBox1.Text = System.Environment.CurrentDirectory;
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                button2.Visible = true;
                panel2.Visible = true;
                panel5.Visible = false;
            }
            else
            {
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            label1.Text = "建立一個私密資料夾在目標位置。";
            RadioButton1 = 1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            label1.Text = "開啟選擇的目標私密資料夾。";
            RadioButton1 = 2;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            label1.Text = "關閉選擇的目標私密資料夾並上鎖。";
            RadioButton1 = 3;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            textBox1.Text = path.SelectedPath;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                textBox4.Enabled = true;
            }
            if (checkBox2.Checked == false)
            {
                textBox4.Enabled = false;
            }
            if (checkBox3.Checked)
            {
                textBox5.Enabled = true;
            }
            if (checkBox3.Checked == false)
            {
                textBox5.Enabled = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                textBox5.Enabled = true;
            }
            if (checkBox3.Checked == false)
            {
                textBox5.Enabled = false;
            }
            if (checkBox2.Checked)
            {
                textBox4.Enabled = true;
            }
            if (checkBox2.Checked == false)
            {
                textBox4.Enabled = false;
            }
        }
    }
}
