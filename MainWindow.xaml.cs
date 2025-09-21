using BooksClient.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace BooksClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string BaseUrl = "http://localhost:5043";

        private readonly HttpClient _http = new() { BaseAddress = new Uri(BaseUrl) };
        private readonly ObservableCollection<Book> _books = new();

        public MainWindow()
        {
            InitializeComponent();
            BooksGrid.ItemsSource = _books;
            Loaded += async (_, __) => await LoadBooksAsync();
        }

        private async Task LoadBooksAsync()
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<Book>>("api/books");
                _books.Clear();
                if (list != null) foreach (var b in list) _books.Add(b);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load failed: " + ex.Message);
            }
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var book = new Book
            {
                ISBN = txtISBN.Text.Trim(),
                Name = txtName.Text.Trim(),
                Author = txtAuthor.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            if (string.IsNullOrWhiteSpace(book.ISBN) ||
                string.IsNullOrWhiteSpace(book.Name) ||
                string.IsNullOrWhiteSpace(book.Author))
            {
                MessageBox.Show("ISBN, Name, and Author are required.");
                return;
            }

            try
            {
                var resp = await _http.PostAsJsonAsync("api/books", book);
                if (resp.IsSuccessStatusCode)
                {
                    await LoadBooksAsync();
                    ClearInputs();
                }
                else
                {
                    var msg = await resp.Content.ReadAsStringAsync();
                    MessageBox.Show($"Add failed: {resp.StatusCode}\n{msg}");
                }
            }
            catch (Exception ex) { MessageBox.Show("Add failed: " + ex.Message); }
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            var book = new Book
            {
                ISBN = txtISBN.Text.Trim(),
                Name = txtName.Text.Trim(),
                Author = txtAuthor.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            if (string.IsNullOrWhiteSpace(book.ISBN))
            {
                MessageBox.Show("Select a row or enter an ISBN to update.");
                return;
            }

            try
            {
                var resp = await _http.PutAsJsonAsync($"api/books/{book.ISBN}", book);
                if (resp.IsSuccessStatusCode || resp.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    await LoadBooksAsync();
                    ClearInputs();
                }
                else
                {
                    var msg = await resp.Content.ReadAsStringAsync();
                    MessageBox.Show($"Update failed: {resp.StatusCode}\n{msg}");
                }
            }
            catch (Exception ex) { MessageBox.Show("Update failed: " + ex.Message); }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var isbn = txtISBN.Text.Trim();
            if (string.IsNullOrWhiteSpace(isbn))
            {
                MessageBox.Show("Select a row or enter ISBN to delete.");
                return;
            }

            if (MessageBox.Show($"Delete {isbn}?", "Confirm",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                var resp = await _http.DeleteAsync($"api/books/{isbn}");
                if (resp.IsSuccessStatusCode || resp.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    await LoadBooksAsync();
                    ClearInputs();
                }
                else
                {
                    var msg = await resp.Content.ReadAsStringAsync();
                    MessageBox.Show($"Delete failed: {resp.StatusCode}\n{msg}");
                }
            }
            catch (Exception ex) { MessageBox.Show("Delete failed: " + ex.Message); }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadBooksAsync();

        private void BooksGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (BooksGrid.SelectedItem is Book b)
            {
                txtISBN.Text = b.ISBN;
                txtName.Text = b.Name;
                txtAuthor.Text = b.Author;
                txtDescription.Text = b.Description ?? "";
            }
        }

        private void ClearInputs()
        {
            txtISBN.Clear(); txtName.Clear(); txtAuthor.Clear(); txtDescription.Clear();
            txtISBN.Focus();
        }
    }
}