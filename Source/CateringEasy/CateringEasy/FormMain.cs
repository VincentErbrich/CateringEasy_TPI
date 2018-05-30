﻿/*
 * Author : Vincent Erbrich - CPNV
 * Project : TPI 2018 Exam - CateringEasy
 * Project Manager : Pascal Benzonana
 * First Expert : Alain Roy
 * Second Expert : Laurent Ruchat
 * Creation date : 22/05/2018
 */

using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
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
            AutoSize = true,
            Name = "dgvMenu",
            RowHeadersVisible = false,
        };
        //String property indicating from which TableLayoutPanel the display of the currently displayed TableLayoutPanel has been ordered
        private TableLayoutPanel RedirectedFromTlp { get; set; }

        private bool Waiter { get; set; }
        //int property indicating which table number was last selected
        private Int16 SelectedTable { get; set; }

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
            DatabaseManager db = new DatabaseManager();
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

            dgvMenu.Rows.Clear();
            dgvMenu.Columns.Clear();
            dgvMenu.ColumnCount = 2;
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
            dgvMenu.Columns[0].ReadOnly = true;
            dgvMenu.Columns[1].ReadOnly = true;


            DatabaseManager db = new DatabaseManager();
            db.Connexion();

            if (edit)
            {
                DataGridViewTextBoxColumn dgtStock = new DataGridViewTextBoxColumn();
                dgtStock.Name = "dgtStock";
                dgtStock.DefaultCellStyle.NullValue = "0";
                dgtStock.HeaderText = "Quantité";
                dgvMenu.Columns.Insert(2, dgtStock);
            }
            else
            {
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
                    if (readerMenu.HasRows)
                    {
                        while (readerMenu.Read())
                        {
                            Object[] values = new Object[readerMenu.FieldCount];
                            int fieldCount = readerMenu.GetValues(values);
                            dgvMenu.Rows.Add(values[0].ToString(), values[1] + Currency);
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
                catch (Exception e)
                {
                    ExceptionManager.NewException(e, "Nous avons rencontré un problème lors de la construction du menu. Veuillez contacter le support technique", true);
                }
                dgvMenu.CellContentClick += new DataGridViewCellEventHandler(dgvMenu_CellContentClick);
            }

            tlpMenu.Controls.Add(dgvMenu);
            
            
        }
        private void dgvMenu_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                if(e.ColumnIndex == 2 && Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) >= 0)
                {
                    int TimesSelected = Convert.ToInt16(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) + 1;
                    senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value = TimesSelected.ToString();
                }
                else if (e.ColumnIndex == 4 && Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) > 0)
                {
                    int TimesSelected = Convert.ToInt16(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) - 1;
                    senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value = TimesSelected.ToString();
                }
            }
        }
        /*
         * This method builds the order list in the Cuisine part of the application 
         */
        private void BuildCuisineHome()
        {
            foreach(Accordion acc in tlpCuisine.Controls.OfType<Accordion>())
            {
                tlpCuisine.Controls.Remove(acc);
                break;
            }
            Accordion accCuisine = BuildOrdersAccordion();
            DatabaseManager db = new DatabaseManager();
            db.Connexion();
            MySqlDataReader readerOrder = db.SqlRequest("SELECT * FROM cateasy_bd.order WHERE Completed = false;");
            try
            {
                if (readerOrder.HasRows)
                {
                    int i = 0;
                    try
                    {
                        //For each order, adds a TableLayoutPanel to the accordion containing the details of the order
                        while (readerOrder.Read())
                        {
                            Panel pnl = new Panel { Dock = DockStyle.Fill };

                            MySqlDataReader readerOrderItems = db.SqlRequest("SELECT IDMenuItem, Name FROM cateasy_bd.order_mitems RIGHT JOIN cateasy_bd.menuitem ON FIDMenuItem = IDMenuItem WHERE 'Completed' IS FALSE AND FIDOrder = " + readerOrder.GetValue(0) + ";");
                            if (readerOrderItems.HasRows)
                            {
                                int j = 0;
                                TableLayoutPanel tlpCmdItems = new TableLayoutPanel { Width = 800, ColumnCount = 2, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(15), TabStop = false, Name = "tlpCmdItems" + readerOrder.GetValue(0)};

                                //Adds the details of the order to the TableLayoutPanel
                                while (readerOrderItems.Read())
                                {
                                    Button btnDeleteItem = new Button
                                    {
                                        Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point),
                                        BackColor = Color.OrangeRed,
                                        Dock = DockStyle.Fill,
                                        Text = "Supprimmer",
                                        Name = "btnDeleteItem" + j.ToString()
                                    };
                                    btnDeleteItem.Click += new EventHandler(btnDeleteItem_Click);

                                    tlpCmdItems.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 70 });
                                    tlpCmdItems.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent, Height = 40 });
                                    tlpCmdItems.Controls.Add(new Label { Text = readerOrderItems.GetValue(1).ToString(), Dock = DockStyle.Fill }, 0, j);
                                    tlpCmdItems.Controls.Add(btnDeleteItem, 1, j);
                                    j++;
                                }
                                //Adds the TableLayoutPanel containing order details to the main panel 
                                pnl.Controls.Add(tlpCmdItems);
                            }

                            //Adds action buttons to the main panel
                            TableLayoutPanel tlpCmdButtons = new TableLayoutPanel { ColumnCount = 3, RowCount = 1, Dock = DockStyle.Bottom, Width = 200 };

                            Button btnOrderStarted = new Button { Name = "btnOrderStarted" + readerOrder.GetValue(0).ToString(), Width = 200, BackColor = Color.Yellow, Height = 80, AutoSize = true, Text = "Commencer la préparation" };
                            btnOrderStarted.Click += new EventHandler(btnOrder_Click);
                            if ((bool)readerOrder.GetValue(4))
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
                            pnl.Name = "pnlOrder" + readerOrder.GetValue(0).ToString();
                            accCuisine.Add(pnl, "Commande " + readerOrder.GetValue(0).ToString() + " Table " + readerOrder.GetValue(1).ToString(), "", 0, false);
                            i++;
                        }
                        tlpCuisine.Controls.Add(accCuisine, 0, 1);
                    }
                    catch (Exception e)
                    {
                        ExceptionManager.NewException(e, "Erreur lors du chargement des commandes. Veuillez contacter le support technique", false);
                    }

                }
            }
            catch (Exception e)
            {
                ExceptionManager.NewException(e, "Erreur lors du chargement des commandes. Veuillez contacter le support technique", false);
            }
        }

        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            DatabaseManager db = new DatabaseManager();
            db.Connexion();

            Button btn = sender as Button;
            db.SqlRequest("");
            //btn.Parent.Name;
        }

        private static Accordion BuildOrdersAccordion()
        {
            return new Accordion
            {
                AddResizeBars = false,
                AllowMouseResize = false,
                AnimateCloseEffect = ((AnimateWindowFlags.VerticalNegative | AnimateWindowFlags.Hide)
                            | AnimateWindowFlags.Slide),
                AnimateCloseMillis = 100,
                AnimateOpenEffect = ((AnimateWindowFlags.VerticalPositive | AnimateWindowFlags.Show)
                            | AnimateWindowFlags.Slide),
                AnimateOpenMillis = 100,
                AutoFixDockStyle = true,
                AutoScroll = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.WhiteSmoke,
                CheckBoxFactory = null,
                CheckBoxMargin = new Padding(2),
                ContentBackColor = null,
                ContentMargin = new Padding(15, 5, 15, 5),
                ContentPadding = new Padding(1),
                ControlBackColor = null,
                ControlMinimumHeightIsItsPreferredHeight = true,
                ControlMinimumWidthIsItsPreferredWidth = true,
                Dock = DockStyle.Fill,
                DownArrow = null,
                FillHeight = true,
                FillLastOpened = false,
                FillModeGrowOnly = false,
                FillResetOnCollapse = false,
                FillWidth = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0),
                GrabCursor = Cursors.SizeNS,
                GrabRequiresPositiveFillWeight = true,
                GrabWidth = 6,
                GrowAndShrink = true,
                Insets = new Padding(10),
                Location = new Point(3, 39),
                Name = "accCuisine",
                OpenOnAdd = false,
                OpenOneOnly = false,
                Padding = new Padding(10),
                ResizeBarFactory = null,
                ResizeBarsAlign = 0.5D,
                ResizeBarsArrowKeyDelta = 10,
                ResizeBarsFadeInMillis = 800,
                ResizeBarsFadeOutMillis = 800,
                ResizeBarsFadeProximity = 24,
                ResizeBarsFill = 1D,
                ResizeBarsKeepFocusAfterMouseDrag = false,
                ResizeBarsKeepFocusIfControlOutOfView = true,
                ResizeBarsKeepFocusOnClick = true,
                ResizeBarsMargin = null,
                ResizeBarsMinimumLength = 50,
                ResizeBarsStayInViewOnArrowKey = true,
                ResizeBarsStayInViewOnMouseDrag = true,
                ResizeBarsStayVisibleIfFocused = true,
                ResizeBarsTabStop = true,
                ShowPartiallyVisibleResizeBars = false,
                ShowToolMenu = true,
                ShowToolMenuOnHoverWhenClosed = false,
                ShowToolMenuOnRightClick = true,
                ShowToolMenuRequiresPositiveFillWeight = false,
                Size = new Size(1074, 999),
                TabIndex = 3,
                UpArrow = null
            };
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
            Waiter = true;
            ShowTableLayoutPanel(tlpServeur, tlpHome);
        }
        private void btnHomeClient_Click(object sender, EventArgs e)
        {
            Waiter = false;
            ShowTableLayoutPanel(tlpTable_select, tlpHome);
        }
        private void btnHomeCuisine_Click(object sender, EventArgs e)
        {
            BuildCuisineHome();
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
            DatabaseManager db = new DatabaseManager();
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
                        ExceptionManager.NewException(ex2, "Nous avons rencontré un problème lors de la mise à jour du stock. Veuillez contacter le support technique", true);
                    }
                    BuildCuisineHome();
                }
                else if (Regex.IsMatch(btn.Name, "btnOrderDelete*"))
                {
                    db.SqlRequest("UPDATE cateasy_bd.order SET Started = true, Completed = true, Paid = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    BuildCuisineHome();
                }
            }
            catch(Exception ex)
            {
                ExceptionManager.NewException(ex, "Nous avons rencontré un problème lors de la mise à jour de la commande. Veuillez contacter le support technique.", false);
            }
        }
        /*
         * Event handler method for Click event on the confirm table number button
         * Redirects to the menu and saves the table number.
         */
        private void btnTable_selectConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseManager db = new DatabaseManager();
                db.Connexion();
                MySqlDataReader reader = db.SqlRequest("SELECT IDTable FROM cateasy_bd.table ORDER BY IDTable DESC;");

                SelectedTable = Convert.ToInt16(txtTable_selectNumber.Text);

                if(reader.HasRows)
                {
                    if(reader.Read())
                    {
                        if(Convert.ToInt16(reader.GetValue(0)) >= SelectedTable)
                        {
                            txtTable_selectNumber.Text = "";
                            lblTable_selectError.Text = "";
                            BuildMenuList(false);
                            ShowTableLayoutPanel(tlpMenu, tlpTable_select);
                        }
                        else
                            lblTable_selectError.Text = "Cette table n'existe pas";
                    }
                }
                else
                {
                    lblTable_selectError.Text = "Veuillez ajouter une table dans les réglages";
                }
            }
            catch
            {
                lblTable_selectError.Text = "Veuillez entrer un nombre";
            }
            
        }

        private void btnMenuConfirm_Click(object sender, EventArgs e)
        {
            DatabaseManager db = new DatabaseManager();
            db.Connexion();
            string valuesToInsert = "";
            bool orderCreated = false;
            bool itemSelected = false;
            Int16 orderId = 0;
            foreach (DataGridViewRow row in dgvMenu.Rows)
            {
                //Checks if a menu item has been selected in the TextBox DataGridViewRow of the dgvMenu DataGridView.
                if (Convert.ToInt16(row.Cells[3].Value) > 0)
                {
                    //If a menu item has been selected and the order has not been created yet, inserts a new order in the database and saves the autoincremented id of the order in the orderId Int16 varialble.
                    if(!orderCreated)
                    {
                        MySqlDataReader reader = db.SqlRequest("INSERT INTO cateasy_bd.order(FIDTable, Completed, Started) VALUES(" + SelectedTable.ToString() + ", false, false); SELECT LAST_INSERT_ID();");
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    orderId = Convert.ToInt16(reader.GetValue(0));
                                    orderCreated = true;
                                }
                            }
                            else
                            {
                                throw new Exception("Problème interne avec la base de données");
                            }
                        }
                        catch (Exception e2)
                        {
                            ExceptionManager.NewException(e2, " ", true);
                        }
                        itemSelected = true;
                    }

                    //Adds the insert query corresponding to the ordered menu item to add to the database in the valuesToInsert string.
                    for (int i = 0; i < Convert.ToInt16(row.Cells[3].Value.ToString()); i++)
                    {
                        try
                        {
                            //Gets the remaining stock of the item in the current row
                            MySqlDataReader readerStock = db.SqlRequest("SELECT Remaining FROM cateasy_bd.menuitem WHERE IDMenuItem = " + (row.Index + 1) + ";");
                            if (readerStock.Read() && readerStock.HasRows)
                            {
                                if (i < Convert.ToInt16(readerStock.GetValue(0).ToString()))
                                {
                                    valuesToInsert += "INSERT INTO cateasy_bd.order_mitems(FIDMenuItem, FIDOrder) VALUES(" + (row.Index + 1) + ", " + orderId + ");";
                                }
                                else
                                    break;
                            }
                            else
                            {
                                throw new Exception("Problème interne avec la base de données");
                            }
                        }
                        catch (Exception e2)
                        {
                            ExceptionManager.NewException(e2, " ", true);
                        }
                    }
                }
            }
            //If one or more item has been selected for order, adds the ordered item(s) to the database and shows the order confirmation TableLayoutPanel.
            if(itemSelected)
            {
                MessageBox.Show(valuesToInsert);
                db.SqlRequest(valuesToInsert);
                BuildTlpOrderConfirm(Waiter);
                ShowTableLayoutPanel(tlpOrder_confirmation, tlpMenu);
                btnMenuConfirm.Text = "Confirmer";
                btnMenuConfirm.ForeColor = DefaultForeColor;
            }
            //If no item has been selected for order, asks the user to select an item.
            else
            {
                btnMenuConfirm.Text = "Veuillez ajouter un élément à votre sélection puis re-cliquez";
                btnMenuConfirm.ForeColor = Color.Red;
            }
        }

        private void BuildTlpOrderConfirm(bool waiter)
        {
            if (waiter)
            {
                lblOrder_confirmation.Text = "Votre commande à bien été reçue";
                bool alreadyBuilt = false;
                foreach(Control c in tlpOrder_confirmation.Controls)
                {
                    if (c.Name == "tlpReturnToHomeProtection")
                    {
                        tlpOrder_confirmation.Controls.Remove(c);
                        break;
                    }
                    else if (c.Name == "btnServeurReturn")
                    {
                        alreadyBuilt = true;
                        break;
                    }
                }
                if(!alreadyBuilt)
                {
                    Button btnReturnToWaiterHome = new Button
                    {
                        Dock = DockStyle.Bottom,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                        Location = new Point(3, 948),
                        Name = "btnServeurReturn",
                        Size = new Size(1074, 111),
                        TabIndex = 2,
                        Text = "OK",
                        UseVisualStyleBackColor = true,
                    };
                    btnReturnToWaiterHome.Click += new EventHandler(btnReturnToHome_Click);
                    tlpOrder_confirmation.Controls.Add(btnReturnToWaiterHome, 0, 1);
                }
            }
            else
            {
                bool alreadyBuilt = false;
                foreach (Control c in tlpOrder_confirmation.Controls)
                {
                    if (c.Name == "btnServeurReturn")
                    {
                        tlpOrder_confirmation.Controls.Remove(c);
                        break;
                    }
                    else if(c.Name == "txtReturnToHomeProtection")
                    {
                        c.ResetText();
                        alreadyBuilt = true;
                        break;
                    }
                        
                }
                if (!alreadyBuilt)
                {
                    TableLayoutPanel tlpReturnToHomeProtection = new TableLayoutPanel
                    {
                        ColumnCount = 2,
                        Dock = DockStyle.Fill,
                        Location = new Point(3, 468),
                        Name = "tlpReturnToHomeProtection",
                        RowCount = 2,
                        Size = new Size(1074, 591),
                        TabIndex = 3,
                    };
                    Label lblReturnToHomeProtection = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0),
                        Location = new Point(237, 137),
                        Name = "lblReturnToHomeProtection",
                        Size = new Size(297, 90),
                        TabIndex = 0,
                        Text = "Mot de passe \r\n(Pour le personnel)",
                    };
                    TextBox txtReturnToHomeProtection = new TextBox
                    {
                        Anchor = AnchorStyles.Left,
                        BorderStyle = BorderStyle.FixedSingle,
                        Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0),
                        Location = new Point(540, 151),
                        MinimumSize = new Size(146, 27),
                        Name = "txtReturnToHomeProtection",
                        PasswordChar = '*',
                        Size = new Size(531, 63),
                        TabIndex = 4,
                        TextAlign = HorizontalAlignment.Center,
                    };
                    Button btnReturnToHomeProtection = new Button
                    {
                        Anchor = AnchorStyles.Top,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                        Location = new Point(540, 368),
                        Name = "btnReturnToHomeProtection",
                        Size = new Size(531, 69),
                        TabIndex = 2,
                        Text = "Retour",
                        UseVisualStyleBackColor = true,
                    };
                    btnReturnToHomeProtection.Click += new EventHandler(btnReturnToHomeProtection_Click);

                    tlpReturnToHomeProtection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    tlpReturnToHomeProtection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    tlpReturnToHomeProtection.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                    tlpReturnToHomeProtection.RowStyles.Add(new RowStyle(SizeType.Absolute, 226F));

                    tlpReturnToHomeProtection.Controls.Add(lblReturnToHomeProtection, 0, 0);
                    tlpReturnToHomeProtection.Controls.Add(btnReturnToHomeProtection, 1, 1);
                    tlpReturnToHomeProtection.Controls.Add(txtReturnToHomeProtection, 1, 0);

                    tlpOrder_confirmation.Controls.Add(tlpReturnToHomeProtection, 0, 1);
                }
            }
        }
        private void btnReturnToHome_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpServeur, tlpOrder_confirmation);
        }
        private void btnReturnToHomeProtection_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            foreach (Control cTxt in btn.Parent.Controls.OfType<TextBox>())
            {
                if(PasswordCheck.PasswordIsCorrect(cTxt.Text))
                {
                    foreach (Control cLbl in tlpOrder_confirmation.Controls.OfType<Label>())
                    {
                        cLbl.Text = "Mot de passe \r\n(Pour le personnel)";
                        cLbl.ForeColor = DefaultForeColor;
                    }
                    ShowTableLayoutPanel(tlpHome, tlpOrder_confirmation);
                }
                else
                {
                    foreach (Control cLbl in btn.Parent.Controls.OfType<Label>())
                    {
                        cLbl.Text = "Mot de passe incorrect";
                        cLbl.ForeColor = Color.Red;
                    }
                }
                break;
            }
             
        }
    }
}
