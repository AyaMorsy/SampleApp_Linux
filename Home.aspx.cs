using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;
using System.Windows.Forms;
//using Apriori;
using BAL;
using Label = System.Web.UI.WebControls.Label;

namespace AprioriWebApp2
{
    public partial class Home : System.Web.UI.Page


    {
       
       
        protected void Page_Load(object sender, EventArgs e)
        {
           if (!Page.IsPostBack)
            {
                loadControls();
            }
          
          


        }
        GridView DataGridview = new GridView();
        private void loadControls ()
        {
            flowLayoutPanel1.HorizontalAlign = HorizontalAlign.Center;
            flowLayoutPanel2.HorizontalAlign = HorizontalAlign.Center;
            flowLayoutPanel3.HorizontalAlign = HorizontalAlign.Center;
            
            System.Threading.Thread.Sleep(3000);

           
        }

      
        string FileName = string.Empty;
        List<Thread> threads = new List<Thread>();
        int PageSize = 100;
        List<string> lines = new List<string>();
        string line;
        public string inputContent { get; private set; }



        protected void LoadFile_Click(object sender, EventArgs e)
        {
            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();

            var file = FileUploadControl.FileName;
            FileUploadControl.AllowMultiple = false;
           
            //check if have value 
            if (FileUploadControl.PostedFile != null && FileUploadControl.PostedFile.FileName != "" && FileUploadControl.PostedFile.ContentLength >0)
            {

                // check uploaded file is text 

                string ext = System.IO.Path.GetExtension(FileUploadControl.FileName);
                string[] allowedExtenstions = new string[] {".txt"};

                if (allowedExtenstions.Contains(ext))
                {
                    FileName = FileUploadControl.PostedFile.FileName;
                    lblMessage.Text = "File Successfully Uploaded";
                    DoThingThread();
                    RefreshButton.Enabled = true;

                }
                else
                {
                    lblMessage.Text = "You must upload only txt file ";
                }
            }
            else
            {
                lblMessage.Text = "You must upload  file ";
            }
            watch.Stop();
            lblTimeElapsed.Text = watch.ElapsedMilliseconds +"MS"; 
        }

        protected void RefreshButton_Click(object sender, EventArgs e)
        {
             AbortThread();
            DoThingThread();
        }
        private void AbortThread()
        {
            foreach (var thread in threads)
            {
                thread.Abort();
            }
            threads.Clear();
        }
        private void DoThings()
        {
            int Support = 2;

            if (FileUploadControl.PostedFile != null && FileUploadControl.PostedFile.FileName != "" && FileUploadControl.PostedFile.ContentLength > 0)
            {


                using (System.IO.TextReader tr = new System.IO.StreamReader(FileUploadControl.PostedFile.InputStream))
                {
                    while ((line = tr.ReadLine()) != null)
                    {
                        line.Replace("\t", "#");
                        lines.Add(tr.ReadLine());
                        
                    }
                    ViewState["lines"] = lines;
                }
            }
            else
            {

                lines =( List<string> ) ViewState["lines"];


            }
            DataTable dt =   TableUserControl(lines.ToList());
            // flowLayoutPanel1.Controls.Add(DataGridview);
            DataGridview1.DataSource = dt;
            //DataGridview.AllowPaging = true;
            //DataGridview.PageSize = PageSize;
            DataGridview1.DataBind();
            // DataGridview.PageIndexChanging += new GridViewPageEventHandler(GridView1_PageIndexChanging);

            //DataGridview.PageIndexChanging += GridView1_PageIndexChanging;


            var enumerableTable = (dt as System.ComponentModel.IListSource).GetList();
            Chart1.DataBindTable(enumerableTable, "Itemset");
            //Chart1.Series["Series1"].XValueMember = "Count";
            //Chart1.Series["Series1"].YValueMembers = "Itemset";
            //Chart1.DataSource = dt;
            //Chart1.DataBind();

            //Chart ItemsetChart = new Chart();
            //ItemsetChart.Series.Add("x");
            //ItemsetChart.Series["x"].XValueMember = "Itemset";
            //ItemsetChart.Series["x"].YValueMembers = "Count";
            //ItemsetChart.DataSource = dt;
            //ItemsetChart.DataBind();

            BAL.Apriori apriori = new BAL.Apriori(lines.ToList());
            int k = 1;
            List<BAL.ItemSet> ItemSets = new List<BAL.ItemSet>();
            bool next;
            do
            {
                next = false;
                var L = apriori.GetItemSet(k, Support, IsFirstItemList: k == 1);
                if (L.Count > 0)
                {
                    List<AssociationRule> rules = new List<AssociationRule>();
                    if (k != 1)
                        rules = apriori.GetRules(L);
                  //  TableUserControl tableL = new TableUserControl(L, rules);
                   TableUserControl(L, rules);

                    next = true;
                    k++;
                    ItemSets.Add(L);
                  
                }
            } while (next);

          
        }
        private void DoThingThread()
        {
            DoThings();
            //Thread t = new Thread(delegate ()
            //{
               
            //    DoThings();
              
            //})
            //{ Name = "DoThings" };
            //threads.Add(t);
            //t.Start();
        }
        public void TableUserControl(ItemSet itemSet, List<AssociationRule> rules)
        {
            GridView ItemSetsDataGrid = new GridView();
            GridView RulesDataGrid = new GridView();
            Chart ItemsetChart = new Chart();
            Label ItemSet = new Label();
            Label RuleSet = new Label();
            ItemSet.Text = itemSet.Label;
            flowLayoutPanel2.Controls.Add(ItemSet);
            flowLayoutPanel2.Controls.Add(ItemSetsDataGrid);
            flowLayoutPanel3.Controls.Add(RuleSet);
            flowLayoutPanel3.Controls.Add(RulesDataGrid);
          
            ItemsetChart.Visible = true;
            //ItemSetsDataGrid.AllowPaging = true;
            //ItemSetsDataGrid.PageSize = PageSize;
            //RulesDataGrid.AllowPaging = true;
            //RulesDataGrid.PageSize = PageSize;
           // ItemSetLabel.Text = itemSet.Label;
            DataTable dt = new DataTable();
            DataTable dt1 = new DataTable();
            if (dt.Columns.Count == 0)
            {

                dt.Columns.Add("itemSet", typeof(string));
                dt.Columns.Add("Count", typeof(string));
                dt1.Columns.Add("itemSet", typeof(string));
                dt1.Columns.Add("Count", typeof(int));
            }
            DataTable dt2 = new DataTable();
            if (dt2.Columns.Count == 0)
            {

                dt2.Columns.Add("item", typeof(string));
                dt2.Columns.Add("Confidance", typeof(string));
                dt2.Columns.Add("Support", typeof(string));
            }
            
            foreach (var item in itemSet)
            {
                dt.Rows.Add(item.Key.ToDisplay(), item.Value);
               dt1.Rows.Add(item.Key.ToDisplay(),int.Parse(item.Value.ToString()));
            }
            if (rules.Count == 0)
            {
                ItemSetsDataGrid.Height = 342;
                //RulesDataGrid.Visible = false;
            }
            else
            {
                RuleSet.Text = "Rules"; 
                foreach (var item in rules)
                {

                    dt2.Rows.Add(item.Label, item.Confidance.ToPercentString(), item.Support.ToPercentString());

                }
            }
            ItemSetsDataGrid.Height = 500;
            RulesDataGrid.Height = 500;
            ItemSetsDataGrid.DataSource = dt;
            ItemSetsDataGrid.DataBind();          
            RulesDataGrid.DataSource = dt2;
            RulesDataGrid.DataBind();

           

            foreach (var item in itemSet)
            {
                if (item.Value < itemSet.Support)
                    ItemSetsDataGrid.Rows[ItemSetsDataGrid.Rows.Count - 1].BackColor = System.Drawing.Color.LightGray;
            }

            //var enumerableTable = (dt1 as System.ComponentModel.IListSource).GetList();
            //Chart1.DataBindTable(enumerableTable, "Itemset");
            Chart1.Series["Series1"].XValueMember = "Itemset";
            Chart1.Series["Series1"].YValueMembers ="Count";
            Chart1.DataSource = dt1;
            Chart1.DataBind();
        }

        public DataTable TableUserControl(List<string> Values)
        {
            DataTable dt = new DataTable();

            if (dt.Columns.Count == 0)
            {
                dt.Columns.Add("Itemset", typeof(string));
                dt.Columns.Add("Count", typeof(string));

            }
            dataGridLabel.Text = "Transactions";
            //ItemSetsDataGridView.Columns[0].HeaderText = "TransactionID";
            //ItemSetsDataGridView.Columns[1].HeaderText = "Items";
            for (int i = 0; i < Values.Count; i++)
            {

                dt.Rows.Add(i, Values[i]);

                // ItemSetsDataGridView.Rows.Add(i, Values[i]);
            }
            
            return dt;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            //ItemSetsDataGridView.ClearSelection();
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView GridView1 = sender as GridView;
            GridView1.PageIndex = e.NewPageIndex;
           
            GridView1.DataBind();
            AbortThread();
            DoThingThread();
        }
    }
}