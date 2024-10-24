using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using static ScreenCapture.BaseLogRecord;
using System.Windows.Media.Media3D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Windows.Threading;


namespace ScreenCapture
{
    #region Config Class
    public class SerialNumber
    {
        [JsonProperty("Parameter1_val")]
        public string Parameter1_val { get; set; }
        [JsonProperty("Parameter2_val")]
        public string Parameter2_val { get; set; }
    }

    public class Model
    {
        [JsonProperty("SerialNumbers")]
        public SerialNumber SerialNumbers { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("Models")]
        public List<Model> Models { get; set; }
    }
    #endregion

    public partial class MainWindow : System.Windows.Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Function
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (System.Windows.MessageBox.Show("請問是否要關閉？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        #region Config
        private SerialNumber SerialNumberClass()
        {
            SerialNumber serialnumber_ = new SerialNumber
            {
                Parameter1_val = Parameter1.Text,
                Parameter2_val = Parameter2.Text
            };
            return serialnumber_;
        }

        private void LoadConfig(int model, int serialnumber, bool encryption = false)
        {
            List<RootObject> Parameter_info = Config.Load(encryption);
            if (Parameter_info != null)
            {
                Parameter1.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Parameter1_val;
                Parameter2.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Parameter2_val;
            }
            else
            {
                // 結構:2個Models、Models下在各2個SerialNumbers
                SerialNumber serialnumber_ = SerialNumberClass();
                List<Model> models = new List<Model>
                {
                    new Model { SerialNumbers = serialnumber_ },
                    new Model { SerialNumbers = serialnumber_ }
                };
                List<RootObject> rootObjects = new List<RootObject>
                {
                    new RootObject { Models = models },
                    new RootObject { Models = models }
                };
                Config.SaveInit(rootObjects, encryption);
            }
        }
       
        private void SaveConfig(int model, int serialnumber, bool encryption = false)
        {
            Config.Save(model, serialnumber, SerialNumberClass(), encryption);
        }
        #endregion

        #region Dispatcher Invoke 
        public string DispatcherGetValue(System.Windows.Controls.TextBox control)
        {
            string content = "";
            this.Dispatcher.Invoke(() =>
            {
                content = control.Text;
            });
            return content;
        }

        public void DispatcherSetValue(string content, System.Windows.Controls.TextBox control)
        {
            this.Dispatcher.Invoke(() =>
            {
                control.Text = content;
            });
        }
        #endregion

        private void StartScreenCapture()
        {
            startTime = DateTime.Now;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);  
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;

            if (elapsedTime.TotalMinutes >= 1) // 如果經過時間超過1分鐘
            {
                timer.Stop();  // 停止定時器
                //System.Windows.MessageBox.Show("已達到1分鐘，停止擷取螢幕。");
            }
            else
            {
                CaptureScreen();  // 繼續擷取螢幕
            }
        }

        private void CaptureScreen()
        {
            try
            {
                // 擷取螢幕畫面
                System.Drawing.Rectangle screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(screenSize.Width, screenSize.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(screenSize.X, screenSize.Y, 0, 0, screenSize.Size);
                        System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;

                        // 繪製滑鼠圖標到擷取的螢幕畫面上
                        System.Windows.Forms.Cursor cursor = System.Windows.Forms.Cursors.Default;
                        System.Drawing.Rectangle cursorBounds = new System.Drawing.Rectangle(cursorPosition, cursor.Size);
                        cursor.Draw(g, cursorBounds);
                    }

                    // 根據當前時間進行命名
                    string fileName = System.IO.Path.Combine(@"E:\DIP Temp\Image Temp", $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                    // 儲存圖片
                    bitmap.Save(fileName, ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"擷取螢幕時發生錯誤: {ex.Message}");
            }
        }
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig(0, 0);
        }
        BaseConfig<RootObject> Config = new BaseConfig<RootObject>();
        private DispatcherTimer timer;
        private DateTime startTime;
        #region Log
        BaseLogRecord Logger = new BaseLogRecord();
        //Logger.WriteLog("儲存參數!", LogLevel.General, richTextBoxGeneral);
        #endregion
        #endregion
        
        #region Main Screen
        private void Main_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as System.Windows.Controls.Button).Name)
            {
                case nameof(Demo):
                    {
                        StartScreenCapture();
                        break;
                    }
            }
        }
        #endregion
        

    }
}
