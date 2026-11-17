using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Akhmetgareev41
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private User currentUser;

        public ProductPage(User user)
        {
            InitializeComponent();
            currentUser = user;

            if (user != null)
            {
                FIOTB.Text = "Вы авторизованы как " + user.FullName;
                switch (user.UserRole)
                {
                    case 1:
                        RoleTB.Text = "Роль: Клиент"; break;
                    case 2:
                        RoleTB.Text = "Роль: Менеджер"; break;
                    case 3:
                        RoleTB.Text = "Роль: Администратор"; break;
                }

                if (user.UserRole == 2 || user.UserRole == 3)
                {
                    AddProductBtn.Visibility = Visibility.Visible;
                }
            }
            else
            {
                FIOTB.Text = "Вы авторизованы как гость";
                RoleTB.Text = "Роль: Гость";
            }

            ComboType.SelectedIndex = 0;
            UpdateProducts();
        }

        private void UpdateProducts()
        {
            var context = Akhmetgareev41Entities.GetContext();
            var query = context.Product.AsQueryable();

            // Поиск по наименованию
            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
            {
                string search = TBoxSearch.Text.Trim().ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(search));
            }

            // Фильтрация по скидке
            switch (ComboType.SelectedIndex)
            {
                case 1:
                    query = query.Where(p => p.ProductDiscountAmount >= 0 &&
                                             p.ProductDiscountAmount < 10);
                    break;
                case 2:
                    query = query.Where(p => p.ProductDiscountAmount >= 10 &&
                                             p.ProductDiscountAmount < 15);
                    break;
                case 3:
                    query = query.Where(p => p.ProductDiscountAmount >= 15);
                    break;
            }

            // Сортировка по стоимости
            if (RButtonUp.IsChecked == true)
            {
                query = query.OrderBy(p => p.ProductCost);
            }
            if (RButtonDown.IsChecked == true)
            {
                query = query.OrderByDescending(p => p.ProductCost);
            }

            var currentProducts = query.ToList();

            TBlockCount.Text = $"кол-во {currentProducts.Count} из {context.Product.Count()}";
            ProductListView.ItemsSource = currentProducts;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void AddProductBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEditProducts())
            {
                MessageBox.Show("Недостаточно прав");
                return;
            }

            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenEditPage();
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null || currentUser.UserRole != 3)
            {
                MessageBox.Show("Удалять товары может только администратор");
                return;
            }

            Product product = ProductListView.SelectedItem as Product;
            if (product == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            if (MessageBox.Show(
                    $"Удалить товар «{product.ProductName}»?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            var context = Akhmetgareev41Entities.GetContext();

            try
            {
                context.Product.Remove(product);
                context.SaveChanges();
                UpdateProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void ProductListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenEditPage();
        }

        private void OpenEditPage()
        {
            if (!CanEditProducts())
            {
                MessageBox.Show("Недостаточно прав");
                return;
            }

            Product product = ProductListView.SelectedItem as Product;
            if (product == null)
            {
                return;
            }

            Manager.MainFrame.Navigate(new AddEditPage(product));
        }

        private bool CanEditProducts()
        {
            return currentUser != null &&
                   (currentUser.UserRole == 2 || currentUser.UserRole == 3);
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Product product = ProductListView.SelectedItem as Product;
            if (product == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            if (product.ProductQuantityInStock <= 0)
            {
                MessageBox.Show("Товара нет в наличии");
                return;
            }

            product.Quantity = 1;

            var selectedProducts = new List<Product> { product };
            var selectedOrderProducts = new List<OrderProduct>();

            string fio = currentUser == null
                ? "Гость"
                : currentUser.FullName;

            OrderWindow window = new OrderWindow(
                selectedOrderProducts,
                selectedProducts,
                fio,
                currentUser);

            window.ShowDialog();
            UpdateProducts();
        }
    }
}
