using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Грузчик
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
            pictureBox1.Click += (s, e) => { AddQueue(textBox1.Text); };
        }

        public void AddQueue(string url)
        {
            using (WebClient wb = new WebClient())
            {
                /*Почему-то у меня при работе с вебклиентом постоянно проблемы с большинством сайтов, 
                  выскакивает ошибка при подключении по защищенному каналу SSL, TLS и пр. Столбец ниже решает эту проблему. Хотя пишут что це небезопасно. 
                  Странно что у других и без этой херни работает.*/
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                try
                {
                    wb.OpenRead(url);
                }
                catch
                {
                    MessageBox.Show("Это не ссылка");
                    return;
                }
                string name = Path.GetFileName(url);
                string size = "";
                double dsize = (Convert.ToDouble(wb.ResponseHeaders["Content-Length"]) / 1048576);
                if (dsize > 1)  // если больше одного мб
                    size = dsize.ToString("#.# МБ"); //Покажи в мбайтах.
                else
                    size = (dsize * 1024).ToString("#.# КБ"); //если меньше 1мб то покажи в кбайтах.

                string[] str = new string[] { size, url }; //Массив для подобъектов в listView.
                listView1.Items.Add(name).SubItems.AddRange(str);
                textBox1.Text = "";
            }
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            if(listView1.Items.Count <= 0)
            {
                MessageBox.Show("Загрузка завершена");
                return; //Если больше нечего загружать то не выполнять дальше.
            }
            using(WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += (f, r) => 
                {
                    progressBar1.Value = r.ProgressPercentage;  label2.Text = $"Сейчас идёт загрузка: {listView1.Items[0].Text}. Размер : {listView1.Items[0].SubItems[1].Text}";
                    speed = r.BytesReceived;
                    label3.Text = $"Скорость: {((double)sum2 / 1024).ToString("#.# КБ/сек")}";
                };
                wc.DownloadFileCompleted += (f, r) =>
                {
                    try
                    {
                        listView1.Items.RemoveAt(0);
                        PictureBox2_Click(null, null);
                    }
                    catch
                    {
                        MessageBox.Show("Загрузки завершены");
                    }
                    label2.Text = "Сейчас ничего не происходит.";
                    label3.Text = "";
                };
                wc.DownloadFileAsync(new Uri(listView1.Items[0].SubItems[2].Text), listView1.Items[0].Text);
                timer1.Start(); //Запуск таймера
            }
        }

        long speed = 0;
        long sum = 0;
        long sum2 = 0;
        private void Timer1_Tick(object sender, EventArgs e) //таймер чтобы расчитать скорость в секунду.
        {
            sum2 = speed - sum;
            sum = speed;
        }
    }
}