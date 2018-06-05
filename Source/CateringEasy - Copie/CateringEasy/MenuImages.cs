/*
 * Author : Vincent Erbrich - CPNV
 * Project : TPI 2018 Exam - CateringEasy
 * Project Manager : Pascal Benzonana
 * First Expert : Alain Roy
 * Second Expert : Laurent Ruchat
 * Creation date : 22/05/2018
 */
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CateringEasy
{
    public static class MenuImages
    {
        public static ImageList ImlMenu { get; set; }

        public static void LoadMenuImages()
        {
            //SqlDataReader reader = new SqlDataReader(/**/);
            //while (reader.Read())
            //{
            //    byte[] bytearray = (byte[])reader.GetValue(/**/);
            //    ImageConverter imgconverter = new ImageConverter();
            //    Image menuimg = imgconverter.ByteArrayToImage(bytearray);
            //    ImlMenu.Images.Add(reader.GetValue(/**/).ToString(), menuimg);
            //}
        }
        public static void AddMenuImage(Image img, string imgkey)
        {

        }
        public static void RemoveMenuImage(string imgkey)
        {
            
        }
    }
}
