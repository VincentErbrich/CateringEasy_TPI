/*
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
using System.Windows;
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
         * Load event handler method for this Form. Shows the home TableLayoutPanel and loads images from the database.
         */ 
        private void FormMain_Load(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpHome, tlpHome);
            MenuImages.LoadMenuImages();
            MenuBuilt = false;
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
            tlpMenuList.RowCount = 0;

            //Gets the currency of the pricing to show in the header. If the currency is not specified in the Database, sets the currency to "CHF".
            string Currency = "CHF";
            MySqlDataReader readerCurrency = Db.SqlRequest("SELECT Currency FROM cateasy_bd.Settings;");
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
                //Initializes the header.
                tlpMenuList.Controls.Add(new Label { Text = "Nom" }, 0, 0);
                tlpMenuList.Controls.Add(new Label { Text = "Prix " + Currency }, 1, 0);
                tlpMenuList.Controls.Add(new Label { Text = "Quantité souhaitée" }, 2, 0);
            }
            else
            {
                tlpMenuList.ColumnCount = 3;
                tlpMenuList.ColumnStyles.Insert(0, new ColumnStyle { SizeType = SizeType.Percent, Width = 60 });
                tlpMenuList.ColumnStyles.Insert(1, new ColumnStyle { SizeType = SizeType.Percent, Width = 20 });
                tlpMenuList.ColumnStyles.Insert(2, new ColumnStyle { SizeType = SizeType.Percent, Width = 20 });

                //Initializes the header.
                tlpMenuList.Controls.Add(new Label { Text = "Nom" }, 0, 0);
                tlpMenuList.Controls.Add(new Label { Text = "Prix " + Currency }, 1, 0);
                tlpMenuList.Controls.Add(new Label { Text = "En stock" }, 2, 0);

                //Reads the menu items from the Database, puts them in a MySqlDataReader object, then builds the TableLayoutPanel according to the values in the MySqlDataReader object.
                MySqlDataReader readerMenu = Db.SqlRequest("SELECT IDMenuItem, Name, Price, Remaining FROM cateasy_bd.MenuItem WHERE Remaining > 1");
                try
                {
                    if (readerMenu.HasRows)
                    {
                        while (readerMenu.Read())
                        {
                            tlpMenuList.RowCount++;
                            tlpMenu.RowStyles.Add(new RowStyle { SizeType = SizeType.Absolute, Height = 30 });
                            tlpMenuList.Controls.Add(new Label { Text = readerMenu.GetValue(1).ToString(), Height = 10, Margin = new Thickness}, 0, tlpMenuList.RowCount);
                            tlpMenuList.Controls.Add(new Label { Text = readerMenu.GetValue(2).ToString(), Height = 10 }, 1, tlpMenuList.RowCount);
                            tlpMenuList.Controls.Add(new NumericUpDown { Value = 0, Name = "nudQuantityItem" + readerMenu.GetValue(0), Maximum = readerMenu.GetInt16(3), Minimum = 0, Height = 10 }, 3, tlpMenuList.RowCount);
                        }
                    }
                }
                catch (Exception e)
                {
                    ExceptionManager.NewException(e, "Nous avons rencontré un problème lors de la construction du menu. Veuillez contacter le support technique", true);
                }
            }
        }
        /*private void BuildMenuList(bool edit)
        {
            //If the menu has not already been built, builds the menu.
            if (!MenuBuilt)
            {
                //Initializes the text columns and the header.
                dgvMenu.ColumnCount = 2;
                dgvMenu.Columns[0].Name = "Nom";
                dgvMenu.Columns[1].Name = "Prix";
                dgvMenu.Columns[0].ReadOnly = true;
                dgvMenu.Columns[1].ReadOnly = true;

                //If the menu has to be edited, adds a Textbox column to edit the stock.
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
                    //Gets the currency of the pricing to show in the DataGridView. If the currency is not specified in the Database, sets the currency to "CHF".
                    string Currency = "CHF";
                    MySqlDataReader readerCurrency = Db.SqlRequest("SELECT Currency FROM cateasy_bd.settings;");

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

                    //Reads the menu items from the Database, puts them in a MySqlDataReader object, then builds the DataGridView according to the values in the MySqlDataReader object.
                    MySqlDataReader readerMenu = Db.SqlRequest("SELECT Name, Price FROM cateasy_bd.MenuItem WHERE Remaining > 1");
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

                        //Initializes a new DataGridViewButtonColumn object for the user to add an item to his selection, then inserts it into the DataGridView.
                        DataGridViewButtonColumn dgbAddColumn = new DataGridViewButtonColumn();
                        dgbAddColumn.HeaderText = "Ajouter";
                        dgbAddColumn.Text = "+";
                        dgbAddColumn.UseColumnTextForButtonValue = true;
                        dgvMenu.Columns.Insert(2, dgbAddColumn);

                        //Initializes a new DataGridView¨TextBoxColumn object for the user to see how many times he has selected x menu item, then inserts it into the DataGridView.
                        DataGridViewTextBoxColumn dgtSelected = new DataGridViewTextBoxColumn();
                        dgtSelected.Name = "dgtSelected";
                        dgtSelected.DefaultCellStyle.NullValue = "0";
                        dgtSelected.HeaderText = "";
                        dgtSelected.ReadOnly = true;
                        dgvMenu.Columns.Insert(3, dgtSelected);

                        //Initializes a new DataGridViewButtonColumn object for the user to remove an item from his selection, then inserts it into the DataGridView.
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
                    dgvMenu.CellContentClick += new DataGridViewCellEventHandler(dgvMenu_CellContentClick); //Adds the dgvMenu_CellContentClick from this Form class to the CellContentClick event handler in the DataGridView.
                }
                MenuBuilt = true;   //Sets the MenuBuilt property to indicate the DataGridView has been built.
                tlpMenu.Controls.Add(dgvMenu); //Adds the DataGridView as a child to the restaurant menu TableLayoutPanel.
            }
        }*/
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
         * This method builds the "Cuisine" part of the application 
         */
        private void BuildCuisineHome()
        {
            //Removes an eventual pre existing Accordion from the controls in the "Cuisine" TableLayoutPanel.
            foreach (Accordion acc in tlpCuisine.Controls.OfType<Accordion>())
            {
                tlpCuisine.Controls.Remove(acc);
                break;
            }

            //Initializes the "Cuisine" accordion using the BuildOrdersAccordion method.
            Accordion accCuisine = BuildOrdersAccordion();
            
            MySqlDataReader readerOrder = Db.SqlRequest("SELECT * FROM cateasy_bd.order WHERE Completed = false;"); //Gets the orders awaiting completion from the database and puts them in a MySqlDataReader object.
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

                            MySqlDataReader readerOrderItems = Db.SqlRequest("SELECT IDMenuItem, Name, IDOrder_Mitem FROM cateasy_bd.Order_MItems RIGHT JOIN cateasy_bd.menuitem ON FIDMenuItem = IDMenuItem WHERE 'Completed' IS FALSE AND FIDOrder = " + readerOrder.GetValue(0) + ";"); //Gets the current order details from the MySql database and puts them in a MySqlDataReader object.
                            if (readerOrderItems.HasRows)
                            {
                                int j = 0;
                                TableLayoutPanel tlpCmdItems = new TableLayoutPanel { Width = 800, ColumnCount = 2, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(15), TabStop = false, Name = "tlpCmdItems" + readerOrder.GetValue(0)};

                                //Adds the details of the order to the TableLayoutPanel
                                while (readerOrderItems.Read())
                                {
                                    Button btnDeleteItem = GenerateButton(Color.OrangeRed, "btnDeleteItem_Order" + readerOrder.GetValue(2), "Supprimmer", 8F);
                                    btnDeleteItem.Click += new EventHandler(btnDeleteItem_Click);

                                    tlpCmdItems.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 70 });
                                    tlpCmdItems.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent, Height = 40 });
                                    tlpCmdItems.Controls.Add(new Label { Text = readerOrderItems.GetValue(1).ToString(), Dock = DockStyle.Fill }, 0, j);
                                    tlpCmdItems.Controls.Add(btnDeleteItem, 1, j);
                                    j++;
                                }
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
            tlpWaiterOrdersCompleted.Controls.Clear();
            tlpWaiterOrdersCompleted.RowStyles.Clear();
            tlpWaiterOrdersCompleted.RowCount = 0;

            MySqlDataReader reader = Db.SqlRequest("SELECT FIDTable, IDOrder FROM cateasy_bd.Order WHERE Completed AND !Delivered AND !Paid;"); //Gets the orders awaiting delivery from the MySql database and puts them in a MySqlDataReader object.

            try
            {
                if(reader.HasRows)
                {
                    //Adds the orders awaiting delivery to the TableLayoutPanel.
                    while (reader.Read())
                    {
                        tlpWaiterOrdersCompleted.RowCount++;
                        tlpWaiterOrdersCompleted.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        Button btnDeleteOrder = GenerateButton(Color.OrangeRed, "btnDeleteItem" + reader.GetValue(1), "Supprimmer", 8F);
                        Button btnOrderDelivered = GenerateButton(Color.LightSeaGreen, "btnItemDelivered" + reader.GetValue(1), "Rendue", 8F);
                        btnDeleteOrder.Click += new EventHandler(btnDeleteOrder_Click); //Adds the btnDeleteOrder_Click event handler to the Click event handler in the button.
                        tlpWaiterOrdersCompleted.Controls.Add(new Label { Text = "Commande " + reader.GetValue(1).ToString() + " Table " + reader.GetValue(0).ToString(), AutoSize = true }, 0, tlpWaiterOrdersCompleted.RowCount);
                        tlpWaiterOrdersCompleted.Controls.Add(btnDeleteOrder, 1, tlpWaiterOrdersCompleted.RowCount);
                        tlpWaiterOrdersCompleted.Controls.Add(btnOrderDelivered, 2, tlpWaiterOrdersCompleted.RowCount);
                    }
                }
            }
            catch(Exception e)
            {
                ExceptionManager.NewException(e, "Un problème est survenu lors du chargement des commandes à livrer. Veuillez contacter le support technique", true);
            }
            tlpWaiterOrdersCompleted.Controls.Add(new Label(), 0, tlpWaiterOrdersCompleted.RowCount = tlpWaiterOrdersCompleted.RowCount++); //Adding an empty label at the bottom of the TableLayoutPanel so the last row does not fill the remaining Height in the TableLayoutPanel.
        }

        /*
         * This method builds the TableLayoutPanel containing the orders awaiting delivery.
         */
        private void BuildWaiterDrinksInStandbyTableLayoutPanel()
        {
            //Clears the TableLayoutPanel.
            tlpWaiterDrinksInStandby.Controls.Clear();
            tlpWaiterDrinksInStandby.RowStyles.Clear();
            tlpWaiterDrinksInStandby.RowCount = 0;

            MySqlDataReader reader = Db.SqlRequest("SELECT FIDTable, Name FROM cateasy_bd.order_mitems RIGHT JOIN cateasy_bd.Order ON IDOrder = FIDOrder RIGHT JOIN cateasy_bd.menuitem ON IDMenuItem = FIDMenuItem WHERE IsDrink AND !Delivered AND !Paid; "); //Gets the drinks awaiting delivery from the MySql database and puts them in a MySqlDataReader object. 

            try
            {
                if (reader.HasRows)
                {
                    //Adds the drinks items awaiting delivery to the TableLayoutPanel.
                    while (reader.Read())
                    {
                        tlpWaiterDrinksInStandby.RowCount++;
                        tlpWaiterDrinksInStandby.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                        Button btnDeleteOrder = GenerateButton(Color.OrangeRed, "btnDeleteItem" + reader.GetValue(1), "Supprimmer", 8F);
                        Button btnOrderDelivered = GenerateButton(Color.LightSeaGreen, "btnItemDelivered" + reader.GetValue(1), "Rendue", 8F);
                        btnDeleteOrder.Click += new EventHandler(btnDeleteItem_Click); //Adds the btnDeleteItem_Click event handler to the Click event handler in the button.
                        tlpWaiterDrinksInStandby.Controls.Add(new Label { Text = "Commande " + reader.GetValue(1).ToString() + " Table " + reader.GetValue(0).ToString(), AutoSize = true }, 0, tlpWaiterDrinksInStandby.RowCount);
                        tlpWaiterDrinksInStandby.Controls.Add(btnDeleteOrder, 1, tlpWaiterDrinksInStandby.RowCount);
                        tlpWaiterDrinksInStandby.Controls.Add(btnOrderDelivered, 2, tlpWaiterDrinksInStandby.RowCount);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionManager.NewException(e, "Un problème est survenu lors du chargement des commandes à livrer. Veuillez contacter le support technique", true);
            }
            //Adding an empty label at the bottom of the TableLayoutPanel so the last row does not fill the remaining Height in the TableLayoutPanel
            tlpWaiterDrinksInStandby.Controls.Add(new Label(), 0, tlpWaiterDrinksInStandby.RowCount = tlpWaiterOrdersCompleted.RowCount++);

        }

        /*
         * This method returns a new button according to the the parameters of the method.
         */
        private static Button GenerateButton(Color col, string name, string text, float fontsize)
        {
            return new Button
            {
                Font = new Font("Segoe UI", fontsize, FontStyle.Regular, GraphicsUnit.Point),
                Dock = DockStyle.Fill,
                BackColor = col,
                Name = name,
                Text = text,
                UseVisualStyleBackColor = true,
                FlatStyle = FlatStyle.Flat,
            };
        }

        /*
         * Event handler method for the Click event on dynamically generated delete order buttons. 
         * Updates the database to indicate that the order is no longer to be treated.
         */
        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Db.SqlRequest("UPDATE cateasy_bd.Order SET Completed AND Paid AND Started AND Delivered WHERE IDOrder = " + MessageBox.Show(Regex.Match(btn.Name, @"\d+$").Value.ToString()) + ";");
        }

        /*
         * Event handler method for the Click event on delete item buttons.
         * Updates the MySql database to indicate that the drink item is no longer to be treated.
         */ 
        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Db.SqlRequest("UPDATE cateasy_bd.Order_MItems SET Completed AND Paid WHERE IDOrder_MItem = " + Regex.Match(btn.Name, @"\d+$"));
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
            if(CurrentlyDisplayedTlp.Name == tlpMenu.Name && Waiter)
            {
                ShowTableLayoutPanel(tlpWaiter, CurrentlyDisplayedTlp);
            }
            else if (CurrentlyDisplayedTlp.Name == tlpMenu.Name && !Waiter)
            {
                ShowTableLayoutPanel(tlpTable_select, CurrentlyDisplayedTlp);
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
                    Db.SqlRequest("UPDATE cateasy_bd.Order SET Started WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    UpdateStock(Regex.Match(btn.Name, @"\d+$").Value.ToString());
                    btn.Enabled = false;
                }
                //In the database, sets the order corresponding to the order id in the Button name as being ready for delivery and, if the order was not marked as started previously, calls the UpdateStock method.
                else if (Regex.IsMatch(btn.Name, "btnOrderEnded*"))
                {
                    MySqlDataReader reader = Db.SqlRequest("SELECT Started FROM cateasy_bd.Order WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");

                    if (Convert.ToInt16(reader.GetValue(0)) == 0)
                    {
                        Db.SqlRequest("UPDATE cateasy_bd.Order SET Started, Completed WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                        UpdateStock(Regex.Match(btn.Name, @"\d+$").Value.ToString());
                    }
                    else
                    {
                        Db.SqlRequest("UPDATE cateasy_bd.Order SET Completed WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    }

                    BuildCuisineHome();
                }
                //In the database, sets the order corresponding to the order id in the Button name as not requiering any further treatement in the application.
                else if (Regex.IsMatch(btn.Name, "btnOrderDelete*"))
                {
                    Db.SqlRequest("UPDATE cateasy_bd.order SET Started = true, Completed = true, Paid = true, Delivered = true WHERE IDOrder = " + Regex.Match(btn.Name, @"\d+$").Value.ToString() + ";");
                    BuildCuisineHome(); //Rebuilds the "cuisine" home TableLayoutPanel.
                }
            }
            catch(Exception ex)
            {
                ExceptionManager.NewException(ex, "Nous avons rencontré un problème lors de la mise à jour de la commande. Veuillez contacter le support technique.", false);
            }
        }
        /*
         * This method decrements the stock of the menu items contained in the order corresponding to the idorder parameter.
         */ 
        private static void UpdateStock(string idorder)
        {
            Db.SqlRequest("UPDATE cateasy_bd.Order SET Started = true, Completed = true WHERE IDOrder = " + idorder + ";");
            MySqlDataReader reader = Db.SqlRequest("SELECT FIDMenuItem FROM cateasy_bd.Order LEFT JOIN cateasy_bd.Order_MItems ON IDOrder = FIDOrder WHERE IDOrder = " + idorder + ";");
            try
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Db.SqlRequest("UPDATE cateasy_bd.MenuItem SET Remaining = Remaining - 1 WHERE IDMenuItem = " + reader.GetValue(0) + ";");
                    }
                    reader.Close();
                }
            }
            catch (Exception ex2)
            {
                ExceptionManager.NewException(ex2, "Nous avons rencontré un problème lors de la mise à jour du stock. Veuillez contacter le support technique", true);
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
                MySqlDataReader reader = Db.SqlRequest("SELECT IDTable FROM cateasy_bd.Table ORDER BY IDTable DESC;"); //Gets the tables from the MySql database and puts them in a MySqlDataReader object.

                SelectedTable = Convert.ToInt16(txtTable_selectNumber.Text); //Tries to converts the text in the table selection TextBox to an integer.

                if(reader.HasRows)
                {
                    if(reader.Read())
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
         * Adds a new order to the Database and adds the items the user has selected in the menu DataGridView.
         */ 
        private void btnMenuConfirm_Click(object sender, EventArgs e)
        {
            string valuesToInsert = ""; //Initializing an empty string variable that will contain the menu items to add to the database.
            bool orderCreated = false; //Initializing a Boolean variable indicating if the order has already been added to the databse.
            bool itemSelected = false; //Initializing a Boolean variable indicating if an item has been selected in the DataGridView.
            Int16 orderId = 0; //Inizializing an integer that will contain the inserted order id.
            foreach (DataGridViewRow row in dgvMenu.Rows)
            {
                //Checks if a menu item has been selected in the TextBox DataGridViewRow of the dgvMenu DataGridView.
                if (Convert.ToInt16(row.Cells[3].Value) > 0)
                {
                    //If a menu item has been selected and the order has not been created yet, inserts a new order in the database and saves the autoincremented id of the order in the orderId Int16 varialble.
                    if(!orderCreated)
                    {
                        itemSelected = true; //Sets the itemSelected boolean to indicate that an item has been selected.
                        MySqlDataReader reader = Db.SqlRequest("INSERT INTO cateasy_bd.Order(FIDTable) VALUES(" + SelectedTable.ToString() + "); SELECT LAST_INSERT_ID();");
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

                    //Adds the insert query corresponding to the ordered menu item to add to the database in the valuesToInsert string.
                    for (int i = 0; i < Convert.ToInt16(row.Cells[3].Value.ToString()); i++)
                    {
                        try
                        {
                            //Gets the remaining stock of the item in the current row from the MySql database and puts it in a MySqlDataReader object.
                            MySqlDataReader readerStock = Db.SqlRequest("SELECT Remaining FROM cateasy_bd.MenuItem WHERE IDMenuItem = " + (row.Index + 1) + ";");
                            if (readerStock.Read() && readerStock.HasRows)
                            {
                                //If the current item is in sufficient quantity in the stock, adds an insert query for the current menu item to the valuesToInsert string.
                                if (i < Convert.ToInt16(readerStock.GetValue(0).ToString()))
                                {
                                    valuesToInsert += "INSERT INTO cateasy_bd.Order_MItems(FIDMenuItem, FIDOrder) VALUES(" + (row.Index + 1) + ", " + orderId + ");";
                                }
                                else
                                    lblOrder_confirmation.Text += "\nLe plat N' " + (row.Index + 1).ToString() + " n'a pas pu être commandé. Nous n'en avons plus suffisemment.";
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
            //If one or more item has been selected for order, adds the ordered item(s) to the database builds the order confirmation TableLayoutPanel and calls the ShowTableLayoutPanel method to show it.
            if(itemSelected)
            {
                Db.SqlRequest(valuesToInsert);
                BuildTlpOrderConfirm();
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
                foreach(Control c in tlpOrder_confirmation.Controls)
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
                if(!alreadyBuilt)
                {
                    Button btnReturnToWaiterHome = GenerateButton(DefaultBackColor, "btnReturnToWaiterHome", "Retour", 9F);
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
                    Button btnReturnToHomeProtection = GenerateButton(DefaultBackColor, "btnReturnToHomeProtection", "Retour à l'accueil", 9F);
                    btnReturnToHomeProtection.Location = new Point(540, 368);
                    btnReturnToHomeProtection.Anchor = AnchorStyles.Top;
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
        * Event handler method for the Click event on the return to home Button in the waiter order confirmation TableLayoutPanel. 
        * Shows the waiter home TableLayoutPanel.
        */
        private void btnReturnToHome_Click(object sender, EventArgs e)
        {
            ShowTableLayoutPanel(tlpWaiter, tlpOrder_confirmation);
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
            BuildMenuList(false);
            ShowTableLayoutPanel(tlpMenu, tlpWaiter);
        }

        /*
         * Event handler method for the DatabaseUpdated event handler in the DatabaseManager object.
         * By calling the appropriate methods, rebuilds the currently displayed lists so that they match the new values in the MySql database.
         */ 
        private void DatabaseUpdated(object sender, EventArgs e)
        {
            if(CurrentlyDisplayedTlp == tlpCuisine)
            {
                BuildCuisineHome();
            }
            if(CurrentlyDisplayedTlp == tlpWaiter)
            {
                BuildWaiterHome();
            }
        }
    }
}
