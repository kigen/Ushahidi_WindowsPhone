using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Ushahidi.Library.Data;
using Ushahidi.Library.Network;

namespace Ushahidi
{
    public partial class CreateReport : PhoneApplicationPage
    {
        public CreateReport()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            PhotoChooserTask tsk = new PhotoChooserTask();
            tsk.ShowCamera = true;
            tsk.Completed += new EventHandler<PhotoResult>(tsk_Completed);
            tsk.Show();
        }

        void tsk_Completed(object sender, PhotoResult e)
        {


            string[] pp = e.OriginalFileName.Split(new string[] { "PlatformData" }, StringSplitOptions.None);

            string FileName = pp[1].Replace("\\", "");
            FileName = FileName.Replace("jpeg", "jpg");
            UploadImage image = new UploadImage()
            {
                FileName = FileName,
                FileType = "image/jpeg",
                Image = ReadToEnd(e.ChosenPhoto)
            };



          


            UploadReport myreport = new UploadReport()
            {
                CategoryList = "1,2,10,14",
                Deployment = (App.Current as App).SelectedDeployment,
                IncidentDate = DateTime.Now,
                incidenttitle = "Windows Phone Test",
                incidentdescription = "Here where we are",
                locationlatitude = "1",
                locationlongitude = "35",
                locationname = "Nairobi",
                Photos = new List<UploadImage>() { image }
            };


            WebTools tools = new WebTools();
            tools.ReportUpload(myreport);



        }



        public byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = stream.Position;
            stream.Position = 0;

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }
    }
}