/*
 * Author : Vincent Erbrich - CPNV
 * Project : TPI 2018 Exam - CateringEasy
 * Project Manager : Pascal Benzonana
 * First Expert : Alain Roy
 * Second Expert : Laurent Ruchat
 * Creation date : 22/05/2018
 */
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace CateringEasy
{
    
    /*
     *  CLASSE DATABASE MANAGER
     *  Cette classe gère la connexion vers le fichier de base de données SQLite et 
     *  éxecute les requêtes SQL envers cette dernière. 
     *  Elle génère la dite base si elle est inexistante.
     *  
     *  SOURCES / AIDES : Remerciements à Cyril Kalbfuss, inspiré de https://blog.tigrangasparian.com/2012/02/09/getting-started-with-sqlite-in-c-part-one/ et https://www.connectionstrings.com/sqlite/
     */
    class DatabaseManager
    {
        /*  
         *  METHODE CONNEXION
         *  Cette méthode crée et gère la connexion vers le fichier de base de données
         *  Elle génère la base de données si elle est inexistante
         *      
         *  RETOURNE : La connexion à la base de donnée
         */
        public MySqlConnection Connexion()
        {
            try
            {
                //  La connexion est initialisée dans une instance de l'objet SQLiteConnection
                //MySqlConnection m_dbConnection = new MySqlConnection("Server=127.0.0.1;Port=3306;UID=cateringeasy;Password=P4$$w04d_?");
                MySqlConnection m_dbConnection = new MySqlConnection("Server=localhost;Port=3306;UID=cateasy_bd;Password=P4$$w04d_?");
                //  La connexion est ouverte
                m_dbConnection.Open();
                //  La connexion est retournée et la méthode stoppée
                return m_dbConnection;
            }
            catch (Exception e)
            {
                ExceptionManager.NewException(e, "La base de données n'a pas pu être trouvée : Veuillez contacter le support technique", true);
                return null;
            }
        }
        /*
         *  Méthode SQLREQUEST
         *  Execute la requête SQLite passée en entrée
         *  
         *  RETOURNE :  le résultat de la requête
         */
        public MySqlDataReader SqlRequest(string Request)
        {
            if (Request == null)
            {
                throw new ArgumentNullException(nameof(Request));
            }
            //  Initialise la commande dans une nouvelle instance de l'objet SQLiteCommand
            //SQLiteCommand command = new SQLiteCommand(Request, Connexion());
            try
            {
                MySqlConnection dbConnexion = Connexion();

                MySqlCommand cmd = new MySqlCommand(Request, dbConnexion);
                MySqlDataReader reader = cmd.ExecuteReader();
                //  Retourne le résultat de la requête
                return reader;
            }
            catch(Exception e)
            {
                //  Exectute ce code en cas d'erreur :
                //  Passe l'exception à Exception_Manager
                ExceptionManager.NewException(e, "Requête incorrecte", false);
                //  Ne retourne rien
                return null;
            }     

        }

    }
}
