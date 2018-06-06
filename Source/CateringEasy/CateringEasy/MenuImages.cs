/*
 * Author : Vincent Erbrich - CPNV
 * Project : TPI 2018 Exam - CateringEasy
 * Project Manager : Pascal Benzonana
 * First Expert : Alain Roy
 * Second Expert : Laurent Ruchat
 * Creation date : 22/05/2018
 */
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CateringEasy
{
    public static class MenuImages
    {
        public static ImageList ImlMenu { get; set; }

        private static void LoadMenuImages(DatabaseManager db)
        {
            MySqlDataReader reader = db.SqlRequest("", false); 
            while (reader.Read())
            {
                byte[] bytearray = (byte[])reader.GetValue(0);
               ImageConverter imgconverter = new ImageConverter();
               Image menuimg = imgconverter.ByteArrayToImage(bytearray);
               ImlMenu.Images.Add(reader.GetValue(0).ToString(), menuimg);
            }
        }
        public static void AddMenuImage(Image img, string imgkey)
        {

        }
        public static void RemoveMenuImage(string imgkey)
        {
            
        }
    }
}
