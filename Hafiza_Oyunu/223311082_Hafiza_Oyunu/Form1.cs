using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _223311082_Hafiza_Oyunu
{
    public partial class Form1 : Form
    {
        private PictureBox ilktiklama = null;  // İlk tıklanan resmi tutmak için
        private PictureBox ikincitiklama = null; // İkinci tıklanan resmi tutmak için
        private Random random = new Random();
        private Timer timer = new Timer();
        private List<Image> images = new List<Image>();
        private int currentPlayer = 1; // 1. oyuncu ile başla
        private int player1Score = 0;
        private int player2Score = 0;
        private Timer startTimer = new Timer();  // Oyun başlangıcı için timer
        private Timer selectionTimer = new Timer(); // Seçim süresi için timer
        private int selectionTimeLimit = 5; // 5 saniye
        private int timeRemaining; // Kalan süre
        private Label timeRemainingLabel; // Kalan süreyi göstermek için label

        public Form1()
        {
            InitializeComponent();
            LoadImages();
            AssignImagesToPictureBoxes();

            timer.Interval = 1200;  // Eşleşme olmadığında resimleri gizlemek için bekleme süresi
            timer.Tick += Timer_Tick;

            // Oyun başladığında tüm resimleri göster
            ShowAllImages();

            // 5 saniyelik bir süre tanımla
            startTimer.Interval = 5000; // 5 saniye
            startTimer.Tick += StartTimer_Tick;  // Timer süresi dolduğunda tetiklenecek olay
            startTimer.Start();  // Timer'ı başlat

            // Seçim süresi için ayarlamalar
            selectionTimer.Interval = 1000; // 1 saniye aralıklarla çalışacak
            selectionTimer.Tick += SelectionTimer_Tick;

            // Kalan süre label'ı oluşturma
            timeRemainingLabel = new Label();
            timeRemainingLabel.Location = new Point(250, 70); // Label'in konumunu ayarla
            timeRemainingLabel.AutoSize = true;
            this.Controls.Add(timeRemainingLabel);
        }

        private void LoadImages()
        {
            for (int i = 1; i <= 20; i++)
            {
                try
                {
                    // Her resim için dosya yolunu belirtiyoruz
                    Image resim = Image.FromFile($"resim{i}.jpeg");
                    // Aynı resimden iki adet ekliyoruz
                    images.Add(resim);
                    images.Add(resim); // Çift oluşturmak için aynı resmi iki kez ekle
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show("Resim dosyası bulunamadı: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bir hata oluştu: " + ex.Message);
                }
            }
        }

        private void AssignImagesToPictureBoxes()
        {
            List<Image> randomizedImages = images.OrderBy(x => random.Next()).ToList();
            int index = 0;
            foreach (Control control in this.Controls)
            {
                if (control is PictureBox)
                {
                    PictureBox pictureBox = (PictureBox)control;
                    pictureBox.Tag = randomizedImages[index];  // Resmi tag olarak saklıyoruz
                    pictureBox.Click += PictureBox_Click;
                    index++;
                }
            }
        }

        // Resme tıklama olayı
        private void PictureBox_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
                return;

            PictureBox clickedBox = sender as PictureBox;

            if (clickedBox == null || clickedBox.Image != null)
                return;

            if (ilktiklama == null)
            {
                ilktiklama = clickedBox;
                ilktiklama.Image = (Image)ilktiklama.Tag;

                // Seçim süresini başlat
                timeRemaining = selectionTimeLimit;
                timeRemainingLabel.Text = $"Kalan Süre: {timeRemaining} saniye";
                selectionTimer.Start(); // Seçim süresi başlat
                return;
            }

            ikincitiklama = clickedBox;
            ikincitiklama.Image = (Image)ikincitiklama.Tag;

            // Eğer eşleşme varsa
            if (ilktiklama.Tag == ikincitiklama.Tag)
            {
                UpdateScore();
                ResetClicks();
            }
            else
            {
                timer.Start();
            }
        }

        // Timer olayı (eşleşme olmazsa resimleri gizler)
        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            ilktiklama.Image = null;
            ikincitiklama.Image = null;
            SwitchPlayer();
            ResetClicks();
        }

        // Seçim süresi timer olayı
        private void SelectionTimer_Tick(object sender, EventArgs e)
        {
            timeRemaining--; // Kalan süreyi bir azalt

            if (timeRemaining < 0)
            {
                selectionTimer.Stop(); // Zamanlayıcıyı durdur

                // İlk seçilen kutunun görselini gizle
                if (ilktiklama != null)
                {
                    ilktiklama.Image = null; // İlk kutunun resmini temizle
                }

                SwitchPlayer(); // Oyuncu değiştir
                ResetClicks(); // Seçimleri sıfırla
                timeRemainingLabel.Text = ""; // Süreyi temizle
                return;
            }

            // Kalan süreyi güncelle
            timeRemainingLabel.Text = $"Kalan Süre: {timeRemaining} saniye";
        }

        // Sıra değiştirme
        private void SwitchPlayer()
        {
            // Mevcut kutuları kapat
            if (ilktiklama != null)
            {
                ilktiklama.Image = null; // İlk kutunun resmini temizle
            }
            if (ikincitiklama != null)
            {
                ikincitiklama.Image = null; // İkinci kutunun resmini temizle
            }

            // Oyuncu değişimini UI'da göster
            currentPlayer = currentPlayer == 1 ? 2 : 1;

            if (currentPlayer == 1)
            {
                label3.Text = "1";
            }
            else
            {
                label3.Text = "2";
            }
        }

        // Puan güncelleme
        private void UpdateScore()
        {
            if (currentPlayer == 1)
            {
                player1Score++;
                label1.Text = player1Score.ToString();
            }
            else
            {
                player2Score++;
                label2.Text = player2Score.ToString();
            }

            // Oyun bitiş kontrolü
            if (IsGameOver())
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            string winner;

            if (player1Score > player2Score)
            {
                winner = "Oyuncu 1 kazandı!";
            }
            else if (player2Score > player1Score)
            {
                winner = "Oyuncu 2 kazandı!";
            }
            else
            {
                winner = "Oyun berabere!";
            }

            // Sonucu göster
            MessageBox.Show(winner + "\nOyuncu 1 Puan: " + player1Score + "\nOyuncu 2 Puan: " + player2Score, "Oyun Bitti");

            // Oyun bittiği için tüm resimleri göstermeye devam edebiliriz
            ShowAllImages();
        }

        // Seçilen resimleri sıfırlama
        private void ResetClicks()
        {
            ilktiklama = null;
            ikincitiklama = null;
            selectionTimer.Stop(); // Seçim zamanlayıcısını durdur
            timeRemainingLabel.Text = ""; // Süreyi temizle
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label3.Text = "1";
        }

        private void ShowAllImages()
        {
            foreach (Control control in this.Controls)
            {
                if (control is PictureBox)
                {
                    PictureBox pictureBox = (PictureBox)control;
                    pictureBox.Image = (Image)pictureBox.Tag;  // Resmi göster
                }
            }
        }

        private void StartTimer_Tick(object sender, EventArgs e)
        {
            // Timer'ı durdur
            startTimer.Stop();

            // Tüm resimleri gizle
            HideAllImages();
        }

        private void HideAllImages()
        {
            foreach (Control control in this.Controls)
            {
                if (control is PictureBox)
                {
                    PictureBox pictureBox = (PictureBox)control;
                    pictureBox.Image = null;  // Resmi gizle
                }
            }
        }

        private bool IsGameOver()
        {
            foreach (Control control in this.Controls)
            {
                if (control is PictureBox)
                {
                    PictureBox pictureBox = (PictureBox)control;
                    // Eğer herhangi bir PictureBox'ın resmi gizliyse, oyun bitmemiş demektir
                    if (pictureBox.Image == null)
                    {
                        return false; // Oyun bitmemiş
                    }
                }
            }
            return true; // Tüm resimler eşleşmiş
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            label3.Text = "1";
        }
    }
}
