using System;   
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapfor.Net.Ui;
using Dapfor.Net.Formats;
using Dapfor.Net.Theming;
using Dapfor.Net.Data;
using Dapfor.Net.Editors;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace MasterPiece
{
    public partial class MASSGUI : Form
    {
        //PNetInvestor NetInvestorProvider;
        //PMicexBridge MicexBridgeProvider;
        //PQuikODBC QuikODBCProvider;
        PQuikDDE QuikDDEProvider;

        Disruptor<UnifiedEvent> ProvidersDisruptor;

        public static SArbitrage T0_T2_Arbitrage;
        public static SCurrencyHedger CurHedge;
        public static SLevelTrader LevelTrader;
        public static SLevelTrader1 LevelTrader1;
        public static SLevelTrader2 LevelTrader2;
        public static SLevelTrader3 LevelTrader3;
        public static SLevelTrader4 LevelTrader4;
        public static SAntiTilt AntiTilt;
        public static SAntiTilt1 AntiTilt1;
        public static SAntiTilt2 AntiTilt2;
        public static SAntiTilt3 AntiTilt3;
        public static SAntiTilt4 AntiTilt4;

        public static bool DoWork = true;

        private Grid providersGrid;
        public static ThreadSafeBindingList<Provider> providersBindingList;

        private Grid strategiesGrid;
        public static ThreadSafeBindingList<Strategy> strategiesBindingList;

        private Grid ordersGrid;
        public static ThreadSafeBindingList<Order> ordersBindingList;

        private Grid instrumentsGrid;
        public static ThreadSafeBindingList<Instrument> instrumentsBindingList;

        private Grid positionsGrid;

        public static TabControl tabs;

        public MASSGUI()
        {
            InitializeComponent();

            C.Init();
            OrderManager.PreTradeControl.InitControl(70, 60, 1000, 15, true);

            LogClient.Start();
            if (!LogClient.Connected)
                System.Windows.Forms.MessageBox.Show("Check the log server.\nNo logging during this run is available.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
            LogClient.Add(string.Format("[Inf][GUI] started at {0}", DateTime.Now.ToString()));

            tabs = new TabControl();
            this.Controls.Add(tabs);
            tabs.Dock = System.Windows.Forms.DockStyle.Fill;

            tabs.TabPages.Add("General", "General");

            tabs.TabPages["General"].Controls.Add(AllowTradingCheckBox);
            tabs.TabPages["General"].Controls.Add(button3);
            tabs.TabPages["General"].Controls.Add(button4);
            tabs.TabPages["General"].Controls.Add(button1);

            int baseXposition = 100;

            providersGrid = new Grid();
            providersGrid.Parent = tabs.TabPages["General"];
            providersGrid.BackColor = SystemColors.Window;
            providersGrid.Highlighting.Enabled = false;
            providersGrid.Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            providersGrid.Location = new Point(baseXposition, 5);
            providersGrid.Name = "Providers Grid";
            providersGrid.Size = new Size(100, 120);
            //this.Controls.Add(this.providersGrid);
            providersGrid.Headers.Add(new Header());
            providersGrid.Headers[0].Add(new Column("Name", "Provider Name"));
            providersGrid.Headers[0].Add(new Column("Status", "Status"));
            providersGrid.Headers[0].Add(new Column("Toggle", "Toggle On\\Off"));
            providersGrid.Headers[0]["Toggle"].Editable = true;
            providersGrid.Headers[0]["Toggle"].Editor = new CheckBoxEditor();
            providersGrid.Headers[0]["Toggle"].Width = 30;
            providersGrid.Headers[0].Add(new Column("TransAmnt", "# of Transactions"));
            providersGrid.Headers[0].Add(new Column("ActiveOrdersAmnt", "# of Active Orders"));
            providersGrid.Headers[0].Add(new Column("TotalOrdersAmnt", "# of Total Orders"));
            providersGrid.Headers[0].StretchMode = ColumnStretchMode.All;
            providersBindingList = new ThreadSafeBindingList<Provider>();
            providersGrid.DataSource = providersBindingList;
            providersGrid.MouseDown += onProviderGridMouseDown;

            strategiesGrid = new Grid();
            strategiesGrid.Parent = this;
            strategiesGrid.BackColor = SystemColors.Window;
            strategiesGrid.Highlighting.Enabled = false;
            //strategiesGrid.Highlighting.Color = System.Drawing.Color.FromArgb(200, 10, 36, 106);
            strategiesGrid.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            strategiesGrid.Location = new Point(baseXposition, 130);
            strategiesGrid.Name = "Strategies Grid";
            strategiesGrid.Size = new Size(100, 100);
            //this.Controls.Add(this.strategiesGrid);
            strategiesGrid.Headers.Add(new Header());
            strategiesGrid.Headers[0].Add(new Column("Name", "Strategy Name"));
            strategiesGrid.Headers[0].Add(new Column("Status", "Status"));
            strategiesGrid.Headers[0].Add(new Column("Toggle", "Toggle On\\Off"));
            strategiesGrid.Headers[0]["Toggle"].Editable = true;
            strategiesGrid.Headers[0]["Toggle"].Editor = new CheckBoxEditor();
            strategiesGrid.Headers[0]["Toggle"].Width = 30;
            strategiesGrid.Headers[0].Add(new Column("MayWork", "Allow New Orders"));
            strategiesGrid.Headers[0]["MayWork"].Editable = true;
            strategiesGrid.Headers[0]["MayWork"].Editor = new CheckBoxEditor();
            strategiesGrid.Headers[0]["MayWork"].Width = 30;
            strategiesGrid.Headers[0].Add(new Column("Provider", "Provider Name"));
            strategiesGrid.Headers[0].Add(new Column("ActiveOrdersAmnt", "Amount of Active Orders"));
            strategiesGrid.Headers[0].Add(new Column("KillAll", "Kill All and Stop"));
            strategiesGrid.Headers[0]["KillAll"].Editable = true;
            strategiesGrid.Headers[0].StretchMode = ColumnStretchMode.All;
            strategiesBindingList = new ThreadSafeBindingList<Strategy>();
            strategiesGrid.DataSource = strategiesBindingList;
            //strategiesGrid.MouseDown += onProviderGridMouseDown;

            ordersGrid = new Grid();
            ordersGrid.Parent = this;
            ordersGrid.BackColor = SystemColors.Window;
            ordersGrid.Highlighting.Color = System.Drawing.Color.FromArgb(200, 10, 36, 106);
            ordersGrid.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            ordersGrid.Location = new Point(5, 5);//195);
            ordersGrid.Name = "Orders Grid";
            ordersGrid.Size = new Size(100, 300);
            //this.Controls.Add(this.ordersGrid);
            ordersGrid.Headers.Add(new Header());
            ordersGrid.Headers[0].Add(new Column("Provider", "Provider Name"));
            ordersGrid.Headers[0].Add(new Column("StrategyName", "Strategy Name"));
            ordersGrid.Headers[0].Add(new Column("InternalID", "ID"));
            ordersGrid.Headers[0]["InternalID"].Width = 30;
            ordersGrid.Headers[0].Add(new Column("StringStatus", "Status"));
            ordersGrid.Headers[0].Add(new Column("Code", "Instrument"));
            ordersGrid.Headers[0].Add(new Column("Class", "Class"));
            ordersGrid.Headers[0].Add(new Column("Price", "Price"));
            ordersGrid.Headers[0].Add(new Column("Quantity", "Quantity"));
            ordersGrid.Headers[0].Add(new Column("ActiveQuantity", "Active Quantity"));
            ordersGrid.Headers[0].Add(new Column("IsHeir", "IsHeir?"));
            ordersGrid.Headers[0].Add(new Column("Kill", "Kill Order"));
            ordersGrid.Headers[0]["Kill"].Editable = true;
            ordersGrid.Headers[0].StretchMode = ColumnStretchMode.All;
            ordersGrid.Headers[0].GroupingEnabled = true;
            ordersGrid.Headers[0]["Provider"].Grouped = true;
            ordersGrid.Headers[0]["StrategyName"].Grouped = true;
            ordersGrid.Headers[0]["StringStatus"].Grouped = true;
            ordersGrid.Headers[0]["Provider"].Visible = false;
            ordersGrid.Headers[0]["StrategyName"].Visible = false;
            ordersGrid.Headers[0]["StringStatus"].Visible = false;
            ordersBindingList = new ThreadSafeBindingList<Order>();
            ordersGrid.DataSource = ordersBindingList;

            positionsGrid = new Grid();
            positionsGrid.Parent = this;
            positionsGrid.BackColor = SystemColors.Window;
            positionsGrid.Highlighting.Color = System.Drawing.Color.FromArgb(200, 10, 36, 106);
            positionsGrid.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            positionsGrid.Location = new Point(5, 5);
            positionsGrid.Name = "Provider's Grid";
            positionsGrid.Size = new Size(100, this.Height - 20);
            //this.Controls.Add(this.positionsGrid);
            positionsGrid.Headers.Add(new Header());
            positionsGrid.Headers[0].Add(new Column("Strategy", "Strategy Name"));
            positionsGrid.Headers[0].Add(new Column("Code", "Instrument"));
            positionsGrid.Headers[0].Add(new Column("Class", "Class"));
            positionsGrid.Headers[0].Add(new Column("Position", "Position"));
            positionsGrid.Headers[0]["Position"].Editable = true;
            positionsGrid.Headers[0]["Position"].Editor = new myTextEditor();
            positionsGrid.Headers[0].StretchMode = ColumnStretchMode.All;
            positionsGrid.ValidateCell += delegate(object sender, ValidateCellEventArgs e)
            {
                if (e.ErrorText != null)
                    e.Action = ValidateCellAction.CancelValue;
            };

            tabs.TabPages["General"].Controls.Add(providersGrid);
            tabs.TabPages["General"].Controls.Add(strategiesGrid);

            tabs.TabPages.Add("Orders", "Orders");
            tabs.TabPages["Orders"].Controls.Add(ordersGrid);
            ordersGrid.Dock = System.Windows.Forms.DockStyle.Fill;

            tabs.TabPages.Add("Positions", "Positions");
            tabs.TabPages["Positions"].Controls.Add(positionsGrid);
            positionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;

            instrumentsGrid = new Grid();
            instrumentsGrid.Parent = this;
            instrumentsGrid.BackColor = SystemColors.Window;
            instrumentsGrid.Highlighting.Color = System.Drawing.Color.FromArgb(200, 10, 36, 106);
            instrumentsGrid.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            instrumentsGrid.Location = new Point(5, 5);//195);
            instrumentsGrid.Name = "Instruments Grid";
            instrumentsGrid.Size = new Size(100, 300);
            //this.Controls.Add(this.ordersGrid);
            instrumentsGrid.Headers.Add(new Header());
            instrumentsGrid.Headers[0].Add(new Column("InstrumentName", "Instrument Name"));
            instrumentsGrid.Headers[0].Add(new Column("InstrumentClass", "Instrument Class"));
            instrumentsGrid.Headers[0].Add(new Column("InstrumentProvider", "Instrument Provider"));
            instrumentsGrid.Headers[0].Add(new Column("InstrumentL1Bid", "L1 Bid"));
            instrumentsGrid.Headers[0].Add(new Column("InstrumentL1Ask", "L1 Ask"));
            instrumentsGrid.Headers[0].StretchMode = ColumnStretchMode.All;
            instrumentsBindingList = new ThreadSafeBindingList<Instrument>();
            instrumentsGrid.DataSource = instrumentsBindingList;

            tabs.TabPages.Add("Instruments", "Instruments");
            tabs.TabPages["Instruments"].Controls.Add(instrumentsGrid);
            instrumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;


            new Thread(() =>
            {
                
                ProvidersDisruptor = new Disruptor<UnifiedEvent>(6, 1024 * 8, Provider.EventsTosser, 1);
                ProvidersDisruptor.Start();
                QuikDDEProvider = new PQuikDDE(ProvidersDisruptor);
                //QuikODBCProvider = new PQuikODBC(ProvidersDisruptor);
                //NetInvestorProvider = new PNetInvestor(ProvidersDisruptor);
                //MicexBridgeProvider = new PMicexBridge(ProvidersDisruptor);

                //T0_T2_Arbitrage = new SArbitrage();
                //CurHedge = new SCurrencyHedger();
                //LevelTrader = new SLevelTrader();

                OrderManager.AllowTrading = AllowTradingCheckBox.Checked;

                while (DoWork)
                {
                    Thread.Sleep(1000);
                }

            }).Start();

            
        }

        #region Grid Appearance
        private void onProviderGridMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //HitTestInfo hi = oldgrid.HitTest(e.Location);
                //if (hi == HitTestInfo.Cell)
                //{
                //    Row row = oldgrid.HitTests.RowTest(e.Location);
                //    oldgrid.FocusedRow = row;
                //    ContextMenuStrip m = new ContextMenuStrip();
                //    DisplayInGrid x = row.DataObject as DisplayInGrid;
                    
                //    //m.Items.Add("Test1", null, delegate { TestMenuClick(e.Location); });
                //    m.Items.Add("Test:");
                //    m.Items[0].Click += delegate { Debug.WriteLine("123");};
                //    m.Items.Add(new ToolStripSeparator());

                //    foreach (Row xxx in oldgrid.Selection)
                //    {
                //        DisplayInGrid x2 = xxx.DataObject as DisplayInGrid;
                //        m.Items.Add(x2.Code);
                //        x2.Code += "1";
                //    }

                //    //m.Items.Add(x.Code);
                //    m.Items.Add(new ToolStripSeparator());
                //    //m.Items.Add(hi.ToString());
                //    m.Show(oldgrid, e.Location);
                //}
            }
        }

        class myTextEditor : UITypeEditorEx
        {
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.None;
            }

            public override StopEditReason EditCell(IGridEditorService service, Cell cell, StartEditReason reason)
            {
                using (TextBox tb = new TextBox())
                {
                    tb.KeyPress += delegate(object sender, KeyPressEventArgs e)
                    {
                        if ((Keys)e.KeyChar == Keys.Enter)
                        {
                            if (cell.DataField.FieldType.FullName=="System.Double")
                                cell.Value = Convert.ToDouble(tb.Text);
                            else
                                cell.Value = tb.Text;// _newvalue_;

                            
                            service.CloseCellControl(StopEditReason.UserStop);
                        }
                    };

                    tb.Text = cell.Value.ToString();
                    return service.CellEditControl(tb, cell.VirtualBounds, reason);
                }
            }
        }
        #endregion

        private void Form_MainGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            DoWork = false;

            try
            {
                T0_T2_Arbitrage.SavePositions();
            }
            catch
            { }

            if (CurHedge != null)
                CurHedge.Stop();

            //if (NetInvestorProvider != null)
            //    if (NetInvestorProvider.IsConnected)
            //        NetInvestorProvider.Disconnect();

            //if (MicexBridgeProvider != null)
            //    if (MicexBridgeProvider.IsConnected)
            //        MicexBridgeProvider.Disconnect();

            //if (QuikODBCProvider != null)
            //    QuikODBCProvider.Stop();

            if (QuikDDEProvider != null)
                QuikDDEProvider.Stop();

            if (LogClient.Connected)
                LogClient.Stop();

            C.TRADE_CONTROL_ONLINE = false;
        }

      

        private void button3_Click(object sender, EventArgs e)
        {
            // Kill All
            Thread T = new Thread(() =>
            {
                for (int i = 0; i < C.AllOrdersByID.Keys.Count; i++)
                {
                    if (C.AllOrdersByID[C.AllOrdersByID.Keys.ElementAt(i)].Status == OrderStatus.active)
                    {
                        LogClient.Add(string.Format("[Inf][GUI] Killing all orders => order #{0} for active {1} with current status {2}", 
                            C.AllOrdersByID.Keys.ElementAt(i),
                            (string)C.AllOrdersByID[C.AllOrdersByID.Keys.ElementAt(i)].Instrument.LevelI[EData.Code],
                            C.AllOrdersByID[C.AllOrdersByID.Keys.ElementAt(i)].Status.ToString()));
                        OrderManager.KillOrder(C.AllOrdersByID[C.AllOrdersByID.Keys.ElementAt(i)]);
                        Thread.Sleep(70);
                    }
                }

                if (T0_T2_Arbitrage != null)
                    T0_T2_Arbitrage.Stop();
            });
            T.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Kill All Except Heirs
            Thread T = new Thread(() =>
            {
                //T0_T2_Arbitrage.StrategyMayCreateNewOrders = false;

                for (int i = 0; i < C.AllOrdersByID.Keys.Count; i++)
                {
                    if ((C.AllOrdersByID[C.AllOrdersByID.Keys.ElementAt(i)].Status == OrderStatus.active) &&
                        !(C.AllOrdersByID[C.AllOrdersByID.Keys.ElementAt(i)].IsHeir))
                    {
                        LogClient.Add(string.Format("[Inf][GUI] Killing all orders => order #{0} for active {1} with current status {2}",
                            C.AllOrdersByID.Keys.ElementAt(i),
                            (string)C.AllOrdersByID[C.AllOrdersByID.Keys.ElementAt(i)].Instrument.LevelI[EData.Code],
                            C.AllOrdersByID[C.AllOrdersByID.Keys.ElementAt(i)].Status.ToString()));
                        OrderManager.KillOrder(C.AllOrdersByID[C.AllOrdersByID.Keys.ElementAt(i)]);
                        Thread.Sleep(40);
                    }

                }
            });
            T.Start();
        }

        private void AllowTradingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AllowTradingCheckBox.Checked)
                OrderManager.AllowTrading = true;
            else
                OrderManager.AllowTrading = false;

            LogClient.Add(string.Format("[Inf][GUI][UserAction] OrderManager.Allotrading is set to {0}", AllowTradingCheckBox.Checked));
        }

        private void MASSGUI_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Order test = new Order();
            for (int i = 0; i < C.Instruments.Count; i++)
            {
                if (C.Instruments[i].LevelI[EData.Code].ToString() == "SiM6")
                {
                    test.Instrument = C.Instruments[i];
                }
            }
            if (test.Instrument != null)
            {
                test.Quantity = 1;
                test.Price = 65000;//Math.Round((double)test.Instrument.LevelI[EData.LevelIBid] * 0.98 / 100) * 100;
                test.Operation = OrderOperation.buy;

                test.Strategy = null;
                test.BrokerRef = "";
                test.AccountNo = "41000VK";

                //QuikDDEProvider.SendOrder(test);
                OrderManager.NewOrder(test);
            }
        }
    }

}
