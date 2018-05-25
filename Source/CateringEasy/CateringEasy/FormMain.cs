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
        private DataGridView dgvMenu = new DataGridView
        {
            Dock = DockStyle.Fill,
            Anchor = AnchorStyles.Top,
            Name = "dgvMenu",
            ColumnCount = 2
        };
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
        private void BuildMenuList(bool edit)
        {
            //string[] imagekeys = new string[imlMenu.Images.Count];
            //imlMenu.Images.Keys.CopyTo(imagekeys, 0);
            /*DataGridView dgvMenu = new DataGridView
            {
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top,
                Name = "dgvMenu",
                ColumnCount = 2
            };*/

            dgvMenu.Columns[0].Name ="Nom";
            dgvMenu.Columns[1].Name ="Prix";

            if (edit)
            {
                dgvMenu.ColumnCount++;
                dgvMenu.Columns[2].Name = "Quantité";
            }

            Database_Manager db = new Database_Manager();
            db.Connexion();

            string Currency = "CHF";
            MySqlDataReader readerCurrency = db.SqlRequest("SELECT Currency FROM cateasy_bd.settings;");
            try
            {
                if (readerCurrency.HasRows)
                {
                    while (readerCurrency.Read())
                    {
                        Currency = readerCurrency.GetValue(0).ToString();
                    }
                }
            }
            catch { }


            MySqlDataReader readerMenu = db.SqlRequest("SELECT Name, Price FROM cateasy_bd.MenuItem WHERE Remaining > 1");
            try
            {
                if(readerMenu.HasRows)
                {
                    while(readerMenu.Read())
                    {
                        if(edit)
                        {

                        }
                        else
                        {
                            Object[] values = new Object[readerMenu.FieldCount];
                            int fieldCount = readerMenu.GetValues(values);

                            for (int i = 0; i < fieldCount; i++)
                            {
                                dgvMenu.Rows.Add(values[0].ToString(), values[1]);
                            }
                        }
                    }
                }

                DataGridViewButtonColumn dgbAddColumn = new DataGridViewButtonColumn();
                dgbAddColumn.HeaderText = "Ajouter";
                dgbAddColumn.Text = "+";
                dgbAddColumn.UseColumnTextForButtonValue = true;
                dgvMenu.Columns.Insert(2, dgbAddColumn);

                DataGridViewTextBoxColumn dgtSelected = new DataGridViewTextBoxColumn();
                dgtSelected.Name = "dgtSelected";
                dgtSelected.DefaultCellStyle.NullValue = "0";
                dgtSelected.HeaderText = "";
                dgvMenu.Columns.Insert(3, dgtSelected);

                DataGridViewButtonColumn dgbRemoveColumn = new DataGridViewButtonColumn();
                dgbRemoveColumn.HeaderText = "Retirer";
                dgbRemoveColumn.Text = "-";
                dgbRemoveColumn.UseColumnTextForButtonValue = true;
                dgvMenu.Columns.Insert(4, dgbRemoveColumn);

            }
            catch(Exception e)
            {
                Exception_Manager.NewException(e, "Nous avons rencontré un problème lors de la construction du menu. Veuillez contacter le support technique", true);
            }
            dgvMenu.CellContentClick += new DataGridViewCellEventHandler(dgvMenu_CellContentClick);
            tlpMenu.Controls.Add(dgvMenu);
            
            
        }
        private void dgvMenu_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                if(e.RowIndex == 2)
                {
                    int TimesSelected = Convert.ToInt16(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) + 1;
                    senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value = TimesSelected.ToString();
                }
                else if (e.RowIndex == 2 && Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) >= 0)
                {
                    int TimesSelected = Convert.ToInt16(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) + - 1;
                    senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value = TimesSelected.ToString();
                }
            }
        }
        /*private void dgvMenu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore clicks that are not on button cells. 
            if (e.RowIndex < 0 || e.ColumnIndex !=
            dgvMenu.Columns["Ajouter"].Index) return;

            // Retrieve the task ID.
            Int32 taskID = (Int32)dgvMenu[0, e.RowIndex].Value;

            dgvMenu.Rows[taskID].Cells[3].Value = "Soin";

        }*/
        /*
         * This method builds the order list in the Cuisine part of the application 
         */
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
                    db.SqlRequest("UPDATE cateasy_bd.order SET Started = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    btn.Enabled = false;
                }
                else if (Regex.IsMatch(btn.Name, "btnOrderEnded*"))
                {
                    db.SqlRequest("UPDATE cateasy_bd.order SET Started = true, Completed = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    MySqlDataReader reader = db.SqlRequest("SELECT FIDMenuItem FROM cateasy_bd.order LEFT JOIN cateasy_bd.order_mitems ON IDOrder = FIDOrder WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    try
                    {
                        if(reader.HasRows)
                        {
                            while(reader.Read())
                            {
                                db.SqlRequest("UPDATE cateasy_bd.menuitem SET Remaining = Remaining - 1 WHERE IDMenuItem = " + reader.GetValue(0) + ";");
                            }
                            reader.Close();
                        }
                    }
                    catch(Exception ex2)
                    {
                        Exception_Manager.NewException(ex2, "Nous avons rencontré un problème lors de la mise à jour du stock. Veuillez contacter le support technique", true);
                    }
                    btn.Parent.Parent.Dispose();
                }
                else if (Regex.IsMatch(btn.Name, "btnOrderDelete*"))
                {
                    db.SqlRequest("UPDATE cateasy_bd.order SET Started = true, Completed = true, Paid = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    btn.Parent.Parent.Dispose();
                }
            }
            catch(Exception ex)
            {
                Exception_Manager.NewException(ex, "Nous avons rencontré un problème lors de la mise à jour de la commande. Veuillez contacter le support technique.", false);
            }
        }
        /*
         * Event handler method for Click event on the confirm table number button
         * Redirects to the menu and saves the table number.
         */
        private void btnTable_selectConfirm_Click(object sender, EventArgs e)
        {
            BuildMenuList(false);
            ShowTableLayoutPanel(tlpMenu, tlpTable_select);
        }
    }
}
