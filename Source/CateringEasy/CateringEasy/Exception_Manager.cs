using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CateringEasy
{
    public class Exception_Manager
    {
        public static void NewException(Exception e, string message, bool critical)
        {
            MessageBox.Show(message + "\n" + e.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (critical == true)
            {
                Application.Exit();
            }
        }
    }
}
