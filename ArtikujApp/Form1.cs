using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Entity;



namespace ArtikujApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (var ctx = new ArtikujDbEntities())
            {
                // Tipet
                var tipet = ctx.Tipis.OrderBy(t => t.Emertimi).ToList();
                cmbTipi.DataSource = tipet;
                cmbTipi.DisplayMember = "Emertimi";
                cmbTipi.ValueMember = "TipiID";

                // Njesite
                var njesite = ctx.Njesias.OrderBy(n => n.Emertimi).ToList(); 
                cmbNjesia.DataSource = njesite;
                cmbNjesia.DisplayMember = "Emertimi";
                cmbNjesia.ValueMember = "NjesiaID";
            }

            // default radio
            rdbI.Checked = true;
            dtpDataSkadences.Value = DateTime.Today;
        }

        private void btnRuaj_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtEmertimi.Text))
            {
                MessageBox.Show("Emertimi eshte i detyrueshem.");
                return;
            }

            if (!decimal.TryParse(txtCmimi.Text.Trim(), out decimal cmimi))
            {
                MessageBox.Show("Cmimi duhet te jete numer.");
                return;
            }

            using (var ctx = new ArtikujDbEntities())
            {
                // find by barcode if provided
                string barkod = string.IsNullOrWhiteSpace(txtBarkod.Text) ? null : txtBarkod.Text.Trim();

                Product produkt = null;

                if (!string.IsNullOrEmpty(barkod))
                {
                    produkt = ctx.Products.FirstOrDefault(p => p.Barkod == barkod);
                }

                // if no barkod or not found, try matching by Emertimi + TipiID
                if (produkt == null)
                {
                    produkt = ctx.Products.FirstOrDefault(p => p.Emertimi == txtEmertimi.Text.Trim()
                                                               && p.TipiID == (int)cmbTipi.SelectedValue);
                }

                bool isNew = false;
                if (produkt == null)
                {
                    produkt = new Product();
                    ctx.Products.Add(produkt);
                    isNew = true;
                }

                // Map fields
                produkt.Emertimi = txtEmertimi.Text.Trim();
                produkt.Barkod = barkod;
                produkt.Cmimi = cmimi;
                produkt.DataSkadences = dtpDataSkadences.Value.Date;
                produkt.KaTvsh = chkKaTvsh.Checked;

                var llojEm = rdbI.Checked ? "I" : "V";
                var lloj = ctx.Llojs.FirstOrDefault(l => l.Emertimi == llojEm);
                if (lloj == null)
                {
                    // if Lloj rows missing, create them
                    lloj = new Lloj { Emertimi = llojEm };
                    ctx.Llojs.Add(lloj);
                    ctx.SaveChanges(); 
                }
                produkt.LlojID = lloj.LlojID;

                produkt.TipiID = (int)cmbTipi.SelectedValue;
                produkt.NjesiaID = (int)cmbNjesia.SelectedValue;

                ctx.SaveChanges();

                MessageBox.Show(isNew ? "Produkti u shtua me sukses." : "Produkti u perditesua me sukses.");
            }

            ClearForm();
        }

        private void btnKerko_Click(object sender, EventArgs e)
        {
            using (var frm = new FormSearch())
            {
                if (frm.ShowDialog(this) == DialogResult.OK && frm.SelectedProductID.HasValue)
                {
                    LoadProductIntoForm(frm.SelectedProductID.Value);
                }
            }
        }
        private void LoadProductIntoForm(int productId)
        {
            using (var ctx = new ArtikujDbEntities())
            {
                var p = ctx.Products.Include(x => x.Lloj).FirstOrDefault(x => x.ProductID == productId);
                if (p == null) return;

                txtEmertimi.Text = p.Emertimi;
                txtBarkod.Text = p.Barkod;
                txtCmimi.Text = p.Cmimi.ToString("F2");
                dtpDataSkadences.Value = p.DataSkadences ?? DateTime.Today;
                chkKaTvsh.Checked = p.KaTvsh;

                if (p.Lloj != null)
                {
                    rdbI.Checked = p.Lloj.Emertimi == "I";
                    rdbV.Checked = p.Lloj.Emertimi == "V";
                }

                cmbTipi.SelectedValue = p.TipiID;
                cmbNjesia.SelectedValue = p.NjesiaID;
            }
        }

        private void btnFshi_Click(object sender, EventArgs e)
        {
            // Delete by barcode or current loaded product
            using (var ctx = new ArtikujDbEntities())
            {
                Product produkt = null;
                if (!string.IsNullOrWhiteSpace(txtBarkod.Text))
                {
                    produkt = ctx.Products.FirstOrDefault(p => p.Barkod == txtBarkod.Text.Trim());
                }

                if (produkt == null && !string.IsNullOrWhiteSpace(txtEmertimi.Text))
                {
                    produkt = ctx.Products.FirstOrDefault(p => p.Emertimi == txtEmertimi.Text.Trim()
                                                              && p.TipiID == (int)cmbTipi.SelectedValue);
                }

                if (produkt == null)
                {
                    MessageBox.Show("Nuk u gjet produkti per fshirje.");
                    return;
                }

                var confirm = MessageBox.Show($"Deshironi te fshini produktin: {produkt.Emertimi}?",
                                              "Konfirmo fshirjen", MessageBoxButtons.YesNo);
                if (confirm == DialogResult.Yes)
                {
                    ctx.Products.Remove(produkt);
                    ctx.SaveChanges();
                    MessageBox.Show("Produkti u fshi me sukses.");
                }
            }

            ClearForm();
        }

        private void btnDil_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ClearForm()
        {
            txtEmertimi.Clear();
            txtCmimi.Clear();
            txtBarkod.Clear();
            cmbTipi.SelectedIndex = -1;
            cmbNjesia.SelectedIndex = -1;
            dtpDataSkadences.Value = DateTime.Today;
            rdbI.Checked = false;
            rdbV.Checked = false;
            chkKaTvsh.Checked = false;

            txtEmertimi.Focus();
        }

    }
}
