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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Opulos.Core.UI;


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
            BuildAccordion(accCuisine);
            Database_Manager db = new Database_Manager();
            db.Connexion();
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
            foreach (Panel p in Controls)
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
        private void BuildAccordion(Accordion acc)
        {
            Database_Manager db = new Database_Manager();
            db.Connexion();
            MySqlDataReader readerOrder = db.SqlRequest("SELECT * FROM cateasy_bd.order WHERE Completed = false;");
            if(readerOrder.HasRows)
            {
                int i = 0;
                try
                {
                    //For each order, adds a TableLayoutPanel to the accordion containing the details of the order
                    while (readerOrder.Read())
                    {
                        Panel pnl = new Panel { Dock = DockStyle.Fill };
                        TableLayoutPanel tlpCmdItems = new TableLayoutPanel { Width = 800, ColumnCount = 2, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(15), TabStop = false};

                        MySqlDataReader readerOrderItems = db.SqlRequest("SELECT IDMenuItem, Name FROM cateasy_bd.order_mitems RIGHT JOIN cateasy_bd.menuitem ON FIDMenuItem = IDMenuItem WHERE 'Completed' IS FALSE AND FIDOrder = " + readerOrder.GetValue(0) + ";");
                        if(readerOrderItems.HasRows)
                        {
                            int j = 0;
                            //Adds the details of the order to the TableLayoutPanel
                            while(readerOrderItems.Read())
                            {
                                tlpCmdItems.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 70 });
                                tlpCmdItems.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent, Height = 40 });
                                tlpCmdItems.Controls.Add(new Label { Text = readerOrderItems.GetValue(1).ToString(), Dock = DockStyle.Fill}, 0, j);
                                tlpCmdItems.Controls.Add(new Button { Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point), BackColor = Color.OrangeRed, Dock = DockStyle.Fill, Text = "Supprimmer", Name = "btnSupprimmerMItem" + j.ToString() }, 1, j);
                                j++;
                            }
                            //Adds the TableLayoutPanel containing order details to the main panel 
                            pnl.Controls.Add(tlpCmdItems);

                            //Adds action buttons to the main panel
                            TableLayoutPanel tlpCmdButtons = new TableLayoutPanel { ColumnCount = 3, RowCount = 1, Dock = DockStyle.Bottom, Width = 200 };

                            Button btnOrderStarted = new Button { Name = "btnOrderStarted" + readerOrder.GetValue(0).ToString(), Width = 200, BackColor = Color.Yellow, Height = 80, AutoSize = true, Text = "Commencer la préparation" };
                            btnOrderStarted.Click += new EventHandler(btnOrder_Click);
                            if((bool)readerOrder.GetValue(4))
                            {
                                btnOrderStarted.Enabled = false;
                            }

                            Button btnOrderEnded = new Button { Name = "btnOrderEnded" + readerOrder.GetValue(0).ToString(), Width = 200, BackColor = Color.Green, Height = 80, AutoSize = true, Text = "Terminée" };
                            btnOrderEnded.Click += new EventHandler(btnOrder_Click);

                            Button btnOrderDelete = new Button { Name = "btnOrderDelete" + readerOrder.GetValue(0).ToString(), Width = 200, BackColor = Color.OrangeRed, Height = 80, AutoSize = true, Text = "Supprimmer" };
                            btnOrderDelete.Click += new EventHandler(btnOrder_Click);

                            tlpCmdButtons.Controls.Add(btnOrderStarted, 0, 0);
                            tlpCmdButtons.Controls.Add(btnOrderEnded, 1, 0);
                            tlpCmdButtons.Controls.Add(btnOrderDelete, 2, 0);
                            pnl.Controls.Add(tlpCmdButtons);
                        }
                        pnl.Name = "pnlOrder" + readerOrder.GetValue(0).ToString();
                        accCuisine.Add(pnl, "Commande " + readerOrder.GetValue(0).ToString() + " Table " + readerOrder.GetValue(1).ToString(), "", 0, false);
                        i++;
                    }
                }
                catch(Exception e)
                {
                    Exception_Manager.NewException(e, "Erreur lors du chargement des commandes. Veuillez contacter le support technique", false);
                }

            }
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
        /*
         * Event handler method for Click event on buttons generated dynamically for the Cuisine order panel.
         * Depending on which button is clicked, marks an order as started, marks an order as ended or deletes the order.
         */
        private void btnOrder_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Database_Manager db = new Database_Manager();
            db.Connexion();
            try
            {
                if (Regex.IsMatch(btn.Name, "btnOrderStarted*"))
                {
                    MySqlDataReader reader = db.SqlRequest("UPDATE cateasy_bd.order SET Started = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    btn.Enabled = false;
                }
                else if (Regex.IsMatch(btn.Name, "btnOrderEnded*"))
                {
                    MySqlDataReader reader = db.SqlRequest("UPDATE cateasy_bd.order SET Started = true, Completed = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    accCuisine.Controls.Remove(btn.Parent);
                }
                else if (Regex.IsMatch(btn.Name, "btnOrderDelete*"))
                {
                    MySqlDataReader reader = db.SqlRequest("UPDATE cateasy_bd.order SET Started = true, Completed = true, Paid = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    btn.Parent.Parent.Dispose();
                    BuildAccordion(accCuisine);
                }
            }
            catch(Exception ex)
            {
                Exception_Manager.NewException(ex, "Nous avons rencontré un problème lors de la mise à jour de la commande. Veuillez contacter le support technique.", false);
            }
        }

    }
}
