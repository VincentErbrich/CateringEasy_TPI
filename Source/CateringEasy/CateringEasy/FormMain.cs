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
        //DataGridView containing the menu of the restaurant.
        private DataGridView dgvMenu = new DataGridView
        {
            Dock = DockStyle.Fill,
            Anchor = AnchorStyles.Top,
            AutoSize = true,
            Name = "dgvMenu",
            RowHeadersVisible = false,
        };

        //String property indicating from which TableLayoutPanel the display of the currently displayed TableLayoutPanel has been ordered.
        private TableLayoutPanel RedirectedFromTlp { get; set; }

        //String property indicating which TableLayoutPanel is currently displayed to the user.
        private TableLayoutPanel CurrentlyDisplayedTlp { get; set; }

        //Declaring a DatabaseManager object to communicate with the MySql database.
        private static DatabaseManager Db;

        //Boolean property indicating if the user currently completing an order is a waiter or a client.
        private bool Waiter { get; set; }

        //Boolean property indicating if the menu of the restaurant has already been built
        private bool MenuBuilt { get; set; }

        //Integer property indicating which table number was last selected
        private Int16 SelectedTable { get; set; }

        /*
         * Constructor method - Initializes this Form.
         */
        public FormMain()
        {
            InitializeComponent();
            //Instanciating the DatabaseManager object.
            Db = new DatabaseManager();
            //Connecting the DatabaseManager object to the MySql Database
            Db.Connexion();
            //Adds the event DatabaseUpdated event handler from this Form class to the DatabaseUpdated event handler in the DatabaseManager object.
            Db.DatabaseUpdated += DatabaseUpdated;
        }
        /*
         * Load event handler method for this Form. Shows the home TableLayoutPanel.
         */
        private void FormMain_Load(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpHome, tlpHome);
            //MenuImages.LoadMenuImages();
            MenuBuilt = false;
        }

        /*
         * This method decrements the stock of the menu items contained in the order corresponding to the idorder parameter.
         */
        private static void UpdateStock(string idorder)
        {
            //Db.SqlRequestNonQuery("UPDATE cateasy_bd.order SET Started = true WHERE IDOrder = " + idorder + ";", true);
            MySqlDataReader reader = Db.SqlRequest("SELECT FIDMenuItem FROM cateasy_bd.order LEFT JOIN cateasy_bd.order_mitems ON IDOrder = FIDOrder WHERE IDOrder = " + idorder + ";", false);
            try
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Db.SqlRequestNonQuery("UPDATE cateasy_bd.menuitem SET Remaining = Remaining - 1 WHERE IDMenuItem = " + reader.GetValue(0) + ";", false);
                    }
                }
            }
            catch (Exception ex2)
            {
                ExceptionManager.NewException(ex2, "Nous avons rencontré un problème lors de la mise à jour du stock. Veuillez contacter le support technique", true);
            }
        }

        /*
         * This method saves the TableLayoutPanel currently displayed to indicate from which TableLayoutPanel 
         * the display of the currently shown TableLayoutPanel has been requested, saves the TableLayoutPanel to show
         * in order to indicate which TableLayoutPanel is currently Shown, then requests the HideAllPanels method
         * to hide every TableLayoutPanel in the Form, finally showing the TableLayoutPanel requested in parameter tlpToShow.
         */
        private void ShowTableLayoutPanel(TableLayoutPanel tlpToShow, TableLayoutPanel tlpCurrent)
        {
            RedirectedFromTlp = tlpCurrent;
            CurrentlyDisplayedTlp = tlpToShow;
            HideAllPanels();
            tlpToShow.Show();
        }
        /*
         * This method hides all Controls of type TableLayoutPanel contained in FormMain.
         */
        private void HideAllPanels()
        {
            foreach (TableLayoutPanel tlp in Controls)
            {
                tlp.Hide();
            }
        }
        /*
         * This method builds the menu list TableLayoutPanel.
         */
        private void BuildMenuList(bool edit)
        {
            //Resets the TableLayoutPanel
            tlpMenuList.Controls.Clear();
            tlpMenuList.RowStyles.Clear();
            tlpMenuList.ColumnStyles.Clear();
            tlpMenuList.RowCount = 0;

            //Gets the currency of the pricing to show in the header. If the currency is not specified in the Database, sets the currency to "CHF".
            string Currency = "CHF";
            MySqlDataReader readerCurrency = Db.SqlRequest("SELECT Currency FROM cateasy_bd.settings;", false);
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

            if (edit)
            {

            }
            else
            {
                tlpMenuList.ColumnCount = 3;
                tlpMenuList.ColumnStyles.Insert(0, new ColumnStyle { SizeType = SizeType.Percent, Width = 60 });
                tlpMenuList.ColumnStyles.Insert(1, new ColumnStyle { SizeType = SizeType.Percent, Width = 20 });
                tlpMenuList.ColumnStyles.Insert(2, new ColumnStyle { SizeType = SizeType.Percent, Width = 20 });

                tlpMenuList.CellBorderStyle = TableLayoutPanelCellBorderStyle.InsetDouble;

                //Initializes the header.
                tlpMenu.RowStyles.Add(new RowStyle { SizeType = SizeType.Absolute, Height = 50 });
                //Initializes the header.
                tlpMenuList.Controls.Add(new Label { Text = "Nom", AutoSize = false, Height = 48, Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point) }, 0, 0);
                tlpMenuList.Controls.Add(new Label { Text = "Prix " + Currency, AutoSize = false, Height = 48, Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point) }, 1, 0);
                tlpMenuList.Controls.Add(new Label { Text = "En stock", AutoSize = false, Height = 48, Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point) }, 2, 0);

                //Reads the menu items from the Database, puts them in a MySqlDataReader object, then builds the TableLayoutPanel according to the values in the MySqlDataReader object.
                MySqlDataReader readerMenu = Db.SqlRequest("SELECT IDMenuItem, Name, Price, Remaining FROM cateasy_bd.menuitem WHERE Remaining > 1", false);
                try
                {
                    if (readerMenu.HasRows)
                    {
                        while (readerMenu.Read())
                        {
                            tlpMenuList.RowCount++;
                            tlpMenu.RowStyles.Add(new RowStyle { SizeType = SizeType.Absolute, Height = 40 });
                            tlpMenuList.Controls.Add(new Label { Text = readerMenu.GetValue(1).ToString(), Height = 38, AutoSize = true, Font = new Font("Segoe UI Semibold", 12F, FontStyle.Regular, GraphicsUnit.Point) }, 0, tlpMenuList.RowCount);
                            tlpMenuList.Controls.Add(new Label { Text = readerMenu.GetValue(2).ToString(), Height = 38, AutoSize = false, Font = new Font("Segoe UI Semibold", 12F, FontStyle.Regular, GraphicsUnit.Point) }, 1, tlpMenuList.RowCount);
                            tlpMenuList.Controls.Add(new NumericUpDown { Value = 0, Name = "nudQuantityItem" + readerMenu.GetValue(0), Maximum = readerMenu.GetInt16(3), Minimum = 0, Height = 38, Margin = new Padding(0, 0, 0, 0), AutoSize = false, Font = new Font("Segoe UI Semibold", 14F, FontStyle.Regular, GraphicsUnit.Point) }, 3, tlpMenuList.RowCount);
                        }
                    }
                }
                catch (Exception e)
                {
                    ExceptionManager.NewException(e, "Nous avons rencontré un problème lors de la construction du menu. Veuillez contacter le support technique", true);
                }
            }
        }

        /*
         * This method builds the "Cuisine" part of the application 
         */
        private void BuildCuisineHome()
        {
            //Removes an eventual pre existing Accordion from the controls in the "Cuisine" TableLayoutPanel.
            foreach (Accordion acc in tlpCuisine.Controls.OfType<Accordion>())
            {
                tlpCuisine.Controls.Remove(acc);
                acc.Dispose();
                break;
            }

            //Initializes the "Cuisine" accordion using the BuildOrdersAccordion method.
            Accordion accCuisine = BuildOrdersAccordion();

            MySqlDataReader readerOrder = Db.SqlRequest("SELECT * FROM cateasy_bd.order WHERE !Completed;", false); //Gets the orders awaiting completion from the database and puts them in a MySqlDataReader object.
            try
            {
                if (readerOrder.HasRows)
                {
                    int i = 0;
                    try
                    {
                        //For each order, adds a Panel to the accordion containing the details of the order.
                        while (readerOrder.Read())
                        {
                            Panel pnl = new Panel { Dock = DockStyle.Fill };

                            MySqlDataReader readerOrderItems = Db.SqlRequest("SELECT idmenuitem, Name, idorder_mitems FROM cateasy_bd.order_mitems RIGHT JOIN cateasy_bd.menuitem ON FIDMenuItem = IDMenuItem WHERE !Paid AND !IsDrink AND FIDOrder = " + readerOrder.GetValue(0) + ";", false); //Gets the current order details from the MySql database and puts them in a MySqlDataReader object.
                            if (readerOrderItems.HasRows)
                            {
                                int j = 0;
                                TableLayoutPanel tlpCmdItems = new TableLayoutPanel { Width = 800, ColumnCount = 2, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(15), TabStop = false, Name = "tlpCmdItems" + readerOrder.GetValue(0) };

                                //Adds the details of the order to the TableLayoutPanel
                                while (readerOrderItems.Read())
                                {
                                    Button btnDeleteItem = GenerateButton(Color.OrangeRed, "btnDeleteItem" + readerOrderItems.GetValue(2), "Supprimmer", 8F);
                                    btnDeleteItem.Click += new EventHandler(btnDeleteItem_Click);

                                    tlpCmdItems.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 70 });
                                    tlpCmdItems.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent, Height = 40 });
                                    tlpCmdItems.Controls.Add(new Label { Text = readerOrderItems.GetValue(1).ToString(), Dock = DockStyle.Fill }, 0, j);
                                    tlpCmdItems.Controls.Add(btnDeleteItem, 1, j);
                                    j++;
                                }
                                readerOrderItems.Close();
                                readerOrderItems.Dispose();
                                //Adds the TableLayoutPanel containing order details to the main order Panel.
                                pnl.Controls.Add(tlpCmdItems);
                            }

                            //Adds action buttons to a new TableLayoutPanel and adds the TableLayoutPanel to the main Panel.
                            TableLayoutPanel tlpCmdButtons = new TableLayoutPanel { ColumnCount = 3, RowCount = 1, Dock = DockStyle.Bottom, Width = 200 };

                            //Declares and initializes a new button for the user to indicate that he is starting to cook the order.
                            Button btnOrderStarted = GenerateButton(Color.Yellow, "btnOrderStarted" + readerOrder.GetValue(0).ToString(), "Commencer la préparation", 10F);
                            btnOrderStarted.Height = 80;
                            btnOrderStarted.Width = 300;
                            btnOrderStarted.AutoSize = true;
                            btnOrderStarted.Click += new EventHandler(btnOrder_Click); //Adds the btnOrder_Click event handler to the Click event handler in the button.
                            if ((bool)readerOrder.GetValue(4))
                            {
                                btnOrderStarted.Enabled = false;
                            }

                            //Declares and initializes a new button for the user to indicate that he has finished to cook the order.
                            Button btnOrderEnded = GenerateButton(Color.Green, "btnOrderEnded" + readerOrder.GetValue(0).ToString(), "Terminée", 10F);
                            btnOrderEnded.Height = 80;
                            btnOrderEnded.Width = 300;
                            btnOrderEnded.AutoSize = true;
                            btnOrderEnded.Click += new EventHandler(btnOrder_Click); //Adds the btnOrder_Click event handler to the Click event handler in the button.

                            //Declares and initializes a new button for the user to remove the order.
                            Button btnOrderDelete = GenerateButton(Color.OrangeRed, "btnOrderDelete" + readerOrder.GetValue(0).ToString(), "Supprimmer", 10F);
                            btnOrderDelete.Height = 80;
                            btnOrderDelete.Width = 300;
                            btnOrderDelete.AutoSize = true;
                            btnOrderDelete.Click += new EventHandler(btnOrder_Click); //Adds the btnOrder_Click event handler to the Click event handler in the button.

                            //Adds the buttons to the TableLayoutPanel.
                            tlpCmdButtons.Controls.Add(btnOrderStarted, 0, 0);
                            tlpCmdButtons.Controls.Add(btnOrderEnded, 1, 0);
                            tlpCmdButtons.Controls.Add(btnOrderDelete, 2, 0);

                            //Adds the TableLayoutPanel containing the buttons to the main Panel.
                            pnl.Controls.Add(tlpCmdButtons);
                            pnl.Name = "pnlOrder" + readerOrder.GetValue(0).ToString();

                            accCuisine.Add(pnl, "Commande " + readerOrder.GetValue(0).ToString() + " Table " + readerOrder.GetValue(1).ToString(), "", 0, false); //Adds the main Panel to the Accordion.
                            i++;
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionManager.NewException(e, "Erreur lors du chargement des commandes. Veuillez contacter le support technique", false);
                    }
                    readerOrder.Close();
                    readerOrder.Dispose();
                }
            }
            catch (Exception e)
            {
                ExceptionManager.NewException(e, "Erreur lors du chargement des commandes. Veuillez contacter le support technique", false);
            }
            tlpCuisine.Controls.Add(accCuisine, 0, 1);  //Adds the Accordion in the "Cuisine" home TableLayoutPanel.
        }
        /*
         * This method builds the Waiter home. 
         * It requests the BuildWaiterOrdersTableLayoutPanel method to build the TableLayoutPanel containing the orders awaiting delivery and
         * requests the BuildWaiterDrinksInStandbyTableLayoutPanel method to build the TableLayoutPanel containing the drinks items awaiting delivery.
         */
        private void BuildWaiterHome()
        {
            BuildWaiterOrdersTableLayoutPanel();
            BuildWaiterDrinksInStandbyTableLayoutPanel();
        }

        /*
         * This method builds the TableLayoutPanel containing the orders awaiting delivery.
         */
        private void BuildWaiterOrdersTableLayoutPanel()
        {
            //Clears the TableLayoutPanel.
            tlpWaiterOrdersCompleted.RowCount = 0;
            tlpWaiterOrdersCompleted.Controls.Clear();
            tlpWaiterOrdersCompleted.RowStyles.Clear();

            //Build header
            tlpWaiterOrdersCompleted.RowCount++;
            tlpWaiterOrdersCompleted.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpWaiterOrdersCompleted.Controls.Add(new Label { Text = "Commandes en attente de livraison", AutoSize = true, Font = new Font( "Segoe UI Semibold", 10F, FontStyle.Regular, GraphicsUnit.Point) }, 0, tlpWaiterOrdersCompleted.RowCount);

            MySqlDataReader reader = Db.SqlRequest("SELECT FIDTable, IDOrder FROM cateasy_bd.order WHERE Completed AND !Delivered AND !Paid;", false); //Gets the orders awaiting delivery from the MySql database and puts them in a MySqlDataReader object.

            try
            {
                if (reader.HasRows)
                {
                    //Adds the orders awaiting delivery to the TableLayoutPanel.
                    while (reader.Read())
                    {
                        tlpWaiterOrdersCompleted.RowCount++;
                        tlpWaiterOrdersCompleted.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        Button btnDeleteOrder = GenerateButton(Color.OrangeRed, "btnDeleteItem" + reader.GetValue(1), "Supprimmer", 8F);
                        Button btnOrderDelivered = GenerateButton(Color.LightSeaGreen, "btnItemDelivered" + reader.GetValue(1), "Rendue", 8F);
                        btnDeleteOrder.Click += new EventHandler(btnDeleteOrder_Click); //Adds the btnDeleteOrder_Click event handler to the Click event handler in the button.
                        btnOrderDelivered.Click += new EventHandler(btnOrderDelivered_Click); //Adds the btnOrderDelivered_Click event handler to the Click event handler in the button.
                        tlpWaiterOrdersCompleted.Controls.Add(new Label { Text = "Commande " + reader.GetValue(1).ToString() + " Table " + reader.GetValue(0).ToString(), AutoSize = true }, 0, tlpWaiterOrdersCompleted.RowCount);
                        tlpWaiterOrdersCompleted.Controls.Add(btnDeleteOrder, 1, tlpWaiterOrdersCompleted.RowCount);
                        tlpWaiterOrdersCompleted.Controls.Add(btnOrderDelivered, 2, tlpWaiterOrdersCompleted.RowCount);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionManager.NewException(e, "Un problème est survenu lors du chargement des commandes à livrer. Veuillez contacter le support technique", true);
            }
            tlpWaiterOrdersCompleted.RowCount++;
            tlpWaiterOrdersCompleted.Controls.Add(new Label(), 2, tlpWaiterOrdersCompleted.RowCount); //Adding an empty label at the bottom of the TableLayoutPanel so the last row does not fill the remaining Height in the TableLayoutPanel.
        }

        /*
         * This method builds the TableLayoutPanel containing the orders awaiting delivery.
         */
        private void BuildWaiterDrinksInStandbyTableLayoutPanel()
        {
            //Clears the TableLayoutPanel.
            tlpWaiterDrinksInStandby.RowCount = 0;
            tlpWaiterDrinksInStandby.Controls.Clear();
            tlpWaiterDrinksInStandby.RowStyles.Clear();

            //Build header
            tlpWaiterDrinksInStandby.RowCount++;
            tlpWaiterDrinksInStandby.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpWaiterDrinksInStandby.Controls.Add(new Label { Text = "Boissons en attente de livraison", AutoSize = true,Font = new Font("Segoe UI Semibold", 10F, FontStyle.Regular, GraphicsUnit.Point) }, 0, tlpWaiterDrinksInStandby.RowCount);

            MySqlDataReader reader = Db.SqlRequest("SELECT FIDTable, Name, IDOrder_Mitems FROM cateasy_bd.order_mitems RIGHT JOIN cateasy_bd.order ON IDOrder = FIDOrder RIGHT JOIN cateasy_bd.menuitem ON IDMenuItem = FIDMenuItem WHERE IsDrink AND !order_mitems.Delivered AND !order_mitems.Paid; ", false); //Gets the drinks awaiting delivery from the MySql database and puts them in a MySqlDataReader object. 

            try
            {
                if (reader.HasRows)
                {
                    //Adds the drinks items awaiting delivery to the TableLayoutPanel.
                    while (reader.Read())
                    {
                        tlpWaiterDrinksInStandby.RowCount++;
                        tlpWaiterDrinksInStandby.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                        Button btnDeleteItem = GenerateButton(Color.OrangeRed, "btnDeleteItem" + reader.GetValue(2), "Supprimmer", 8F);
                        Button btnItemDelivered = GenerateButton(Color.LightSeaGreen, "btnItemDelivered" + reader.GetValue(2), "Rendue", 8F);
                        btnDeleteItem.Click += new EventHandler(btnDeleteItem_Click); //Adds the btnDeleteItem_Click event handler to the Click event handler in the button.
                        btnItemDelivered.Click += new EventHandler(btnItemDelivered_Click); //Adds the btnItemDelivered_Click event handler to the Click event handler in the button.
                        tlpWaiterDrinksInStandby.Controls.Add(new Label { Text = reader.GetValue(1).ToString() + " Table " + reader.GetValue(0).ToString(), AutoSize = true }, 0, tlpWaiterDrinksInStandby.RowCount);
                        tlpWaiterDrinksInStandby.Controls.Add(btnDeleteItem, 1, tlpWaiterDrinksInStandby.RowCount);
                        tlpWaiterDrinksInStandby.Controls.Add(btnItemDelivered, 2, tlpWaiterDrinksInStandby.RowCount);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionManager.NewException(e, "Un problème est survenu lors du chargement des commandes à livrer. Veuillez contacter le support technique", true);
            }
            //Adding an empty label at the bottom of the TableLayoutPanel so the last row does not fill the remaining Height in the TableLayoutPanel
            tlpWaiterDrinksInStandby.RowCount++;
            tlpWaiterDrinksInStandby.Controls.Add(new Label(), 2, tlpWaiterDrinksInStandby.RowCount);
        }

        /*
         * This method builds the order confirmation TableLayoutPanel.
         */
        private void BuildTlpOrderConfirm()
        {
            // If the current user is a waiter and the TableLayoutPanel has not been built for his usage, builds the order confirmation TableLayoutPanel without password protection.
            if (Waiter)
            {
                bool alreadyBuilt = false;
                lblOrder_confirmation.Text = "Votre commande à bien été reçue";
                foreach (Control c in tlpOrder_confirmation.Controls)
                {
                    //Removes the password protection TableLayoutPanel if it is contained in the order confirmation TableLayoutPanel.
                    if (c.Name == "tlpReturnToHomeProtection")
                    {
                        tlpOrder_confirmation.Controls.Remove(c);
                        break;
                    }
                    //Sets the alreadyBuilt Boolean variable as true if the return Button for the waiter has already been added to the order confirmation TableLayoutPanel.
                    else if (c.Name == "btnWaiterReturn")
                    {
                        alreadyBuilt = true;
                        break;
                    }
                }
                if (!alreadyBuilt)
                {
                    Button btnReturnToWaiterHome = GenerateButton(DefaultBackColor, "btnReturnToWaiterHome", "Retour", 17F);
                    btnReturnToWaiterHome.Location = new Point(3, 948);
                    btnReturnToWaiterHome.Click += new EventHandler(btnReturnToHome_Click); //Adds a new btnReturnToHome_Click event handler to the btnReturnToWaiterHome Click event handler.
                    tlpOrder_confirmation.Controls.Add(btnReturnToWaiterHome, 0, 1);
                }
            }
            //If the current user is a client and the order confirmation TableLayoutPanel has not already been built for his usage, builds the order confirmation TableLayoutPanel with a password protection TableLayoutPanel for the staff parts of the applications.
            else
            {
                bool alreadyBuilt = false;
                foreach (Control c in tlpOrder_confirmation.Controls)
                {
                    //Removes the return to waiter home Button if it is contained in the order confirmation TableLayoutPanel.
                    if (c.Name == "btnWaiterReturn")
                    {
                        tlpOrder_confirmation.Controls.Remove(c);
                        break;
                    }
                    //Sets the alreadyBuilt Boolean variable as true if the password protection TextBox has already been added to the order confirmation TableLayoutPanel.
                    else if (c.Name == "txtReturnToHomeProtection")
                    {
                        c.ResetText();
                        alreadyBuilt = true;
                        break;
                    }

                }
                //If password protection TableLayoutPanel has not been built, initializes a new TableLayoutPanel ands adds the password protection controls to it.
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
                    Button btnReturnToHomeProtection = GenerateButton(DefaultBackColor, "btnReturnToHomeProtection", "Retour à l'accueil", 12F);
                    btnReturnToHomeProtection.Location = new Point(540, 368);
                    btnReturnToHomeProtection.Anchor = AnchorStyles.Top;
                    btnReturnToHomeProtection.Size = new Size(200, 60);
                    btnReturnToHomeProtection.Dock = DockStyle.None;
                    btnReturnToHomeProtection.Click += new EventHandler(btnReturnToHomeProtection_Click); //Adds a new btnReturnToHomeProtection_Click event handler to the Click event in the confirm password Button.

                    tlpReturnToHomeProtection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    tlpReturnToHomeProtection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    tlpReturnToHomeProtection.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                    tlpReturnToHomeProtection.RowStyles.Add(new RowStyle(SizeType.Absolute, 226F));

                    tlpReturnToHomeProtection.Controls.Add(lblReturnToHomeProtection, 0, 0);
                    tlpReturnToHomeProtection.Controls.Add(btnReturnToHomeProtection, 1, 1);
                    tlpReturnToHomeProtection.Controls.Add(txtReturnToHomeProtection, 1, 0);

                    tlpOrder_confirmation.Controls.Add(tlpReturnToHomeProtection, 0, 1); //Adds the password protection TableLayoutPanel to the order confirmation TableLayoutPanel.
                }
            }
        }

        /*
         * Event handler method for the DatabaseUpdated event handler in the DatabaseManager object.
         * By calling the appropriate methods, rebuilds the currently displayed lists so that they match the new values in the MySql database.
         */
        private void DatabaseUpdated(object sender, EventArgs e)
        {
            if (CurrentlyDisplayedTlp == tlpCuisine)
            {
                BuildCuisineHome();
            }
            if (CurrentlyDisplayedTlp == tlpWaiter)
            {
                BuildWaiterHome();
            }
        }

        /*
         * Event handler method managing clicks in the menu DataGridView. 
         */
        private void dgvMenu_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender; //Casts the sender object as a DataGridView

            //Checks that the click was done on a button column.
            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                //If the "Ajouter" button column has been clicked, increases the value in the TextBox situated on the same row representing the number of times the item has been selected.
                if (e.ColumnIndex == 2 && Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) >= 0)
                {
                    int TimesSelected = Convert.ToInt16(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) + 1;
                    senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value = TimesSelected.ToString();
                }
                //If the "Retirer" button column has been clicked, decreases the value in the TextBox situated on the same row representing the number of times the item has been selected.
                else if (e.ColumnIndex == 4 && Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) > 0)
                {
                    int TimesSelected = Convert.ToInt16(senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value) - 1;
                    senderGrid.Rows[e.RowIndex].Cells["dgtSelected"].Value = TimesSelected.ToString();
                }
            }
        }

        /*
         * Event handler method for the Click event on dynamically generated delete order buttons. 
         * Updates the database to indicate that the order is no longer to be treated.
         */
        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Db.SqlRequestNonQuery("UPDATE cateasy_bd.order SET Completed = true, Paid = true, Started = true, Delivered = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";", true);
        }

        /*
         * Event handler method for the Click event on dynamically generated delete order buttons. 
         * Updates the database to indicate that the order is no longer to be treated.
         */
        private void btnOrderDelivered_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Db.SqlRequestNonQuery("UPDATE cateasy_bd.order SET Delivered = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";", true);
        }

        /*
         * Event handler method for the Click event on delete item buttons.
         * Updates the MySql database to indicate that the drink item is no longer to be treated.
         */
        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Db.SqlRequestNonQuery("UPDATE cateasy_bd.order_mitems SET Delivered = true, Paid = true WHERE IDOrder_Mitems = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";", true);
        }

        /*
         * Event handler method for the Click event on drink item delivered buttons.
         * Updates the MySql database to indicate that the drink item is no longer to be delivered.
         */
        private void btnItemDelivered_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Db.SqlRequestNonQuery("UPDATE cateasy_bd.order_mitems SET Delivered = true WHERE IDOrder_Mitems = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";", true);
        }

        /*
         * Event handler method for the click event on the settings PictureBox. 
         * Shows the settings TableLayoutPanel.
         */
        private void pctHomeSettings_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpSettings, tlpHome);
        }

        /*
         * Event handler method for the click event on the waiter button in the home TableLayoutPanel. 
         * Sets the Waiter as true, requests the BuildWaiterHome method to build the waiter home TableLayoutPanel property aShows the waiter home TableLayoutPanel.
         */
        private void btnHomeWaiter_Click(object sender, EventArgs e)
        {
            Waiter = true;
            BuildWaiterHome();
            ShowTableLayoutPanel(tlpWaiter, tlpHome);
        }
        /*
         * Event handler method for the click event on the client button in the home TableLayoutPanel. Sets the Waiter property as false and shows the table selection TableLayoutPanel.
         */
        private void btnHomeClient_Click(object sender, EventArgs e)
        {
            Waiter = false;
            ShowTableLayoutPanel(tlpTable_select, tlpHome);
        }
        /*
         * Event handler method for the click event on the "cuisine" button in the home TableLayourPanel. 
         * Requests the BuildCuisineHome method to build the "cuisine" home TableLayoutPanel and shows the cuisine home TableLayoutPanel.
         */
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
            if (CurrentlyDisplayedTlp.Name == tlpMenu.Name && Waiter)
            {
                ShowTableLayoutPanel(tlpWaiter, CurrentlyDisplayedTlp);
            }
            else if (CurrentlyDisplayedTlp.Name == tlpMenu.Name)
            {
                ShowTableLayoutPanel(tlpTable_select, CurrentlyDisplayedTlp);
            }
            else if (CurrentlyDisplayedTlp.Name == tlpTable_select.Name && Waiter)
            {
                ShowTableLayoutPanel(tlpWaiter, tlpTable_select);

            }
            else if (CurrentlyDisplayedTlp.Name == tlpTable_select.Name && !Waiter)
            {
                ShowTableLayoutPanel(tlpHome, tlpTable_select);

            }
            else if (CurrentlyDisplayedTlp.Name == tlpBilling.Name)
            {
                ShowTableLayoutPanel(tlpWaiter, tlpBilling);

            }
            else if (CurrentlyDisplayedTlp.Name == tlpWaiter.Name)
            {
                ShowTableLayoutPanel(tlpHome, tlpWaiter);

            }
            else
                ShowTableLayoutPanel(RedirectedFromTlp, CurrentlyDisplayedTlp);
        }

        /*
         * Event handler method for Click event on buttons generated dynamically in the Cuisine home TableLayoutPanel.
         * Depending on which button is clicked, marks an order as started, marks an order as ended or deletes the order.
         */
        private void btnOrder_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button; //Casts the sender object as a Button object.

            try
            {
                //In the database, sets the order corresponding to the order id in the Button name as being currently prepared and calls the UpdateStock method.
                if (Regex.IsMatch(btn.Name, "btnOrderStarted*"))
                {
                    UpdateStock(Regex.Match(btn.Name, @"\d+$").Value.ToString());
                    Db.SqlRequestNonQuery("UPDATE cateasy_bd.order SET Started = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";", true);
                    btn.Enabled = false;
                }
                //In the database, sets the order corresponding to the order id in the Button name as being ready for delivery and, if the order was not marked as started previously, calls the UpdateStock method.
                else if (Regex.IsMatch(btn.Name, "btnOrderEnded*"))
                {
                    MySqlDataReader reader = Db.SqlRequest("SELECT Started FROM cateasy_bd.order WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";", false);

                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            if (Convert.ToInt16(reader.GetValue(0)) == 0)
                            {
                                UpdateStock(Regex.Match(btn.Name, @"\d+$").Value.ToString());
                                Db.SqlRequestNonQuery("UPDATE cateasy_bd.order SET Started = true, Completed = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";", true);

                            }
                            else
                            {
                                Db.SqlRequestNonQuery("UPDATE cateasy_bd.order SET Completed = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";", true);
                            }
                        }
                    }

                }
                //In the database, sets the order corresponding to the order id in the Button name as not requiering any further treatement in the application.
                else if (Regex.IsMatch(btn.Name, "btnOrderDelete*"))
                {
                    Db.SqlRequestNonQuery("UPDATE cateasy_bd.order SET Started = true, Completed = true, Paid = true, Delivered = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";", true);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.NewException(ex, "Nous avons rencontré un problème lors de la mise à jour de la commande. Veuillez contacter le support technique.", false);
            }
        }

        /*
         * Event handler method for Click event on the confirm table number button
         * Redirects to the restaurant menu and saves the table number.
         */
        private void btnTable_selectConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                MySqlDataReader reader = Db.SqlRequest("SELECT IDTable FROM cateasy_bd.table ORDER BY IDTable DESC;", false); //Gets the tables from the MySql database and puts them in a MySqlDataReader object.

                SelectedTable = Convert.ToInt16(txtTable_selectNumber.Text); //Tries to converts the text in the table selection TextBox to an integer.

                if (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        //Checks if the input table number is smaller or equal to the final table number in the database. If not, shows an error message.
                        if (Convert.ToInt16(reader.GetValue(0)) >= SelectedTable)
                        {
                            //Resets the text of the controls in the table select TableLayoutPanel and calls the BuildMenuList method.
                            txtTable_selectNumber.Text = "";
                            lblTable_selectError.Text = "";
                            BuildMenuList(false);
                            ShowTableLayoutPanel(tlpMenu, tlpTable_select); //Shows the restaurant menu TableLayoutPanel.
                        }
                        else
                            lblTable_selectError.Text = "Cette table n'existe pas";
                    }
                }
                // If no tables exist in the database, displays an error message.
                else
                {
                    lblTable_selectError.Text = "Veuillez ajouter une table dans les réglages";
                }
            }
            //If the text property in the table selection Textbox is not convertible to an integer, displays an error message. 
            catch
            {
                lblTable_selectError.Text = "Veuillez entrer un nombre";
            }

        }

        /*
         * Event handler method for the Click event on the confirm order button in the restaurant menu TableLayoutPanel.
         * Adds a new order to the Database and adds the items the user has selected in the menu TableLayoutPanel.
         */
        private void btnMenuConfirm_Click(object sender, EventArgs e)
        {
            bool orderCreated = false; //Initializing a Boolean variable indicating if the order has already been added to the databse.
            bool itemSelected = false; //Initializing a Boolean variable indicating if an item has been selected in the DataGridView.
            Int16 orderId = 0; //Inizializing an integer that will contain the inserted order id.
            foreach (Control c in tlpMenuList.Controls)
            {
                if (c is NumericUpDown)
                {
                    NumericUpDown nud = c as NumericUpDown;
                    //If a menu item has been selected and the order has not been created yet, inserts a new order in the database and saves the autoincremented id of the order in the orderId Int16 varialble.
                    if (Convert.ToInt16(nud.Value) > 0)
                    {
                        if (!orderCreated)
                        {
                            MySqlDataReader reader = Db.SqlRequest("INSERT INTO cateasy_bd.order(FIDTable) VALUES(" + SelectedTable.ToString() + "); SELECT LAST_INSERT_ID();", true);
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
                        }
                        //Inserts the selected items(s) into the database.
                        for (int i = 0; i < Convert.ToInt16(nud.Value.ToString()); i++)
                            Db.SqlRequestNonQuery("INSERT INTO cateasy_bd.order_mitems(FIDMenuItem, FIDOrder) VALUES(" + (Regex.Match(nud.Name, @"\d+$").Value.ToString()) + ", " + orderId + ");", true);
                        itemSelected = true; //Sets the itemSelected boolean to indicate that an item has been selected.
                    }
                }

            }
            //If no item has been selected for order, asks the user to select an item.
            if (itemSelected)
            {
                //Adds the ordered item to the database builds the order confirmation TableLayoutPanel and calls the ShowTableLayoutPanel method to show it.
                BuildTlpOrderConfirm();
                ShowTableLayoutPanel(tlpOrder_confirmation, tlpMenu);
                btnMenuConfirm.Text = "Confirmer";
                btnMenuConfirm.ForeColor = DefaultForeColor;
            }
            else
            {
                btnMenuConfirm.Text = "Veuillez ajouter un élément à votre sélection puis re-cliquez";
                btnMenuConfirm.ForeColor = Color.Red;
            }
        }

        /*
         * Event handler method for the Click event on the return to home Button in the waiter order confirmation TableLayoutPanel. 
         * Shows the waiter home TableLayoutPanel.
         */
        private void btnReturnToHome_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpHome, tlpOrder_confirmation);
        }
        /*
        * Event handler method for the Click event on the return to home Button in the client order confirmation TableLayoutPanel. 
        * If the password in the password TextBox corresponds the the one stored in the MySql database, shows the home TableLayoutPanel.
        */
        private void btnReturnToHomeProtection_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button; //Casts the sender object as a Button object.
            foreach (Control cTxt in btn.Parent.Controls.OfType<TextBox>())
            {
                //If the password is correct, shows the home TableLayoutPanel.
                if (PasswordCheck.PasswordIsCorrect(cTxt.Text))
                {
                    foreach (Control cLbl in tlpOrder_confirmation.Controls.OfType<Label>())
                    {
                        //Resets the labels in case they previously displayed an error.
                        cLbl.Text = "Mot de passe \r\n(Pour le personnel)";
                        cLbl.ForeColor = DefaultForeColor;
                    }
                    ShowTableLayoutPanel(tlpHome, tlpOrder_confirmation);
                }
                //If the passowrd is incorrect, displays an error in the password Label.
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
        /*
         * Event handler method for the Click event on the new order button in the waiter home TableLayoutPanel.
         * Sets the Waiter property as true to indicate that the current user is a waiter.
         * Builds an shows the restaurant menu TableLayoutPanel.
         */
        private void btnWaiterNewOrder_Click(object sender, EventArgs e)
        {
            Waiter = true;
            ShowTableLayoutPanel(tlpTable_select, tlpWaiter);
        }

        /*
         * Event handler method for Click event on the "Facturer une table" Button in the waiter home TableLayoutPanel.
         */
        private void tlpWaiterNewBill_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpBilling, tlpWaiter);
        }

        private void btnBillingConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                SelectedTable = Convert.ToInt16(txtBillingTableSelect.Text); //Tries to converts the text in the table selection TextBox to an integer.

                MySqlDataReader readerTable = Db.SqlRequest("SELECT IDTable FROM cateasy_bd.table ORDER BY IDTable DESC;", false); //Gets the tables from the MySql database and puts them in a MySqlDataReader object.

                if (readerTable.HasRows)
                {
                    if (readerTable.Read())
                    {
                        //Checks if the input table number is smaller or equal to the final table number in the database. If not, shows an error message.
                        if (Convert.ToInt16(readerTable.GetValue(0)) >= SelectedTable)
                        {
                            //Resets the text of the controls in the table select TableLayoutPanel and calls the BuildMenuList method.
                            txtBillingTableSelect.Text = "";
                            lblBillingTableSelectError.Text = "";
                        }
                        else
                        {
                            lblBillingTableSelectError.Text = "Cette table n'existe pas";
                            return;
                        }
                    }
                }
                // If no tables exist in the database, displays an error message.
                else
                {
                    lblBillingTableSelectError.Text = "Veuillez ajouter une table dans les réglages";
                    return;
                }
            }
            //If the text property in the table selection Textbox is not convertible to an integer, displays an error message. 
            catch
            {
                lblBillingTableSelectError.Text = "Veuillez entrer un nombre";
                return;
            }

            try
            {
                Int16 ClientsNumber = Convert.ToInt16(txtBillingClientsNumber.Text); //Tries to converts the text in the client(s) number selection TextBox to an integer.

                MySqlDataReader readerOrder = Db.SqlRequest("SELECT IDOrder, Name, Price FROM cateasy_bd.order INNER JOIN cateasy_bd.order_mitems ON IDOrder = FIDOrder RIGHT JOIN cateasy_bd.menuitem ON FIDMenuItem = IDMenuItem WHERE !order.Paid AND !order_mitems.Paid AND FIDTable = " + SelectedTable.ToString() + ";", false);
                if (readerOrder.HasRows)
                {
                    Invoicer invoicer = new Invoicer();
                    bool orderIdSaved = false;
                    int orderId = 0;
                    for (Int16 i = 0; i < ClientsNumber; i++)
                    {
                        List<string> linesToBill = new List<string>();
                        float total = 0;
                        int j = 0;
                        while (readerOrder.Read())
                        {
                            if (readerOrder.GetInt32(0) == orderId || j == 0)
                            {
                                total += readerOrder.GetFloat(2) / ClientsNumber;
                                if (!orderIdSaved)
                                {
                                    orderId = readerOrder.GetInt32(0);
                                }
                                linesToBill.Add(readerOrder.GetValue(1).ToString() + " " + (readerOrder.GetFloat(2) / ClientsNumber).ToString());
                            }
                            else
                                break;
                            j++;
                        }
                        invoicer.NewInvoice(linesToBill, total, "CHF", "CateringEasy_Order" + orderId + "_InvoiceBill" + (i + 1).ToString());
                        ShowTableLayoutPanel(tlpWaiter, tlpBilling); //Shows the waiter home TableLayoutPanel.
                    }
                    //Resets the text of the controls in the client(s) number TableLayoutPanel.
                    txtBillingClientsNumber.Text = "";
                    lblBillingClientsNumberError.Text = "";
                }
                // If no tables exist in the database, displays an error message.
                else
                {
                    lblBillingClientsNumberError.Text = "Cette table n'a pas de commandes a facturer";
                    return;
                }
            }
            //If the text property in the client(s) number selection Textbox is not convertible to an integer, displays an error message. 
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                lblBillingClientsNumberError.Text = "Veuillez entrer un nombre";
                return;
            }
        }

        /*
         * This method returns a new Accordion Control object.
         */
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
        /*
         * This method returns a new button according to the the parameters of the method.
         */
        private static Button GenerateButton(Color col, string name, string text, float fontsize)
        {
            return new Button
            {
                Font = new Font("Segoe UI Semibold", fontsize, FontStyle.Regular, GraphicsUnit.Point),
                Dock = DockStyle.Fill,
                BackColor = col,
                Name = name,
                Text = text,
                UseVisualStyleBackColor = true,
                FlatStyle = FlatStyle.Flat,
            };
        }
    }
}
