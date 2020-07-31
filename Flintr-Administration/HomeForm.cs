using Flintr_lib;
using Flintr_lib.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flintr_Administration
{
    public partial class HomeForm : Form
    {
        private IPEndPoint connectionEndpoint;
        private FlintrInstance flintrInstance;
        private List<WorkerDetail> workerDetails;
        
        public HomeForm()
        {
            InitializeComponent();
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {
            Task.Run(() => Flintr_Runner.Program.RunStandalone(new Flintr_Runner.Configuration.RuntimeConfiguration()));
            LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog();
            connectionEndpoint = new IPEndPoint(loginForm.address, loginForm.port);
            flintrInstance = new FlintrInstance(connectionEndpoint);

            statusStrip1.Items["toolStripStatusLabel1"].Text = $"Connected to {connectionEndpoint.Address}:{connectionEndpoint.Port}";
        }

        private void UpdateAll()
        {
            UpdateWorkerList();
        }

        private void UpdateWorkerList()
        {
            workerDetails = flintrInstance.GetAllWorkerDetails();
            workerListBox.Items.Clear();
            foreach (WorkerDetail wd in workerDetails)
            {
                workerListBox.Items.Add(wd.Name);
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateAll();
        }

        private void HomeForm_DoubleClick(object sender, EventArgs e)
        {

        }
    }
}
