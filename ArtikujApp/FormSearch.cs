using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArtikujApp
{
    public partial class FormSearch : Form
    {
        public int? SelectedProductID { get; private set; }
        public FormSearch()
        {
            InitializeComponent();
        }

        private void FormSearch_Load(object sender, EventArgs e)
        {

            LoadProducts();
        }

        private void LoadProducts()
        {
            lvProducts.Items.Clear();
            using (var ctx = new ArtikujDbEntities())
            {
                var list = ctx.Products
                              .OrderBy(p => p.Emertimi)
                              .Select(p => new { p.ProductID, p.Emertimi, p.Barkod, p.Cmimi})
                              .ToList();

                foreach (var p in list)
                {
                    var item = new ListViewItem(p.ProductID.ToString());
                    item.SubItems.Add(p.Emertimi);
                    item.SubItems.Add(p.Barkod ?? "");
                    item.SubItems.Add(p.Cmimi.ToString("F2"));
                    lvProducts.Items.Add(item);
                }
            }
        }


        private void lvProducts_DoubleClick(object sender, EventArgs e)
        {
            if (lvProducts.SelectedItems.Count > 0)
            {
                SelectedProductID = int.Parse(lvProducts.SelectedItems[0].Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (lvProducts.SelectedItems.Count > 0)
            {
                SelectedProductID = int.Parse(lvProducts.SelectedItems[0].Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Zgjidhni nje produkt nga lista.");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
