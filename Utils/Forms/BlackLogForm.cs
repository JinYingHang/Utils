using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.Forms
{
    public partial class BlackLogForm : Form
    {
        private static BlackLogForm _instance;
        private static RichTextBox _richTextBox = new RichTextBox();
        public BlackLogForm() {
            _richTextBox.BackColor = Color.Black;
            _richTextBox.BorderStyle = BorderStyle.None;
            _richTextBox.Dock = DockStyle.Fill;
            _richTextBox.HideSelection = false;
            _richTextBox.Focus();
        }

        public static BlackLogForm Instance {
            get {
                if (_instance == null) {
                    _instance = new BlackLogForm();
                    _instance.Margin = new System.Windows.Forms.Padding(0);
                    _instance.FormBorderStyle = FormBorderStyle.None;
                    _instance.TopLevel = false;
                    _instance.Dock = DockStyle.Fill;
                    _instance.Show();
                    _instance.Controls.Add(_richTextBox);
                }
                return _instance;
            }
        }




        public void WriteLog(string msg, string level = "INFO") {
            try {
             
                Action ac= () =>{
                    switch (level) {
                        case "INFO":
                            _richTextBox.SelectionColor = Color.White;
                            break;
                        case "WARN":
                            _richTextBox.SelectionColor = Color.Yellow;
                            break;
                        case "DEBUG":
                            _richTextBox.SelectionColor = Color.Gray;
                            break;
                        case "ERROR":
                            _richTextBox.SelectionColor = Color.Red;
                            break;
                        default:
                            _richTextBox.SelectionColor = Color.White;
                            break;
                    }
                    _richTextBox.AppendText(msg + "\r\n");
                    _richTextBox.ScrollToCaret();
                    if (_richTextBox.Text.Length > 1024 * 1024) {
                        _richTextBox.Text = "";
                    }
                };
                if (InvokeRequired) {
                    this.Invoke(ac);
                }
                else {
                    ac();
                }
            }
            catch (Exception) {
                throw;
            }
        }
    }
}
