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
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CateringEasy
{
    public partial class FormMain : Form
    {
        //String property indicating from which TableLayoutPanel the display of the currently displayed TableLayoutPanel has been ordered
        private TableLayoutPanel RedirectedFromTlp { get; set; }
        /*
         * Constructor method. Initializes the Form
         */ 
        public FormMain()
        {
            InitializeComponent();
        }
        /*
         * Load event handler method for this Form. Shows the home panel and loads images from the database
         */ 
        private void FormMain_Load(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpHome, tlpHome);
            MenuImages.LoadMenuImages();
        }
        /*
         * This method requests the HideAllPanels method to hide every TableLayoutPanel, then shows the TableLayoutPanel requested in parameter tlp.
         */
        private void ShowTableLayoutPanel(TableLayoutPanel tlpToShow, TableLayoutPanel tlpCurrent)
        {
            RedirectedFromTlp = tlpCurrent;
            HideAllPanels();
            tlpToShow.Show();
        }
        /*
         * This method hides all Controls of type TableLayoutPanel contained in FormMain.
         */
        private void HideAllPanels()
        {
            foreach (Panel p in this.Controls)
            {
                p.Hide();
            }
        }
        private void BuildList(ListView lsv)
        {
            string[] imagekeys = new string[imlMenu.Images.Count];
            imlMenu.Images.Keys.CopyTo(imagekeys, 0);

            Image img = imlMenu.Images[0];
        }
        private void EmptyTableLayoutPanel(TableLayoutPanel tlp)
        {
            tlp.ResetText();
        }

        private void pctHomeSettings_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpSettings, tlpHome);
        }

        private void btnHomeServeur_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpServeur, tlpHome);
        }

        private void btnHomeClient_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpTable_select, tlpHome);
        }
        private void btnHomeCuisine_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpCuisine, tlpHome);
        }
        /*
         * Event handler method for Click event on return buttons.
         * Executes ShowTableLayoutPanel() method to show the panel that has requested redirection to the current panel
         */
        private void btnReturn_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(RedirectedFromTlp, tlpSettings);
        }


    }
}
