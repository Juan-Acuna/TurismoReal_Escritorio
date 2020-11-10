﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TurismoRealEscritorio.Controlador;
using TurismoRealEscritorio.Modelos;
using TurismoRealEscritorio.Modelos.Util;
using TurismoRealEscritorio.Vista;

namespace TurismoRealEscritorio.Vistas.Logistica
{
    public partial class frmLogistica : Form
    {
        frmMain Main;
        public bool expand = false;
        public bool suma = true;
        public bool anim = false;
        bool timerRun = false;
        bool primeraCargaI = true;
        bool primeraCargaL = true;
        bool primeraCargaV = true;
        bool primeraCargaC = true;
        bool VemailDisp = false;
        bool Vemail = false;
        bool VrutDisp = false;
        Regex formatoCorreo = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$");
        Panel pEdicion;
        Logistica actual = Logistica.Inventario;

        public frmLogistica(frmMain m = null)
        {
            InitializeComponent();
            Main = m;
            foreach(Control c in contMaestro.TabPages)
            {
                Main.ConfigurarBotones(c);
            }
        }

        private void frmLogistica_Load(object sender, EventArgs e)
        {
            pEdicionI.Height = 0;
            pEdicionL.Height = 0;
            pEdicionV.Height = 0;
            pEdicionC.Height = 0;
            pEdicionI.Visible = true;
            pEdicion = pEdicionI;
            tablaArticulo.RowHeadersVisible = false;
            tablaArticulo.MultiSelect = false;
            tablaArticulo.ScrollBars = ScrollBars.Vertical;
            tablaLocalidad.RowHeadersVisible = false;
            tablaLocalidad.MultiSelect = false;
            tablaLocalidad.ScrollBars = ScrollBars.Vertical;
            tablaVehiculo.RowHeadersVisible = false;
            tablaVehiculo.MultiSelect = false;
            tablaVehiculo.ScrollBars = ScrollBars.Vertical;
            tablaChofer.RowHeadersVisible = false;
            tablaChofer.MultiSelect = false;
            tablaChofer.ScrollBars = ScrollBars.Vertical;

            tablaArticulo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tablaLocalidad.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tablaVehiculo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tablaChofer.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            Main.EstadoTrabajo = EstadoTrabajo.Espera;
            cbRegion.DisplayMember = "Region";
            cbRegion.ValueMember = "Region";
            cbGenero.DisplayMember = "Nombre";
            cbGenero.ValueMember = "Id_genero";
            do
            {
                cbRegion.DataSource = Main.Repos.Regiones;
            } while (cbRegion.DataSource == null);
            do
            {
                cbGenero.DataSource = Main.Repos.Generos;
            } while (cbGenero.DataSource == null);

            CargarArticulos();
        }

        private void PrepararComboboxes()
        {
            cbRegion.SelectedItem = null;
            cbComuna.SelectedItem = null;
            cbGenero.SelectedItem = null;
            cbComuna.Enabled = false;
            cbRegion.Text = "Seleccione Region";
            cbComuna.Text = "Seleccione Comuna";
            cbGenero.Text = "Seleccione Genero";
            cbRegion.Refresh();
            cbComuna.Refresh();
            cbGenero.Refresh();
        }
        private void cbRegion_SelectionChangeCommitted(object sender, EventArgs e = null)
        {
            if (sender != null)
            {
                ComboBox temp = (ComboBox)sender;
                cbComuna.DataSource = ((ProxyRegion)temp.SelectedItem).Comunas;
                cbComuna.DisplayMember = "Nombre";
                cbComuna.ValueMember = "Nombre";
                cbComuna.Refresh();
                cbComuna.Enabled = true;
            }
        }
        /* BOTONES*/

        private async void BotonesModificar(object sender, EventArgs e)
        {
            try
            {
                /* CAMBIA SEGUN PAGINA*/
                switch (actual)
                {
                    case Logistica.Inventario:
                        Articulo a;
                        do
                        {
                            a = await ClienteHttp.Peticion.Get<Articulo>(tablaArticulo.SelectedRows[0].Cells[0].Value.ToString(), SesionManager.Token);
                        } while (a == null);
                        lbIdI.Visible = true;
                        txtIdI.Visible = true;
                        txtIdI.Text = a.Id_articulo.ToString();
                        txtNombreI.Text = a.Nombre;
                        txtValorI.Text = a.Valor.ToString();
                        break;
                    case Logistica.Localidades:
                        Localidad l;
                        do
                        {
                            l = await ClienteHttp.Peticion.Get<Localidad>(tablaLocalidad.SelectedRows[0].Cells[0].Value.ToString());
                        } while (l == null);
                        lbIdL.Visible = true;
                        txtIdL.Visible = true;
                        txtIdL.Text = l.Id_localidad.ToString();
                        txtNombreL.Text = l.Nombre;
                        break;
                    case Logistica.Vehiculos:
                        Vehiculo v;
                        do
                        {
                            v = await ClienteHttp.Peticion.Get<Vehiculo>(tablaVehiculo.SelectedRows[0].Cells[0].Value.ToString(),SesionManager.Token);
                        } while (v == null);
                        txtPatente.Text = v.Patente;
                        txtPatente.Enabled = false;
                        txtMarcaV.Text = v.Marca;
                        txtModeloV.Text = v.Modelo;
                        break;
                    case Logistica.Choferes:
                        PersonaChofer pc;
                        do
                        {
                            pc = await ClienteHttp.Peticion.Get<PersonaChofer>(tablaChofer.SelectedRows[0].Cells[0].Value.ToString(), SesionManager.Token);
                        } while (pc == null);
                        PrepararComboboxes();
                        lbIdC.Visible = true;
                        txtIdC.Visible = true;
                        txtIdC.Text = pc.Chofer.Id_chofer.ToString();
                        txtRut.Text = pc.Persona.Rut;
                        txtNombres.Text = pc.Persona.Nombres;
                        txtApellidos.Text = pc.Persona.Apellidos;
                        dtNacimiento.Value = pc.Persona.Nacimiento;
                        cbGenero.SelectedItem = Repositorios.Buscar((List<Genero>)cbGenero.DataSource, "Id_genero", pc.Persona.Id_genero);
                        txtEmail.Text = pc.Persona.Email;
                        txtTelefono.Text = pc.Persona.Telefono.ToString();
                        txtDireccion.Text = pc.Persona.Direccion;
                        cbRegion.SelectedItem = Repositorios.Buscar((List<ProxyRegion>)cbRegion.DataSource, "Region", pc.Persona.Region);
                        cbRegion_SelectionChangeCommitted(cbRegion);
                        cbComuna.SelectedItem = Repositorios.Buscar((List<Comuna>)cbComuna.DataSource, "Nombre", pc.Persona.Comuna);
                        break;
                }
            }
            catch(Exception ex){
                return;
            }
            Main.EstadoTrabajo = EstadoTrabajo.Modificando;
            Desplegar();
        }

        private void BotonesCancelar(object sender, EventArgs e)
        {
            /* CAMBIA SEGUN PAGINA*/
            expand = true;
            switch (actual)
            {
                case Logistica.Inventario:
                    //CargarArticulos();
                    break;
                case Logistica.Localidades:
                    //CargarLocalidades();
                    break;
                case Logistica.Vehiculos:
                    //CargarVehiculos();
                    break;
                case Logistica.Choferes:
                    //CargarChoferes();
                    break;
            }
            Main.EstadoTrabajo = EstadoTrabajo.Espera;
        }
        private void BotonesNuevo(object sender, EventArgs e)
        {
            /* CAMBIA SEGUN PAGINA*/
            try
            {
                switch (actual)
                {
                    case Logistica.Inventario:
                        txtIdI.Text = "";
                        lbIdI.Visible = false;
                        txtIdI.Visible = false;
                        txtNombreI.Text = "";
                        txtValorI.Text = "";
                        break;
                    case Logistica.Localidades:
                        lbIdL.Visible = false;
                        txtIdL.Visible = false;
                        txtIdL.Text = "";
                        txtNombreL.Text = "";
                        break;
                    case Logistica.Vehiculos:
                        txtPatente.Text = "";
                        txtPatente.Enabled = true;
                        txtMarcaV.Text = "";
                        txtModeloV.Text = "";
                        break;
                    case Logistica.Choferes:
                        PrepararComboboxes();
                        lbIdC.Visible = false;
                        txtIdC.Visible = false;
                        txtIdC.Text = "";
                        txtRut.Text = "";
                        txtNombres.Text = "";
                        txtApellidos.Text = "";
                        dtNacimiento.Value = DateTime.Now;
                        txtEmail.Text = "";
                        txtTelefono.Text = "";
                        txtDireccion.Text = "";
                        break;
                }
            }
            catch(Exception ex)
            {
                return;
            }
            Main.EstadoTrabajo = EstadoTrabajo.Agregando;
            Desplegar();
        }
        private async void BotonesAplicar(object sender, EventArgs e)
        {
            /* CAMBIA SEGUN PAGINA*/
            switch (Main.EstadoTrabajo)
            {
                case EstadoTrabajo.Agregando:
                    switch (actual)
                    {
                        case Logistica.Inventario:
                            Articulo a = new Articulo();
                            a.Nombre = txtNombreI.Text;
                            a.Valor = Int32.Parse(txtValorI.Text);
                            await ClienteHttp.Peticion.Send(HttpMethod.Post, a, token: SesionManager.Token);
                            break;
                        case Logistica.Localidades:
                            Localidad l = new Localidad();
                            l.Nombre = txtNombreL.Text;
                            await ClienteHttp.Peticion.Send(HttpMethod.Post, l, token: SesionManager.Token);
                            break;
                        case Logistica.Vehiculos:
                            Vehiculo v = new Vehiculo();
                            v.Patente = txtPatente.Text;
                            v.Marca = txtMarcaV.Text;
                            v.Modelo = txtModeloV.Text;
                            await ClienteHttp.Peticion.Send(HttpMethod.Post, v, token: SesionManager.Token);
                            break;
                        case Logistica.Choferes:
                            PersonaChofer pc = new PersonaChofer();
                            pc.Persona = new Persona();
                            pc.Chofer = new Chofer();
                            var p = pc.Persona;
                            p.Rut = txtRut.Text;
                            p.Nombres = txtNombres.Text;
                            p.Apellidos = txtApellidos.Text;
                            p.Nacimiento = dtNacimiento.Value;
                            p.Id_genero = (int)cbGenero.SelectedValue;
                            p.Email = txtEmail.Text;
                            p.Telefono = Int64.Parse(txtTelefono.Text);
                            p.Direccion = txtDireccion.Text;
                            p.Region = (String)cbRegion.SelectedValue;
                            p.Comuna = (String)cbComuna.SelectedValue;
                            await ClienteHttp.Peticion.Send(HttpMethod.Post, pc, token: SesionManager.Token);
                            break;
                    }
                    break;
                case EstadoTrabajo.Modificando:
                    switch (actual)
                    {
                        case Logistica.Inventario:
                            Articulo a = new Articulo();
                            a.Nombre = txtNombreI.Text;
                            a.Valor = Int32.Parse(txtValorI.Text);
                            await ClienteHttp.Peticion.Send(new HttpMethod("PATCH"), a,txtIdI.Text, SesionManager.Token);
                            break;
                        case Logistica.Localidades:
                            Localidad l = new Localidad();
                            l.Nombre = txtNombreL.Text;
                            await ClienteHttp.Peticion.Send(new HttpMethod("PATCH"), l, txtIdL.Text, SesionManager.Token);
                            break;
                        case Logistica.Vehiculos:
                            Vehiculo v = new Vehiculo();
                            v.Marca = txtMarcaV.Text;
                            v.Modelo = txtModeloV.Text;
                            await ClienteHttp.Peticion.Send(new HttpMethod("PATCH"), v, txtPatente.Text, SesionManager.Token);
                            break;
                        case Logistica.Choferes:
                            PersonaChofer pc = new PersonaChofer();
                            pc.Persona = new Persona();
                            pc.Chofer = new Chofer();
                            var p = pc.Persona;
                            p.Rut = txtRut.Text;
                            p.Nombres = txtNombres.Text;
                            p.Apellidos = txtApellidos.Text;
                            p.Nacimiento = dtNacimiento.Value;
                            p.Id_genero = (int)cbGenero.SelectedValue;
                            p.Email = txtEmail.Text;
                            p.Telefono = Int64.Parse(txtTelefono.Text);
                            p.Direccion = txtDireccion.Text;
                            p.Region = (String)cbRegion.SelectedValue;
                            p.Comuna = (String)cbComuna.SelectedValue;
                            await ClienteHttp.Peticion.Send(new HttpMethod("PATCH"), pc, txtIdC.Text, SesionManager.Token);
                            break;
                    }
                    break;
            }
            switch (actual)
            {
                case Logistica.Inventario:
                    CargarArticulos();
                    break;
                case Logistica.Localidades:
                    CargarLocalidades();
                    break;
                case Logistica.Vehiculos:
                    CargarVehiculos();
                    break;
                case Logistica.Choferes:
                    CargarChoferes();
                    break;
            }
            expand = true;
            Main.EstadoTrabajo = EstadoTrabajo.Espera;
        }
        private void BotonesEliminar(object sender, EventArgs e)
        {
            switch (actual)
            {
                case Logistica.Inventario:
                    CargarArticulos();
                    break;
                case Logistica.Localidades:
                    CargarLocalidades();
                    break;
                case Logistica.Vehiculos:
                    CargarVehiculos();
                    break;
                case Logistica.Choferes:
                    CargarChoferes();
                    break;
            }
        }
        private void BotonesAsignar(object sender, EventArgs e)
        {
            switch (actual)
            {
                case Logistica.Inventario:
                    
                    break;
                case Logistica.Localidades:
                    
                    break;
            }
        }
        private void btnRefrescarI_Click(object sender, EventArgs e)
        {
            CargarArticulos();
        }

        private void btnRefrescarL_Click(object sender, EventArgs e)
        {
            CargarLocalidades();
        }

        private void btnRefrescarV_Click(object sender, EventArgs e)
        {
            CargarVehiculos();
        }

        private void btnRefrescarC_Click(object sender, EventArgs e)
        {
            CargarChoferes();
        }

        /* ZONA DE LOS DATA GRID VIEW */

        public async void CargarArticulos()
        {
            try
            {
                Main.Do();
                var lista = await ClienteHttp.Peticion.GetList<Articulo>(SesionManager.Token);

                if (primeraCargaI)
                {
                    tablaArticulo.Columns.Add("id", "Identificador");
                    tablaArticulo.Columns.Add("nombre", "Nombre");
                    tablaArticulo.Columns.Add("valor", "Valor");
                    tablaArticulo.Columns.Add("depto", "Departamento asignado");

                    tablaArticulo.Columns["id"].Width = 75;
                    tablaArticulo.Columns["nombre"].Width = 400;
                    tablaArticulo.Columns["valor"].Width = 100;
                    tablaArticulo.Columns["depto"].Width = 242;
                }
                else
                {
                    tablaArticulo.Rows.Clear();
                }
                foreach (var i in lista)
                {
                    var depto = await ClienteHttp.Peticion.Get<Departamento>(token: SesionManager.Token, url: "articulo/asignado/" + i.Id_articulo.ToString(),urlEspecial:true);
                    tablaArticulo.Rows.Add(i.Id_articulo, i.Nombre, "$" + i.Valor.ToString(), (depto == null ? "No asignado" : depto.Nombre));
                }
                if (primeraCargaI)
                {
                    tablaArticulo.Rows[0].Selected = true;
                    primeraCargaI = false;
                    pEdicion.Visible = true;
                }
                Main.Undo();
            }
            catch(Exception e)
            {
                Main.Undo();
                return;
            }
        }
        public async void CargarLocalidades()
        {
            try
            {
                Main.Do();
                var lista = await ClienteHttp.Peticion.GetList<Localidad>();

                if (primeraCargaL)
                {
                    tablaLocalidad.Columns.Add("id", "Identificador");
                    tablaLocalidad.Columns.Add("nombre", "Nombre");
                    tablaLocalidad.Columns.Add("username", "Funcionario asignado");

                    tablaLocalidad.Columns["id"].Width = 75;
                    tablaLocalidad.Columns["nombre"].Width = 340;
                    tablaLocalidad.Columns["username"].Width = 402;
                }
                else
                {
                    tablaLocalidad.Rows.Clear();
                }
                foreach (var i in lista)
                {
                    var pu = await ClienteHttp.Peticion.Get<PersonaUsuario>(token: SesionManager.Token, url: "localidad/asignado/" + i.Id_localidad.ToString());
                    tablaLocalidad.Rows.Add(i.Id_localidad, i.Nombre, (pu == null ? "Sin funcionario asignado" : pu.Persona.Nombres+" "+pu.Persona.Apellidos+" ["+pu.Usuario.Username+"]"));
                }
                if (primeraCargaL)
                {
                    tablaLocalidad.Rows[0].Selected = true;
                    primeraCargaL = false;
                    pEdicion.Visible = true;
                }
                Main.Undo();
            }
            catch (Exception e)
            {
                Main.Undo();
                return;
            }
        }
        public async void CargarVehiculos()
        {
            try
            {
                Main.Do();
                var lista = await ClienteHttp.Peticion.GetList<Vehiculo>(SesionManager.Token);

                if (primeraCargaV)
                {
                    tablaVehiculo.Columns.Add("patente", "Patente");
                    tablaVehiculo.Columns.Add("marca", "Marca");
                    tablaVehiculo.Columns.Add("modelo", "Modelo");

                    tablaVehiculo.Columns["patente"].Width = 75;
                    tablaVehiculo.Columns["marca"].Width = 340;
                    tablaVehiculo.Columns["modelo"].Width = 402;
                }
                else
                {
                    tablaVehiculo.Rows.Clear();
                }
                foreach (var i in lista)
                {
                    tablaVehiculo.Rows.Add(i.Patente, i.Marca,i.Modelo);
                }
                if (primeraCargaV)
                {
                    tablaVehiculo.Rows[0].Selected = true;
                    primeraCargaV = false;
                    pEdicion.Visible = true;
                }
                Main.Undo();
            }
            catch (Exception e)
            {
                Main.Undo();
                return;
            }
        }
        public async void CargarChoferes()
        {
            try
            {
                Main.Do();
                var lista = await ClienteHttp.Peticion.GetList<PersonaChofer>(SesionManager.Token);

                if (primeraCargaC)
                {
                    tablaChofer.Columns.Add("id", "Identificador");
                    tablaChofer.Columns.Add("rut", "Rut");
                    tablaChofer.Columns.Add("nombre", "Nombre completo");
                    tablaChofer.Columns.Add("email", "Correo electrónico");
                    tablaChofer.Columns.Add("fono", "Telefono");
                    tablaChofer.Columns.Add("direc", "Dirección");
                    tablaChofer.Columns.Add("region", "Region");
                    tablaChofer.Columns.Add("comuna", "Comuna");

                    tablaChofer.Columns["id"].Width = 75;
                    tablaChofer.Columns["rut"].Width = 80;
                    tablaChofer.Columns["nombre"].Width = 172;
                    tablaChofer.Columns["email"].Width = 142;
                    tablaChofer.Columns["fono"].Width = 90;
                    tablaChofer.Columns["direc"].Width = 151;
                    tablaChofer.Columns["region"].Width = 90;
                    tablaChofer.Columns["comuna"].Width = 90;

                    tablaChofer.Columns["id"].Visible = false;
                }
                else
                {
                    tablaChofer.Rows.Clear();
                }
                foreach (var i in lista)
                {
                    tablaChofer.Rows.Add(i.Chofer.Id_chofer,i.Persona.Rut,i.Persona.Nombres+" "+i.Persona.Apellidos,i.Persona.Email,i.Persona.Telefono,i.Persona.Direccion,i.Persona.Region,i.Persona.Comuna);
                }
                if (primeraCargaC)
                {
                    tablaChofer.Rows[0].Selected = true;
                    primeraCargaC = false;
                    pEdicion.Visible = true;
                }
                Main.Undo();
            }
            catch (Exception e)
            {
                Main.Undo();
                return;
            }
        }

        /* ANIMACIONES */
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (expand)
            {
                if (suma)
                {
                    if (pEdicion.Height >= 314)
                    {
                        suma = !suma;
                        expand = false;
                    }
                    else
                    {
                        pEdicion.Height = pEdicion.Height + 31;
                    }
                }
                else
                {
                    if (pEdicion.Height <= 0)
                    {
                        suma = !suma;
                        expand = false;
                        anim = true;
                        timerEdicion.Stop();timerRun = false;
                        //SE DESBLOQUEAN LOS BOTONES
                        DesbloquearBotones();
                    }
                    else
                    {
                        pEdicion.Height = pEdicion.Height - 31;
                    }
                }
                pEdicion.Refresh();
            }
        }
        private void Desplegar()
        {
            timerEdicion.Start();
            timerRun = true;
            expand = true;
            suma = true;
            //SE BLOQUEAN LOS BOTONES
            BloquearBotones();
        }

        /* OTROS */
        private void contMaestro_SelectedIndexChanged(object sender, EventArgs e)
        {
            DesbloquearBotones();
            actual = (Logistica)((TabControl)sender).SelectedIndex;
            expand = false;
            suma = true;
            anim = false;
            if (timerRun)
            {
                timerEdicion.Stop();
                timerRun = false;
            }

            switch (actual)
            {
                case Logistica.Inventario:
                    pEdicion = pEdicionI;
                    CargarArticulos();
                    break;
                case Logistica.Localidades:
                    pEdicion = pEdicionL;
                    CargarLocalidades();
                    break;
                case Logistica.Vehiculos:
                    pEdicion = pEdicionV;
                    CargarVehiculos();
                    break;
                case Logistica.Choferes:
                    pEdicion = pEdicionC;
                    CargarChoferes();
                    break;
            }
            pEdicion.Height = 0;
        }
        private async void VerificarDisponibilidad(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            switch (actual)
            {
                case Logistica.Inventario:
                    if (Main.EstadoTrabajo == EstadoTrabajo.Agregando)
                    {
                        await ClienteHttp.Peticion.Disponible(txt.Text, "articulo");
                    }
                    break;
                case Logistica.Localidades:
                    if (Main.EstadoTrabajo == EstadoTrabajo.Agregando)
                    {
                        await ClienteHttp.Peticion.Disponible(txt.Text, "localidad");
                    }
                    break;
                case Logistica.Vehiculos:
                    if (Main.EstadoTrabajo == EstadoTrabajo.Agregando)
                    {
                        await ClienteHttp.Peticion.Disponible(txt.Text, "patente");
                    }
                    break;
                case Logistica.Choferes:
                    switch (txt.Name)
                    {
                        case "txtRut":
                            if (Main.EstadoTrabajo == EstadoTrabajo.Agregando)
                            {
                                VrutDisp = await ClienteHttp.Peticion.Disponible(txt.Text, "rut");
                                if (VrutDisp)
                                {
                                    txt.ForeColor = Color.Green;
                                    lbErrorR.Text = "";
                                }
                                else
                                {
                                    txt.ForeColor = Color.Red;
                                    lbErrorR.Text = "El rut ingresado ya pertenece a un usuario.";
                                }
                            }
                            break;
                        case "txtEmail":
                            if (Main.EstadoTrabajo == EstadoTrabajo.Agregando)
                            {
                                if (txtEmail.Text.Trim().Length >= 5)
                                {
                                    Vemail = formatoCorreo.IsMatch(txtEmail.Text.ToUpper());
                                    Console.WriteLine(Vemail);
                                    if (Vemail)
                                    {
                                        txt.ForeColor = Color.Green;
                                        lbErrorE.Text = "";
                                        lbErrorE.Visible = false;
                                        VemailDisp = await ClienteHttp.Peticion.Disponible(txt.Text, "email");
                                        if (!VemailDisp)
                                        {
                                            txt.ForeColor = Color.Red;
                                            lbErrorE.Text = "El correo electrónico ingresado ya pertenece a un usuario/chofer.";
                                            lbErrorE.Visible = true;
                                        }
                                    }
                                    else
                                    {
                                        txt.ForeColor = Color.Red;
                                        lbErrorE.Text = "Formato de correo electrónico inválido.";
                                        lbErrorE.Visible = true;
                                    }
                                }
                                else
                                {
                                    txt.ForeColor = Color.Black;
                                    lbErrorE.Text = "";
                                    lbErrorE.Visible = false;
                                    Vemail = false;
                                    VemailDisp = false;
                                }
                            }
                            break;
                    }
                    break;
            }
        }
        public void BloquearBotones()
        {
            Main.BloquearBotones(contMaestro.SelectedTab);
        }
        public void DesbloquearBotones()
        {
            Main.DesbloquearBotones(contMaestro.SelectedTab);
        }
        
    }
    enum Logistica
    {
        Inventario,
        Localidades,
        Vehiculos,
        Choferes
    }
}
