using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CateringEasy
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            CarteRestaurant crt = new CarteRestaurant();
        }
    }
}
