using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Akhmetgareev41
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Product currentProduct;
        private bool isEdit;

        public AddEditPage(Product product = null)
        {
            InitializeComponent();

            isEdit = product != null;
            currentProduct = product ?? new Product();

            if (isEdit)
            {
                Title = "Редактирование товара";

                ArticleTB.Text = currentProduct.ProductArticleNumber;
                ArticleTB.IsReadOnly = true;

                NameTB.Text = currentProduct.ProductName;
                UnitTB.Text = currentProduct.ProductUnit;
                CostTB.Text = currentProduct.ProductCost.ToString();
                MaxDiscountTB.Text = currentProduct.ProductMaxDiscount.ToString();
                ManufacturerTB.Text = currentProduct.ProductManufacturer;
                SupplierTB.Text = currentProduct.ProductSupplier;
                CategoryTB.Text = currentProduct.ProductCategory;
                DiscountTB.Text = currentProduct.ProductDiscountAmount.ToString();
                QuantityTB.Text = currentProduct.ProductQuantityInStock.ToString();
                DescriptionTB.Text = currentProduct.ProductDescription;
                PhotoTB.Text = currentProduct.ProductPhoto ?? "";
            }
            else
            {
                Title = "Добавление товара";
                DiscountTB.Text = "0";
                MaxDiscountTB.Text = "0";
                QuantityTB.Text = "0";
            }

            UpdatePhoto();
        }

        private void UpdatePhoto()
        {
            string photo = currentProduct.ProductPhoto;

            if (string.IsNullOrWhiteSpace(photo))
            {
                photo = "picture.png";
            }

            try
            {
                string filePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "img", photo);

                if (File.Exists(filePath))
                {
                    PhotoImage.Source = new BitmapImage(new Uri(filePath));
                }
                else
                {
                    PhotoImage.Source = new BitmapImage(
                        new Uri("pack://application:,,,/img/" + photo));
                }
            }
            catch
            {
                PhotoImage.Source = null;
            }
        }

        private void SelectPhotoBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            string fileName = Path.GetFileName(dialog.FileName);
            string imgDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "img");

            Directory.CreateDirectory(imgDir);

            string targetPath = Path.Combine(imgDir, fileName);
            File.Copy(dialog.FileName, targetPath, true);

            currentProduct.ProductPhoto = fileName;
            PhotoTB.Text = fileName;
            UpdatePhoto();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ArticleTB.Text) ||
                string.IsNullOrWhiteSpace(NameTB.Text) ||
                string.IsNullOrWhiteSpace(UnitTB.Text) ||
                string.IsNullOrWhiteSpace(ManufacturerTB.Text) ||
                string.IsNullOrWhiteSpace(SupplierTB.Text) ||
                string.IsNullOrWhiteSpace(CategoryTB.Text) ||
                string.IsNullOrWhiteSpace(DescriptionTB.Text))
            {
                MessageBox.Show("Заполните обязательные поля");
                return;
            }

            if (!decimal.TryParse(CostTB.Text, out decimal cost) || cost < 0)
            {
                MessageBox.Show("Цена должна быть неотрицательным числом");
                return;
            }

            if (!double.TryParse(MaxDiscountTB.Text, out double maxDiscount) ||
                maxDiscount < 0 || maxDiscount > 100)
            {
                MessageBox.Show("Максимальная скидка должна быть от 0 до 100");
                return;
            }

            if (!double.TryParse(DiscountTB.Text, out double discount) ||
                discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка должна быть от 0 до 100");
                return;
            }

            if (discount > maxDiscount)
            {
                MessageBox.Show("Скидка не может быть больше максимальной");
                return;
            }

            if (!int.TryParse(QuantityTB.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным целым числом");
                return;
            }

            var context = Akhmetgareev41Entities.GetContext();

            if (!isEdit && context.Product.Any(
                    p => p.ProductArticleNumber == ArticleTB.Text.Trim()))
            {
                MessageBox.Show("Товар с таким артикулом уже существует");
                return;
            }

            Product product;

            if (isEdit)
            {
                product = context.Product.FirstOrDefault(
                    p => p.ProductArticleNumber ==
                         currentProduct.ProductArticleNumber);

                if (product == null)
                {
                    MessageBox.Show("Товар не найден в базе");
                    return;
                }
            }
            else
            {
                product = new Product();
                context.Product.Add(product);
            }

            product.ProductArticleNumber = ArticleTB.Text.Trim();
            product.ProductName = NameTB.Text.Trim();
            product.ProductUnit = UnitTB.Text.Trim();
            product.ProductCost = cost;
            product.ProductMaxDiscount = maxDiscount;
            product.ProductManufacturer = ManufacturerTB.Text.Trim();
            product.ProductSupplier = SupplierTB.Text.Trim();
            product.ProductCategory = CategoryTB.Text.Trim();
            product.ProductDiscountAmount = discount;
            product.ProductQuantityInStock = quantity;
            product.ProductDescription = DescriptionTB.Text.Trim();
            product.ProductPhoto = string.IsNullOrWhiteSpace(PhotoTB.Text)
                ? null
                : PhotoTB.Text.Trim();

            try
            {
                context.SaveChanges();
                MessageBox.Show("Товар сохранён");

                if (Manager.MainFrame.CanGoBack)
                {
                    Manager.MainFrame.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.MainFrame.CanGoBack)
            {
                Manager.MainFrame.GoBack();
            }
        }
    }
}
