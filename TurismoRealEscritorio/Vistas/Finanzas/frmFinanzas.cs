﻿using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TurismoRealEscritorio.Controlador;
using TurismoRealEscritorio.Modelos.Util;
using TurismoRealEscritorio.Modelos.Util.Strategy;
using TurismoRealEscritorio.Vista;

namespace TurismoRealEscritorio.Vistas.Finanzas
{
    public partial class frmFinanzas : Form
    {
        frmMain Main;
        Informe informe;
        ProxyMetricas Metricas;
        Panel pTip;
        bool tipActivo = false;
        public frmFinanzas(frmMain f = null)
        {
            InitializeComponent();
            if (f != null)
            {
                Main = f;
            }
            txtInforme.Text = "El informe de ingresos y egresos esta disponible desde el"
                +" primer día del mes siguiente, mostrando un balance total del mes."
                +" \nPara generar el informe correspondiente al mes anterior, presione el botón"
                +" 'Generar informe (PDF)'. \nPuede generar una cantidad ilimitada de documentos.";
        }/* CONTROL */
        private void frmFinanzas_Load(object sender, EventArgs e)
        {
            Main.ConfigurarBotones(pInforme);
            CargarInforme();
            CargarMetricas();
        }
        private void CambiarTab(object sender, EventArgs e)
        {
            DesactivarTips();
            if(contMaestro.SelectedIndex==0)
            {
                CargarMetricas();
            }
        }
        /* DATA */
        private async void CargarMetricas()
        {
            Main.Do();
            Metricas = await ClienteHttp.Peticion.Get<ProxyMetricas>("interno/gestion/metricas", SesionManager.Token, urlEspecial: true);
            var us = 0;
            var con = 0;
            foreach (var i in Metricas.Usuarios)
            {
                us += i;
            }
            foreach(var i in Metricas.Conectados)
            {
                con += i;
            }
            var a= (con.ToString() + "/" + us.ToString()).PadLeft(5);
            var b = a.PadLeft(5);
            txtNConectados.Text = b;
            txtNTransacciones.Text = Metricas.Transacciones.ToString().PadLeft(3,' ');
            txtMNT.Text = "(Mes de " + DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es")) + ")";
            txtNReservas.Text = Metricas.Reservas.ToString().PadLeft(3,' ');
            txtMNR.Text = "(Periodo " + DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es")) + " - " + DateTime.Now.AddMonths(3).ToString("MMMM", CultureInfo.CreateSpecificCulture("es")) + ")";
            txtNMantenciones.Text = Metricas.Mantenciones.ToString().PadLeft(3,' ');
            txtMNM.Text = txtMNR.Text;
            txtE1.Text = Metricas.Departamentos[0].ToString();
            txtE2.Text = Metricas.Departamentos[1].ToString();
            txtE3.Text = Metricas.Departamentos[2].ToString();
            txtE4.Text = Metricas.Departamentos[3].ToString();
            txtE5.Text = Metricas.Departamentos[4].ToString();
            Main.Undo();
        }
        private async void CargarInforme()
        {
            int mes = DateTime.Now.Month - 1;
            int ano = DateTime.Now.Year;
            if (mes <= 0)
            {
                mes = 12;
                ano--;
            }
            informe = await ClienteHttp.Peticion.Get<Informe>("interno/gestion/informe/" + mes + "$" + ano, SesionManager.Token, urlEspecial: true);
            foreach (var i in informe.Ingresos.IngresosReserva)
            {
                tablaReservas.Rows.Add(i.Depto, i.CostoDia, i.Reservas, i.DiasTotales, i.Ganancias);
            }
            foreach(var i in informe.Egresos.EgresosDepto)
            {
                TablaEgresos.Rows.Add(i.Depto,i.Dividendo,i.Contribuciones,i.Mantenciones,i.GastoTotal);
            }
        }
        /* TIPS */
        private void DesactivarTips(object sender = null, EventArgs e = null)
        {
            if (pTip != null)
            {
                pTip.Dispose();
                pTip = null;
            }
        }
        private void lbHC_Click(object sender, EventArgs e)
        {
            if (pTip != null)
            {
                DesactivarTips();
            }
            CreadorTip c = CreadorTip.TipConexiones();
            int x = pConexiones.Location.X + lbHC.Location.X + (int)Math.Floor((double)lbHC.Width / 2);
            int y = pConexiones.Location.Y + lbHC.Location.Y + (int)Math.Floor((double)lbHC.Height / 2);
            pTip = c.CrearTip(x, y, Metricas.Usuarios, Metricas.Conectados);
            pVistaGeneral.Controls.Add(pTip);
            pTip.BringToFront();
        }
        private void ActivarTipDepto(object sender, EventArgs e)
        {
            if (pTip != null)
            {
                DesactivarTips();
            }
            Label l = ((Label)sender);
            Control papa = l.Parent;
            CreadorTip c = CreadorTip.TipDeptos();
            int x = pDeptos.Location.X + papa.Location.X + l.Location.X + (int)Math.Floor((double)l.Width / 2);
            int y = pDeptos.Location.Y + papa.Location.Y + l.Location.Y + (int)Math.Floor((double)l.Height / 2);
            EDepto ed = EDepto.No_Disponible;
            short idx = 0;
            switch (l.Name)
            {
                case "lbHD1":
                    ed = EDepto.No_Disponible;
                    break;
                case "lbHD2":
                    idx = 1;
                    ed = EDepto.Disponible;
                    break;
                case "lbHD3":
                    idx = 2;
                    ed = EDepto.Reservado;
                    break;
                case "lbHD4":
                    idx = 3;
                    ed = EDepto.En_Mantencion;
                    break;
                case "lbHD5":
                    idx = 4;
                    ed = EDepto.Inhabitable;
                    break;
            }
            pTip = c.CrearTip(x, y, ed, Metricas.Departamentos[idx]);
            tabPage1.Controls.Add(pTip);
            pTip.BringToFront();
        }
        private void ActivarTipTrans(object sender, EventArgs e)
        {
            if (pTip != null)
            {
                DesactivarTips();
            }
            Control papa = lbHT.Parent;
            CreadorTip c = CreadorTip.TipComun();
            int x = pVistaGeneral.Location.X + papa.Location.X + lbHT.Location.X + (int)Math.Floor((double)lbHT.Width / 2);
            int y = pVistaGeneral.Location.Y + papa.Location.Y + lbHT.Location.Y + (int)Math.Floor((double)lbHT.Height / 2);
            pTip = c.CrearTip(x, y, "Transacciones","Número de transacciones efectuadas durante todo este mes.");
            tabPage1.Controls.Add(pTip);
            pTip.BringToFront();
        }
        private void ActivarTipRes(object sender, EventArgs e)
        {
            if (pTip != null)
            {
                DesactivarTips();
            }
            Control papa = lbHR.Parent;
            CreadorTip c = CreadorTip.TipComun();
            int x = pVistaGeneral.Location.X + papa.Location.X + lbHR.Location.X + (int)Math.Floor((double)lbHR.Width / 2);
            int y = pVistaGeneral.Location.Y + papa.Location.Y + lbHR.Location.Y + (int)Math.Floor((double)lbHR.Height / 2);
            pTip = c.CrearTip(x, y, "Reservas", "Número de reservas cuya fecha de estadía corresponde al periodo " + DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es")) + " - " + DateTime.Now.AddMonths(3).ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))+".");
            tabPage1.Controls.Add(pTip);
            pTip.BringToFront();
        }
        private void ActivarTipMan(object sender, EventArgs e)
        {
            if (pTip != null)
            {
                DesactivarTips();
            }
            Control papa = lbHM.Parent;
            CreadorTip c = CreadorTip.TipComun();
            int x = pVistaGeneral.Location.X + papa.Location.X + lbHM.Location.X + (int)Math.Floor((double)lbHM.Width / 2);
            int y = pVistaGeneral.Location.Y + papa.Location.Y + lbHM.Location.Y + (int)Math.Floor((double)lbHM.Height / 2);
            pTip = c.CrearTip(x, y, "Mantenciones", "Número de reservas cuya fecha de estadía corresponde al periodo " + DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es")) + " - " + DateTime.Now.AddMonths(3).ToString("MMMM", CultureInfo.CreateSpecificCulture("es")) + ".");
            tabPage1.Controls.Add(pTip);
            pTip.BringToFront();
        }
        /* INFORME */
        private void btnPDF_Click(object sender, EventArgs e)
        {
            GenerarInforme();
        }

        private void GenerarInforme()
        {
            PDFTools.GenerarInformePDF(informe, saveDialog);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmInformes f = new frmInformes(Main);
            f.Show();
        }
    }
}
