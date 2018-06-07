/*
 * Author : Vincent Erbrich - CPNV
 * Project : TPI 2018 Exam - CateringEasy
 * Project Manager : Pascal Benzonana
 * First Expert : Alain Roy
 * Second Expert : Laurent Ruchat
 * Creation date : 22/05/2018
 */
using System;
using MySql.Data.MySqlClient;

namespace CateringEasy
{
    /*
     *  This class manages the connection the the MySqlDatabase and executes MySql commands.
     *  
     *  SOURCES : This class was adapted from my Database_Manager class for SQLite I did in my Pré-TPI project. 
     *  Back in the Pré-TPI project, I was helped to develop this class by Cyril Kalbfuss, and was inspired by https://blog.tigrangasparian.com/2012/02/09/getting-started-with-sqlite-in-c-part-one/ and https://www.connectionstrings.com/sqlite/
     */
    class DatabaseManager
    {
        //Event launched once the database is updated.
        public event EventHandler<EventArgs> DatabaseUpdated;

        /*  
         * This methods initializes a connection the the database and returns it
         */
        public MySqlConnection Connexion()
        {
            try
            {
                // The connection is initialized in a new MySqlConnection object
                MySqlConnection dbConnexion = new MySqlConnection("Server=web20.swisscenter.com;Port=3306;UID=cateasy_bd;Password=P4$$w04d_?");
                // Connection is opened
                dbConnexion.Open();
                return dbConnexion;
            }
            catch (Exception e)
            {
                ExceptionManager.NewException(e, "La base de données n'a pas pu être trouvée : Veuillez contacter le support technique", true);
                return null;
            }
        }
        /*
         * This method executes the Sql request in the parameters and returns the results in a MySqlDataReader object. If specified in the parameters, triggers a DatabaseUpdated event
         */
        public MySqlDataReader SqlRequest(string Request, bool TriggerUpdateEvent)
        {
            if (Request == null)
            {
                throw new ArgumentNullException(nameof(Request));
            }
            try
            {
                MySqlConnection dbConnexion = Connexion(); // Connecting to the database
                MySqlCommand cmd = new MySqlCommand(Request, dbConnexion); // Initializes the command using the connection and the request in the parameters.
                MySqlDataReader reader = cmd.ExecuteReader(); // Executes the command and puts the results in a MySqlDataReader.

                //If the DatabaseUpdated EventHandler has been assigned from FormMain and the trigger event parameter is true, invokes DatabaseUpdated EventHandler
                if(TriggerUpdateEvent)
                    DatabaseUpdated?.Invoke(this, new EventArgs());
                //  Returns the results
                return reader;
            }
            catch(Exception e)
            {
                ExceptionManager.NewException(e, "Requête incorrecte", false);
                return null;
            }     
        }
        public void SqlRequestNonQuery(string Request, bool TriggerUpdateEvent)
        {
            if (Request == null)
            {
                throw new ArgumentNullException(nameof(Request));
            }
            //  Initialise la commande dans une nouvelle instance de l'objet SQLiteCommand
            //SQLiteCommand command = new SQLiteCommand(Request, Connexion());
            try
            {
                MySqlConnection dbConnexion = Connexion(); // Connecting to the database
                MySqlCommand cmd = new MySqlCommand(Request, dbConnexion); // Initializes the command using the connection and the request in the parameters.
                cmd.ExecuteNonQuery(); // Executes the command.

                //If the DatabaseUpdated EventHandler has been assigned from FormMain and the trigger event parameter is true, invokes DatabaseUpdated EventHandler
                if (TriggerUpdateEvent)
                    DatabaseUpdated?.Invoke(this, new EventArgs());
                cmd.Dispose();
            }
            catch (Exception e)
            {
                ExceptionManager.NewException(e, "Requête incorrecte", false);
            }
        }
    }
}
