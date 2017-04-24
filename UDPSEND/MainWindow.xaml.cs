using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace UDPSEND
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// 几个重要的类定义
        /// </summary>
        private UDPSENDclass recorder = null;
        private UDPCLIRNT Recv = null;
        ThreadCompleteHandler result = null;
        private Thread startrecv = null;
        public MainWindow()
        {
            InitializeComponent();
            SendReady();

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            if (button.Content.Equals("发送"))
            {
                SendReady();
            }else
            {
                RecvReady();
            }
            
        }

        public delegate void ThreadCompleteHandler(int i);

        public void ThreadRunOver(int result)
        {
//            MessageBox.Show("走到这一步了："+result, "提示");
            if (result == 0)
            {
                Recv.Close();
                Recv = null;
//                MessageBox.Show("进if了：" + result, "提示");

                // 

                // 录音设置 

                // 

                MessageBox.Show("一个文件接收完毕","提示");
                this.Dispatcher.Invoke(
                    new Action(
                        delegate
                        {
                            button.Content = "点击再次接收";
                        }));



            }
        }

        private void SendReady()
        {
            // 

            // 发送准备 

            // 

            recorder = new UDPSENDclass();

            string wavfile = null;

            wavfile = "test.wav";

            recorder.SetFileName(wavfile);

            recorder.RecStart();
            if (Recv != null)
                Recv.Close();
            Recv = null;
            if (startrecv != null)
                startrecv.Abort();
            startrecv = null;
            button.Content = "接收";
        }

        private void RecvReady()
        {
            if (recorder != null)
                recorder.RecStop();

            recorder = null;

            result = ThreadRunOver;
            button.Content = "发送";
            Recv = new UDPCLIRNT();
            Recv.SetFileName("RecvTest.wav");
            startrecv = new Thread((ThreadStart)delegate
            {
                result(Recv.recv());//调用result委托

            });
            startrecv.IsBackground = true;
            startrecv.Start();
        }

    }
}
