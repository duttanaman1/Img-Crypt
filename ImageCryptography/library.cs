using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageCrypto
{
  internal static class HelperLibrary
  {
    public static byte[] DecodeHex(string hextext)
    {
      var arr = hextext.Split('-');
      var array = new byte[arr.Length];
      for (var i = 0; i < arr.Length; i++)
        try
        {
          array[i] = Convert.ToByte(arr[i], 16);
        }
        catch (Exception ex)
        {
          Console.Write(ex.Message);
        }

      return array;
    }

    public static bool IsPrime(int number)
    {
      if (number < 2) return false;
      if (number % 2 == 0) return number == 2;
      var root = (int)Math.Sqrt(number);
      for (var i = 3; i <= root; i += 2)
        if (number % i == 0)
          return false;

      return true;
    }

    public static Bitmap ConvertByteToImage(byte[] bytes)
    {
      return new Bitmap(Image.FromStream(new MemoryStream(bytes)));
    }

    public static byte[] ConvertImageToByte(Image myImage)
    {
      var m1 = new MemoryStream();
      new Bitmap(myImage).Save(m1, ImageFormat.Jpeg);
      var header = m1.ToArray();
      return header;
    }
  }
}