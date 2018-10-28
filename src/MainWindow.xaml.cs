using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MySetRubyTag {

    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        #region Constructor
        private TextBox[] _rubyText;
        private TextBlock[] _charText;
        private Dictionary<string, SaveData> _rubyData = null;
        //        private string saveDataPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\ruby.dic";
        private string saveDataPath = Properties.Settings.Default.SettingFile;

        public MainWindow() {
            InitializeComponent();

            RubyModel rubyModel = new RubyModel();
            this.DataContext = rubyModel;

            this._rubyText = new TextBox[] { this.cRuby0, this.cRuby1, this.cRuby2, this.cRuby3, this.cRuby4, this.cRuby5, this.cRuby6, this.cRuby7, this.cRuby8, this.cRuby9 };
            this._charText = new TextBlock[] { this.cChar0, this.cChar1, this.cChar2, this.cChar3, this.cChar4, this.cChar5, this.cChar6, this.cChar7, this.cChar8, this.cChar9 };

            if (System.IO.File.Exists(saveDataPath)) {
                this._rubyData = AppData.LoadFromXml<string, SaveData>(saveDataPath);
                if (null == this._rubyData) {
                    this._rubyData = new Dictionary<string, SaveData>();
                }
            } else {
                this._rubyData = new Dictionary<string, SaveData>();
            }

        }
        #endregion

        #region Control Event

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None) {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None) {
                    if (e.Key == Key.C) {
                        this.cCopy_Click(null, null);
                    } else if (e.Key == Key.R) {
                        this.SplitRuby2();
                    }
                } else {
                    if (e.Key == Key.S) {
                        this.cSave_Click(this.cSave, null);
                    } else if (e.Key == Key.R) {
                        this.SplitRuby();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cSrcText_TextChanged(object sender, TextChangedEventArgs e) {
            this.SetDefault();
            if (0 < this.cRuby0.Text.Length) {
                Clipboard.SetText(this.CreateRubyTag());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cRuby_TextChanged(object sender, TextChangedEventArgs e) {
            this.ShowPreview();
        }


        /// <summary>
        /// save button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name=""></param>
        private void cSave_Click(Object sender, RoutedEventArgs args) {
            SaveData saveData;
            if (this._rubyData.ContainsKey(this.cSrcText.Text)) {
                saveData = this._rubyData[this.cSrcText.Text];
            } else {
                saveData = new SaveData();
                saveData.Ruby = new String[10];
                saveData.Char = new String[10];
            }
            for (int i = 0; i < this._rubyText.Length; i++) {
                saveData.Ruby[i] = this._rubyText[i].Text;
                saveData.Char[i] = this._charText[i].Text;
            }

            if (this._rubyData.ContainsKey(this.cSrcText.Text)) {
            } else {
                this._rubyData.Add(this.cSrcText.Text, saveData);
            }
            AppData.SaveToXml<string, SaveData>(saveDataPath, this._rubyData);
        }

        /// <summary>
        /// copy button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name=""></param>
        private void cCopy_Click(Object sender, RoutedEventArgs args) {
            Clipboard.SetText(this.CreateRubyTag());
        }
        #endregion

        #region Private Method
        private void SetDefault() {
            if (this._rubyData.ContainsKey(this.cSrcText.Text)) {
                SaveData data = this._rubyData[this.cSrcText.Text];
                for (int i = 0; i < data.Char.Length; i++) {
                    this._charText[i].Text = data.Char[i];
                    this._rubyText[i].Text = data.Ruby[i];
                }
                this.ShowPreview();
            } else {
                foreach (TextBox textBox in this._rubyText) {
                    textBox.Text = "";
                }
                foreach (TextBlock textBlock in this._charText) {
                    textBlock.Text = "";
                }
                this.cPreview.NavigateToString("&nbsp;");

                string text = this.cSrcText.Text;
                if (10 < text.Length) {
                    text = text.Substring(0, 10);
                }
                for (int i = 0; i < text.Length; i++) {
                    this._charText[i].Text = text.Substring(i, 1);
                }
            }
        }

        /// <summary>
        /// show preview
        /// </summary>
        private void ShowPreview() {
            //            this.cPreview.Navigate("<html><body>" + this.CreateRubyTag()  + "</body></html>");
            StringBuilder tag = new StringBuilder();
            tag.Append("<html>");
            tag.Append("<head>");
            tag.Append("<meta charset=\"UTF-8\"/>");
            tag.Append("<style>");
            tag.Append("*{font-size:12pt;}");
            tag.Append("rt{font-size:11pt;color:gray;}");
            tag.Append("</style>");
            tag.Append("</head>");
            tag.Append("<body>");
            tag.Append(this.CreateRubyTag());
            tag.Append("</body>");
            tag.Append("</html>");
            //            this.cPreview.NavigateToString("<html><head><meta charset=\"UTF-8\"/><style>*{font-size:16pt;}</style></head><body>" + this.CreateRubyTag() + "</body></html>");
            this.cPreview.NavigateToString(tag.ToString());

        }

        /// <summary>
        /// create ruby tag
        /// </summary>
        /// <returns></returns>
        private string CreateRubyTag() {
            StringBuilder tag = new StringBuilder();

            tag.Append("<ruby>");
            for (int i = 0; i < this._charText.Length; i++) {
                if (0 == this._charText[i].Text.Length) {
                    break;
                }
                tag.Append(this._charText[i].Text);
                if (0 < this._rubyText[i].Text.Length) {
                    tag.Append("<rt>" + this._rubyText[i].Text + "</rt>");
                }
            }
            tag.Append("</ruby>");


            return tag.ToString();
        }

        private void SplitRuby() {
            string srcText = this.cSrcText.Text;
            int srcLen = srcText.Length;
            string ruby = this.cRuby0.Text;
            if (ruby.Length < srcLen) {
                ruby = ruby + "　　　　　　　　　　";
                ruby = ruby.Substring(0, srcLen);
            }

            int[] rubyLen = new int[srcLen];
            int index = 0;
            for (int i = 0; i < ruby.Length; i++) {
                rubyLen[index]++;
                index++;
                if (srcLen <= index) {
                    index = 0;
                }
            }

            int lastPos = 0;
            for (int i = 0; i < rubyLen.Length; i++) {
                this._rubyText[i].Text = ruby.Substring(lastPos, rubyLen[i]);
                lastPos += rubyLen[i];
            }
            this.cCopy_Click(null, null);
        }


        private void SplitRuby2() {
            string baseText = this.cSrcText.Text;
            if(baseText.StartsWith("｜")) {
                baseText = baseText.Substring(1);
            }
            int pos = baseText.IndexOf("（");
            if (0<= pos) {
                this.cSrcText.Text = baseText.Substring(0, pos);
                string ruby = baseText.Substring(pos + 1);
                this._rubyText[0].Text = ruby.Substring(0, ruby.Length - 1);
                this.SplitRuby();
            }

        }
        #endregion

    }
}
