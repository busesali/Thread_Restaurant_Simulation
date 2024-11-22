using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yazlab
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int totalCustomers = int.Parse(textBox1.Text); // Müşteri sayısı
            int intervalSeconds = int.Parse(textBox2.Text); // Müşteri gelme aralığı (saniye cinsinden)
            int totalSeconds = int.Parse(textBox3.Text); // Toplam süre (saniye cinsinden)

            int priorityCustomers = (totalSeconds / intervalSeconds)+ totalCustomers; // Öncelikli müşteri sayısı
            int normalCustomers = (totalSeconds / intervalSeconds) * (totalCustomers - 1); // Normal müşteri sayısı (Öncelikli müşteri hariç)

            
            int tableCount = 6;
            int waiterCount = 3;
            int chefCount = 2;

            // Maliyetlerin hesaplanması
            int tableCost = tableCount;
            int waiterCost = waiterCount;
            int chefCost = chefCount;

            // Müşteri başına elde edilen kazanç
            int customerProfit = 1;

            int totalCost = tableCost + waiterCost + chefCost;
            int totalProfit = (priorityCustomers * customerProfit) + (normalCustomers * customerProfit);

            int netProfit = totalProfit - totalCost;

            MessageBox.Show($"Maliyet: {totalCost}, Kazanç: {totalProfit}, Net Kar: {netProfit}");
        }
    }
}
