/*
 * Author : Vincent Erbrich - CPNV
 * Project : TPI 2018 Exam - CateringEasy
 * Project Manager : Pascal Benzonana
 * First Expert : Alain Roy
 * Second Expert : Laurent Ruchat
 * Creation date : 22/05/2018
 */
using System.Drawing;
using System.IO;

namespace CateringEasy
{
    /*
     * CLASS AUTHOR : Rajan Tawate 
     * Found on the Code Project website https://www.codeproject.com/articles/15460/c-image-to-byte-array-and-byte-array-to-image-conv
     * Licensed under Code Project Open License (CPOL) https://www.codeproject.com/info/cpol10.aspx. Commercial or non-commercial use is authorized.
     * 
     * This class is used to convert a byte array to an image object or the other way around.
     */
    class ImageConverter
    {
        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }
        public Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

    }
}
