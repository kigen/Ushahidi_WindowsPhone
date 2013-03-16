using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace Ushahidi
{
   public class ImageConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return FromAssets();
            }
            else
            {
                string url = value.ToString();
                return LoadPictrueByUrl(url);
            }
            }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }


        private BitmapImage LoadPictrueByUrl(string url)        {         
           
            var bitmapImage = new BitmapImage();
            //bitmapImage.SetSource(stream);
            return bitmapImage;

        }

        public BitmapImage FromAssets()
        {
            var m_Image = new BitmapImage(new Uri("/Assets/placeholder.png"));
            return m_Image;
        }

    }
}
