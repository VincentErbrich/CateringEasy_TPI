/*
 * Author : Vincent Erbrich - CPNV
 * Project : TPI 2018 Exam - CateringEasy
 * Project Manager : Pascal Benzonana
 * First Expert : Alain Roy
 * Second Expert : Laurent Ruchat
 * Creation date : 22/05/2018
 */

using MySql.Data.MySqlClient;
using System;

namespace CateringEasy
{
    public class PasswordCheck
    {
        /*
         * This method checks if the password string parameter is equal to the password contained in the database.
         */ 
        internal static bool PasswordIsCorrect(string password)
        {
            //Checks if password is empty
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            //Verifying if the password parameter corresponds to the password stored in the database. The method returns true if that is the case.
            else
            {
                DatabaseManager db = new DatabaseManager();
                MySqlDataReader results = db.SqlRequest("SELECT Password FROM cateasy_bd.settings;");
                try
                {
                    while (results.Read())
                    {
                        if (results.GetValue(0).Equals(password))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    if (results != null) results.Close();
                }

            }
        }
    }
}
