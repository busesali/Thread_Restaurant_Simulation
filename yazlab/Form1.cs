using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace yazlab
{
    public partial class Form1 : Form
    {
        private readonly object lockObj = new object();
        private readonly List<string> customers = new List<string>();
        private readonly Queue<string> waitingCustomers = new Queue<string>();
        private readonly Queue<string> siparisbekleyen = new Queue<string>();
        private readonly Dictionary<string, Panel> customerTables = new Dictionary<string, Panel>();

        private readonly Random random = new Random();
        private readonly int waiterCount = 3; // Garson sayýsý
        private readonly int chefCount = 2; // Aþçý sayýsý
        private readonly int cashCount = 1; // Kasa sayýsý
        private readonly SemaphoreSlim waiterSemaphore1;
        private readonly SemaphoreSlim waiterSemaphore2;
        private readonly SemaphoreSlim waiterSemaphore3;

        private readonly SemaphoreSlim chefSemaphore1;
        private readonly SemaphoreSlim chefSemaphore2;

        private readonly SemaphoreSlim chef1CookCount = new SemaphoreSlim(2, 2);
        private readonly SemaphoreSlim chef2CookCount = new SemaphoreSlim(2, 2);

        private int priorityCustomersCount = 0;
        private int normalCustomersCount = 0;

        public Form1()
        {
            InitializeComponent();
            waiterSemaphore1 = new SemaphoreSlim(1, 1);
            waiterSemaphore2 = new SemaphoreSlim(1, 1);
            waiterSemaphore3 = new SemaphoreSlim(1, 1);

            chefSemaphore1 = new SemaphoreSlim(2, 2);
            chefSemaphore2 = new SemaphoreSlim(2, 2);

            listView1.View = View.Details;
            listView2.View = View.Details;
            listView3.View = View.Details;

            listView1.Columns.Add("", -2, HorizontalAlignment.Left);
            listView2.Columns.Add("", -2, HorizontalAlignment.Left);
            listView3.Columns.Add("", -2, HorizontalAlignment.Left);

            dataGridView1.Columns.Add("Müþteri", "Müþteri");
            dataGridView1.Columns.Add("Sipariþ Durumu", "Sipariþ Durumu");
            dataGridView1.Columns.Add("Sipariþ Verdiði Garson", "Sipariþ Verdiði Garson");
            dataGridView1.Columns.Add("Sipariþi Hazýrlayan Aþçý", "Sipariþi Hazýrlayan Aþçý");

            dataGridView2.Columns.Add("Müþteri", "Müþteri");
            dataGridView2.Columns.Add("Ödeme Durumu", "Ödeme Durumu");

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            listView3.Visible = false;

            InitializePanels();
            StartAddingCustomers();
        }

        private void InitializePanels()
        {
            foreach (Control control in Controls)
            {
                if (control is Panel panel)
                {
                    panel.Paint += Panel_Paint;
                }
            }
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void AddToWaitingOrdersList()
        {
            foreach (ListViewItem item in listView3.Items)
            {
                string customer = item.Text;
                siparisbekleyen.Enqueue(customer);
            }
        }


        private void AddCustomerToList(string customer)
        {
            lock (lockObj)
            {
                customers.Add(customer);
                listView1.Items.Add(customer);

            }
        }

        private readonly List<int> emptyTables = new List<int> { 1, 2, 3, 4, 5, 6 };

        private void SetPanelText(Panel panel, string text)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new MethodInvoker(delegate { SetPanelText(panel, text); }));
            }
            else
            {
                using (Graphics g = panel.CreateGraphics())
                {
                    g.Clear(panel.BackColor);
                    g.DrawString(text, Font, Brushes.Black, new PointF(10, 30));
                }
            }
        }

        private void AddCustomerToWaitingList(string customer)
        {
            if (listView2.InvokeRequired)
            {
                listView2.Invoke(new MethodInvoker(delegate { AddCustomerToWaitingList(customer); }));
            }
            else
            {
                listView2.Items.Add(customer);
            }
        }

        private void AssignCustomerToTable(string customer)
        {
            if (emptyTables.Count > 0)
            {
                int randomIndex = random.Next(emptyTables.Count);
                int selectedTable = emptyTables[randomIndex];

                emptyTables.RemoveAt(randomIndex);

                if (Controls.Find($"panel{selectedTable}", true).FirstOrDefault() is Panel selectedPanel)
                {
                    SetPanelText(selectedPanel, customer);
                    customerTables[customer] = selectedPanel;

                }

                listView2.Invoke(new Action(() =>
                {
                    listView2.Items.RemoveByKey(customer);
                }));

                listView3.Invoke(new Action(() =>
                {
                    listView3.Items.Add(customer);
                    siparisbekleyen.Enqueue(customer);
                }));
            }
            else
            {
                waitingCustomers.Enqueue(customer);

                AddCustomerToWaitingList(customer);
            }
        }

        private void AddTableToEmptyList(int tableNumber)
        {
            if (!emptyTables.Contains(tableNumber))
            {
                emptyTables.Add(tableNumber);
            }
        }





        private bool hasGeneratedCustomers = false;

        private async void StartAddingCustomers()
        {
            if (!hasGeneratedCustomers)
            {
                int priorityCustomersCount = random.Next(2, 4);
                int normalCustomersCount = random.Next(6, 8);
                int priorityCustomerNumber = 1;
                int normalCustomerNumber = 1;

                while (priorityCustomersCount > 0 || normalCustomersCount > 0)
                {
                    string newCustomer;
                    if (priorityCustomersCount > 0)
                    {
                        newCustomer = $"Öncelikli Müþteri {priorityCustomerNumber}";
                        priorityCustomersCount--;
                        priorityCustomerNumber++;
                    }
                    else if (normalCustomersCount > 0)
                    {
                        newCustomer = $"Normal Müþteri {normalCustomerNumber}";
                        normalCustomersCount--;
                        normalCustomerNumber++;
                    }
                    else
                    {
                        break;
                    }


                    AddCustomerToList(newCustomer);
                    await Task.Delay(1000);
                    await Task.Run(() => AssignCustomerToTable(newCustomer));

                    await Task.Delay(1000);
                    StartService();
                }

                listView1.Items.Clear();
                hasGeneratedCustomers = true;
            }
        }

        private async void StartService()
        {
            List<Task> waiterTasks = new List<Task>();
            List<Task> chefTasks = new List<Task>();

            for (int i = 0; i < waiterCount; i++)
            {
                Task waiterTask = Task.Run(() => ServeCustomers());
                waiterTasks.Add(waiterTask);
            }

            for (int i = 0; i < chefCount; i++)
            {
                Task chefTask = Task.Run(() => PrepareOrder());
                chefTasks.Add(chefTask);
            }

            await Task.WhenAll(waiterTasks);
            await Task.WhenAll(chefTasks);
        }





        private string GetAvailableWaiter()
        {
            if (waiterSemaphore1.Wait(0))
            {
                return "Garson 1";
            }
            else if (waiterSemaphore2.Wait(0))
            {
                return "Garson 2";
            }
            else if (waiterSemaphore3.Wait(0))
            {
                return "Garson 3";
            }

            return null;
        }

        private void ReleaseWaiter(string garson)
        {
            switch (garson)
            {
                case "Garson 1":
                    waiterSemaphore1.Release();
                    break;
                case "Garson 2":
                    waiterSemaphore2.Release();
                    break;
                case "Garson 3":
                    waiterSemaphore3.Release();
                    break;
            }
        }

        private string GetAvailableChef()
        {
            if (chefSemaphore1.Wait(0))
            {
                return "Aþçý 1";
            }
            else if (chefSemaphore2.Wait(0))
            {
                return "Aþçý 2";
            }

            return null;
        }

        private void ReleaseChef(string asci)
        {
            switch (asci)
            {
                case "Aþçý 1":
                    chefSemaphore1.Release();
                    break;
                case "Aþçý 2":
                    chefSemaphore2.Release();
                    break;
            }
        }


        private async void ServeCustomers()
        {
            while (true)
            {
                string customer;
                lock (lockObj)
                {
                    if (siparisbekleyen.Count == 0)
                        break;

                    customer = siparisbekleyen.Dequeue();
                }

                string garson = GetAvailableWaiter();

                if (garson != null)
                {
                    dataGridView1.Invoke((MethodInvoker)delegate
                    {
                        dataGridView1.Rows.Add(customer, "Sipariþ veriliyor", garson);
                    });

                    Thread.Sleep(5000);

                    dataGridView1.Invoke(new Action(() =>
                    {
                        int rowIndex = dataGridView1.Rows
                            .Cast<DataGridViewRow>()
                            .Where(r => r.Cells["Müþteri"].Value.ToString().Equals(customer))
                            .Select(r => r.Index)
                            .FirstOrDefault();

                        if (rowIndex >= 0)
                        {
                            dataGridView1.Rows[rowIndex].Cells["Sipariþ Durumu"].Value = "Sipariþ alýndý";
                        }
                    }));

                    ReleaseWaiter(garson);
                    await Task.Delay(1000);

                    dataGridView1.Invoke(new Action(() =>
                    {
                        int rowIndex = dataGridView1.Rows
                            .Cast<DataGridViewRow>()
                            .Where(r => r.Cells["Müþteri"].Value.ToString().Equals(customer))
                            .Select(r => r.Index)
                            .FirstOrDefault();

                        if (rowIndex >= 0)
                        {
                            dataGridView1.Rows[rowIndex].Cells["Sipariþ Durumu"].Value = "Sipariþ hazýrlanýyor";
                        }
                    }));

                    AssignChefToOrder(customer);

                }
                else
                {
                    dataGridView1.Invoke(new Action(() =>
                    {
                        dataGridView1.Rows.Add(customer, "Sipariþ vermeyi bekliyor", " - ");
                    }));
                }
            }
        }

        private async void AssignChefToOrder(string customer)
        {
            string chef = GetAvailableChef();

            if (chef != null)
            {
                dataGridView1.Invoke(new Action(() =>
                {
                    int rowIndex = dataGridView1.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => r.Cells["Müþteri"].Value.ToString().Equals(customer))
                        .Select(r => r.Index)
                        .FirstOrDefault();

                    if (rowIndex >= 0)
                    {
                        dataGridView1.Rows[rowIndex].Cells["Sipariþi Hazýrlayan Aþçý"].Value = chef;
                    }
                }));

                await Task.Delay(7000);

                dataGridView1.Invoke(new Action(() =>
                {
                    int rowIndex = dataGridView1.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => r.Cells["Müþteri"].Value.ToString().Equals(customer))
                        .Select(r => r.Index)
                        .FirstOrDefault();

                    if (rowIndex >= 0)
                    {
                        dataGridView1.Rows[rowIndex].Cells["Sipariþ Durumu"].Value = "Sipariþ hazýr";
                    }
                }));

                ReleaseChef(chef);

                await Task.Delay(5000);

                dataGridView1.Invoke(new Action(() =>
                {
                    int rowIndex = dataGridView1.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => r.Cells["Müþteri"].Value.ToString().Equals(customer))
                        .Select(r => r.Index)
                        .FirstOrDefault();

                    if (rowIndex >= 0)
                    {
                        dataGridView1.Rows[rowIndex].Cells["Sipariþ Durumu"].Value = "Müþteri yemek yiyor";
                    }
                }));

                await Task.Delay(5000);

                dataGridView1.Invoke(new Action(() =>
                {
                    int rowIndex = dataGridView1.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => r.Cells["Müþteri"].Value.ToString().Equals(customer))
                        .Select(r => r.Index)
                        .FirstOrDefault();

                    if (rowIndex >= 0)
                    {
                        dataGridView1.Rows.RemoveAt(rowIndex);
                    }
                }));
                Kasa(customer);

            }
            else
            {
                dataGridView1.Invoke(new Action(() =>
                {
                    dataGridView1.Rows.Add(customer, "Aþçýnýn müsait olmasý bekleniyor", " - ");
                }));
            }
        }





        private async void Kasa(string customer)
        {
            string odemeYapanMusteri = customer;

            dataGridView2.Invoke((MethodInvoker)delegate
            {
                dataGridView2.Rows.Add(customer, "Ödeme alýnýyor");
            });

            await Task.Delay(5000);

            dataGridView2.Invoke(new Action(() =>
            {
                int rowIndex = dataGridView2.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells["Müþteri"].Value.ToString().Equals(customer))
                    .Select(r => r.Index)
                    .FirstOrDefault();

                if (rowIndex >= 0)
                {
                    dataGridView2.Rows[rowIndex].Cells["Ödeme Durumu"].Value = "Ödendi";
                }
            }));

            await Task.Delay(3000);

            dataGridView2.Invoke(new Action(() =>
            {
                int rowIndex = dataGridView2.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells["Müþteri"].Value.ToString().Equals(customer))
                    .Select(r => r.Index)
                    .FirstOrDefault();

                if (rowIndex >= 0)
                {
                    dataGridView2.Rows.RemoveAt(rowIndex);
                }

                if (customerTables.ContainsKey(odemeYapanMusteri))
                {
                    Panel customerTablePanel = customerTables[odemeYapanMusteri];
                    SetPanelText(customerTablePanel, "");
                    customerTables.Remove(odemeYapanMusteri);
                    int tableNumber = int.Parse(customerTablePanel.Name.Replace("panel", ""));

                    AddTableToEmptyList(tableNumber);
                    customerTables.Remove(odemeYapanMusteri);

                    if (waitingCustomers.Count > 0)
                    {
                        string nextCustomer = waitingCustomers.Dequeue();
                        AssignCustomerToTable(nextCustomer);
                        listedensilme(nextCustomer, waitingCustomers);

                        listView2.Invoke(new Action(() =>
                        {
                            var itemToRemove = listView2.Items.Cast<ListViewItem>()
                                .Where(item => item.Text == nextCustomer)
                                .FirstOrDefault();

                            if (itemToRemove != null)
                            {
                                listView2.Items.Remove(itemToRemove);
                            }
                        }));

                        StartService();
                    }
                }


            }));

        }

        private void listedensilme(string customer, Queue<string> waitingCustomers)
        {
            waitingCustomers = new Queue<string>(waitingCustomers.Where(c => c != customer));
        }


        private void PrepareOrder()
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView3.Visible = false;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox13_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void problem2Button_Click(object sender, EventArgs e)
        {
            Form2 problem2Form = new Form2();
            problem2Form.Show();
        }
    }
}