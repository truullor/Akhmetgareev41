using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Akhmetgareev41
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        private List<Product> selectedProducts = new List<Product>();
        private User currentUser;

        public OrderWindow()
        {
            InitializeComponent();
        }

        public OrderWindow(List<OrderProduct> selectedOrderProducts,
                           List<Product> selectedProducts,
                           string fio,
                           User user = null)
        {
            InitializeComponent();

            this.selectedOrderProducts = selectedOrderProducts ?? new List<OrderProduct>();
            this.selectedProducts = selectedProducts ?? new List<Product>();
            currentUser = user;

            ClientTB.Text = fio;

            TBOrderID.Text = this.selectedOrderProducts.Count > 0
                ? this.selectedOrderProducts.First().OrderID.ToString()
                : "новый";

            foreach (Product product in this.selectedProducts)
            {
                if (product.Quantity <= 0)
                {
                    product.Quantity = 1;
                }
            }

            ProductList.ItemsSource = this.selectedProducts;

            var currentPickups = Akhmetgareev41Entities.GetContext()
                .PickUpPoint.ToList();

            PickupCombo.ItemsSource = currentPickups;
            PickupCombo.SelectedIndex = 0;

            OrderDP.SelectedDate = DateTime.Now.AddDays(3);

            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = 0;

            foreach (Product product in selectedProducts)
            {
                int quantity = product.Quantity > 0 ? product.Quantity : 1;
                decimal price = product.ProductCost *
                    (1 - (decimal)product.ProductDiscountAmount / 100);

                total += price * quantity;
            }

            TotalTB.Text = $"Итого: {total:0.00} руб.";
        }

        private void Quantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotal();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("В заказе нет товаров");
                return;
            }

            if (PickupCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите пункт выдачи");
                return;
            }

            if (OrderDP.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату выдачи");
                return;
            }

            foreach (Product product in selectedProducts)
            {
                if (product.Quantity <= 0)
                {
                    MessageBox.Show("Количество должно быть больше нуля");
                    return;
                }

                if (product.Quantity > product.ProductQuantityInStock)
                {
                    MessageBox.Show(
                        $"Товара «{product.ProductName}» недостаточно на складе");
                    return;
                }
            }

            var context = Akhmetgareev41Entities.GetContext();
            var pickup = (PickUpPoint)PickupCombo.SelectedItem;

            Order order = new Order
            {
                OrderBuyDate = DateTime.Now,
                OrderDeliveryDate = OrderDP.SelectedDate.Value,
                OrderPickupPoint = pickup.PickupPointID,
                OrderUserID = currentUser?.UserID,
                OrderCode = new Random().Next(100000, 999999),
                OrderStatus = 1
            };

            foreach (Product product in selectedProducts)
            {
                order.OrderProduct.Add(new OrderProduct
                {
                    ProductArticleNumber = product.ProductArticleNumber,
                    Count = product.Quantity
                });

                product.ProductQuantityInStock -= product.Quantity;
            }

            try
            {
                context.Order.Add(order);
                context.SaveChanges();

                MessageBox.Show("Заказ оформлен");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка оформления заказа: " + ex.Message);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
