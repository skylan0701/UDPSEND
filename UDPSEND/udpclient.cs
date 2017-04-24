using Microsoft.DirectX.DirectSound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ZLibNet;

namespace UDPSEND
{
    class UDPCLIRNT
    {

        private string mFileName = string.Empty;     // 文件名 

        private FileStream mWaveFile = null;         // 文件流 

        private BinaryWriter mWriter = null;         // 写文件 

        private WaveFormat mWavFormat;                       // 录音的格式 

        private int mSampleCount = 0;            // 接收的样本数目 

        Socket newsock;

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 6001);//定义一网络端点

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);//定义要发送的计算机的地址

        public UDPCLIRNT()
        {


        }
        public int recv()
        {


            // 设定录音格式 

            mWavFormat = CreateWaveFormat();

            CreateSoundFile();

            int recv;

            string str;

            byte[] data = new byte[1024];

            newsock = new Socket(SocketType.Dgram, ProtocolType.Udp);//定义一个Socket

            newsock.Bind(ipep);//Socket与本地的一个终结点相关联

            EndPoint Remote = (EndPoint)sender;

            while (true)
            {
                data = new byte[5000];
                try
                {
                    recv = newsock.ReceiveFrom(data, ref Remote);
                }
                catch /*(Exception e)*/
                {
                    // MessageBox.Show(e.Message, "错误");
                    return 1;
                }




                str = Encoding.Unicode.GetString(data);

                try
                {
                    if (str.Substring(0, 6) == "TheEnd")    //判断文件的传输是否结束
                        break;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "错误");
                }


                data = ZLibCompressor.DeCompress(data);
                mSampleCount += data.Length;
                mWriter.Write(data, 0, data.Length);

            }

            // 回写长度信息 

            mWriter.Seek(4, SeekOrigin.Begin);

            mWriter.Write((int)(mSampleCount + 36));   // 写文件长度 

            mWriter.Seek(40, SeekOrigin.Begin);

            mWriter.Write(mSampleCount);                // 写数据长度 



            mWriter.Close();

            mWaveFile.Close();

            mWriter = null;

            mWaveFile = null;

            return 0;

        }

        /// <summary> 

        /// 创建录音格式,此处使用16bit,16KHz,Mono的录音格式 

        /// </summary> 

        /// <returns>WaveFormat结构体</returns> 

        private WaveFormat CreateWaveFormat()

        {

            WaveFormat format = new WaveFormat();




            format.FormatTag = WaveFormatTag.Pcm;   // PCM 

            format.SamplesPerSecond = 16000;        // 16KHz 

            format.BitsPerSample = 16;              // 16Bit 

            format.Channels = 1;                    // Mono 

            format.BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));

            format.AverageBytesPerSecond = format.BlockAlign * format.SamplesPerSecond;




            return format;

        }



        /// <summary> 

        /// 创建保存的波形文件,并写入必要的文件头. 

        /// </summary> 

        private void CreateSoundFile()

        {

            /************************************************************************** 

         Here is where the file will be created. A 

         wave file is a RIFF file, which has chunks 

         of data that describe what the file contains. 

         A wave RIFF file is put together like this: 


         

         The 12 byte RIFF chunk is constructed like this: 

         Bytes 0 - 3 :  'R' 'I' 'F' 'F' 

         Bytes 4 - 7 :  Length of file, minus the first 8 bytes of the RIFF description. 

                           (4 bytes for "WAVE" + 24 bytes for format chunk length + 

                           8 bytes for data chunk description + actual sample data size.) 

          Bytes 8 - 11: 'W' 'A' 'V' 'E' 


         

          The 24 byte FORMAT chunk is constructed like this: 

          Bytes 0 - 3 : 'f' 'm' 't' ' ' 

          Bytes 4 - 7 : The format chunk length. This is always 16. 

          Bytes 8 - 9 : File padding. Always 1. 

          Bytes 10- 11: Number of channels. Either 1 for mono,  or 2 for stereo. 

          Bytes 12- 15: Sample rate. 

          Bytes 16- 19: Number of bytes per second. 

          Bytes 20- 21: Bytes per sample. 1 for 8 bit mono, 2 for 8 bit stereo or 

                          16 bit mono, 4 for 16 bit stereo. 

          Bytes 22- 23: Number of bits per sample. 


         

          The DATA chunk is constructed like this: 

          Bytes 0 - 3 : 'd' 'a' 't' 'a' 

          Bytes 4 - 7 : Length of data, in bytes. 

          Bytes 8 -…: Actual sample data. 

                    ***************************************************************************/

            // Open up the wave file for writing. 

            mWaveFile = new FileStream(mFileName, FileMode.Create);

            mWriter = new BinaryWriter(mWaveFile);




            // Set up file with RIFF chunk info. 

            char[] ChunkRiff = { 'R', 'I', 'F', 'F' };


            char[] ChunkType = { 'W', 'A', 'V', 'E' };


            char[] ChunkFmt = { 'f', 'm', 't', ' ' };

            char[] ChunkData = { 'd', 'a', 't', 'a' };




            short shPad = 1;                // File padding 

            int nFormatChunkLength = 0x10;  // Format chunk length. 

            int nLength = 0;                // File length, minus first 8 bytes of RIFF description. This will be filled in later. 

            short shBytesPerSample = 0;     // Bytes per sample. 




            // 一个样本点的字节数目 

            if (8 == mWavFormat.BitsPerSample && 1 == mWavFormat.Channels)

                shBytesPerSample = 1;

            else if ((8 == mWavFormat.BitsPerSample && 2 == mWavFormat.Channels) || (16 == mWavFormat.BitsPerSample && 1 == mWavFormat.Channels))

                shBytesPerSample = 2;

            else if (16 == mWavFormat.BitsPerSample && 2 == mWavFormat.Channels)

                shBytesPerSample = 4;




            // RIFF 块 

            mWriter.Write(ChunkRiff);

            mWriter.Write(nLength);

            mWriter.Write(ChunkType);




            // WAVE块 

            mWriter.Write(ChunkFmt);

            mWriter.Write(nFormatChunkLength);

            mWriter.Write(shPad);

            mWriter.Write(mWavFormat.Channels);

            mWriter.Write(mWavFormat.SamplesPerSecond);

            mWriter.Write(mWavFormat.AverageBytesPerSecond);

            mWriter.Write(shBytesPerSample);

            mWriter.Write(mWavFormat.BitsPerSample);



            // 数据块 

            mWriter.Write(ChunkData);

            mWriter.Write((int)0);   // The sample length will be written in later. 

        }


        //<summary>

        //关闭所有资源

        //<summary>

        public void Close()
        {
            newsock.Shutdown(SocketShutdown.Both);
            newsock.Dispose();
            newsock = null;
            if (mWriter != null)
                mWriter.Close();
            if (mWaveFile != null)
                mWaveFile.Close();

            mWriter = null;

            mWaveFile = null;
        }

        /// <summary> 

        /// 设定录音结束后保存的文件,包括路径 

        /// </summary> 

        /// <param name="filename">保存wav文件的路径名</param> 

        public void SetFileName(string filename)

        {

            mFileName = filename;

        }
    }
}
