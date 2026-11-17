using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Akhmetgareev41
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private string captchaCode;
        private bool captchaRequired = false;
        private readonly Random random = new Random();

        public AuthPage()
        {
            InitializeComponent();
        }

        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            User user = null;
            Manager.MainFrame.Navigate(new ProductPage(user));
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLogin.Text.Trim();
            string password = UserPassword.Password;
            if (login == "" || password == "")
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }
            
            if (captchaRequired)
            {
                string enteredCaptcha = CaptchaInput.Text.Trim();
                if (enteredCaptcha == "")
                {
                    MessageBox.Show("Введите капчу");
                    return;
                }
                if (!string.Equals(enteredCaptcha, captchaCode, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Капча введена неверно, вход заблокирован на 10 секунд");
                    await BlockLoginAsync();
                    return;
                }
            }

            User user = Akhmetgareev41Entities.GetContext().User
                .FirstOrDefault(p => p.UserLogin == login && p.UserPassword == password);
            if (user != null)
            {
                Manager.MainFrame.Navigate(new ProductPage(user));
                UserLogin.Text = "";
                UserPassword.Password = "";
                CaptchaInput.Text = "";
                captchaRequired = false;
                CaptchaPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!captchaRequired)
                {
                    captchaRequired = true;
                    CaptchaPanel.Visibility = Visibility.Visible;
                    GenerateCaptcha();
                    MessageBox.Show("Неуспешная авторизация. Введите капчу");
                }
                else
                {
                    MessageBox.Show("Введены неккоретные данные");
                    GenerateCaptcha();
                }
                UserPassword.Password = "";
                CaptchaInput.Text = "";
            }
        }

        private void GenerateCaptcha()
        {
            const string symbols = "abcefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            captchaCode = new string(Enumerable.Range(0, 4)
                .Select(_ => symbols[random.Next(symbols.Length)]).ToArray());

            TextBlock[] blocks = { captchaOneWord, captchaTwoWord, captchaThreeWord, captchaFourWord };

            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].Text = captchaCode[i].ToString();
               
                blocks[i].TextDecorations = random.Next(2) == 0
                    ? TextDecorations.Strikethrough : null;
                
                blocks[i].Margin = new Thickness(random.Next(-8, 4), random.Next(0, 9), 0, 0);
                
                blocks[i].RenderTransform = new RotateTransform(random.Next(0, 21));
            }
        }

        private async Task BlockLoginAsync()
        {
            LoginBtn.IsEnabled = false;
            GuestBtn.IsEnabled = false;
            for (int seconds = 10; seconds > 0; seconds--)
            {
                LoginBtn.Content = $"Подождите {seconds} с";
                await Task.Delay(1000);
            }
            LoginBtn.Content = "Войти";
            LoginBtn.IsEnabled = true;
            GuestBtn.IsEnabled = true;
            GenerateCaptcha();
            CaptchaInput.Text = "";
        }
    }
}
