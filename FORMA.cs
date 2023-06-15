using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LB7
{
  public partial class FORMA : Form
  {
    private BindingList<Author> Authors = new BindingList<Author>();
    private BindingList<string> Authorsnames = new BindingList<string>();
    private BindingList<Book> Books = new BindingList<Book> ();
    private BindingList<string> Booksnames = new BindingList<string>();

    public FORMA()
    {
      InitializeComponent();
      this.FormClosed += FORMAClosed;
    }

    private void FORMALoading(object sender, System.EventArgs e)
    {
      LoadingBooks();
      LoadingAuthors();
      Updating();
    }

    private void FORMAClosed(object sender, FormClosedEventArgs e)
    {
      Database.Disconnect();
    }

    private void Updating ()
    {
      // Authors
      Authorsnames = new BindingList<string>(Authors.Select(a => a.Name).ToList());

      comboBox1.DataSource = Authorsnames;
      comboBox4.DataSource = Authorsnames;

      // Books
      Booksnames = new BindingList<string>(Books.Select(a => a.Title).ToList());

      comboBox2.DataSource = Booksnames;
      comboBox3.DataSource = Booksnames;
    }

       private void LoadingBooks()
    {
      Books = new BindingList<Book>();  

      using (SqlDataReader result = Database.ExecuteQuery("SELECT * FROM books"))
      {
        while (result.Read())
        {
          string id = result.GetInt32(0).ToString();  // Отримуємо ідентифікатор книги з першого стовпця результатів запиту
          string title = result.GetString(1);  // Отримуємо назву книги з другого стовпця результатів запиту

          Book book = new Book(id, title);  
          Books.Add(book);  // Додаємо об'єкт Book до колекції Books
        }
      }
    }

       private void LoadingAuthors()
    {
        Authors = new BindingList<Author>();  // Створюємо новий екземпляр BindingList для зберігання авторів

        using (SqlDataReader result = Database.ExecuteQuery("SELECT * FROM authors"))
        {
            while (result.Read())
            {
                string id = result.GetInt32(0).ToString();  // Отримуємо ідентифікатор та імя автора з першого стовпця результатів запиту
                string name = result.GetString(1);  

                Author author = new Author(id, name);  
                Authors.Add(author);  // Додаємо об'єкт Author до колекції Authors
            }
        }
    }
        private async void AddAuthors(object sender, EventArgs e)
        {
            string name = textBox1.Text;  // Отримуємо ім'я автора з текстового поля textBox1

            int rows = await Task.Run(() =>
            {
                // Виконуємо асинхронний запит до бази даних для вставки нового автора в таблицю authors
                return Database.ExecuteInsertQueryAsync($@"
                INSERT INTO authors (name)
                VALUES ('{name}');");
            });

            MessageBox.Show(" Змінено"); 

            LoadingBooks();  
            LoadingAuthors(); 
            Updating(); 
        }

        private async void AddBooks(object sender, EventArgs e)
        {
            string title = textBox2.Text; 

            int rows = await Task.Run(() =>
            {
                // Виконуємо асинхронний запит до бази даних для вставки нової книги в таблицю books
                return Database.ExecuteInsertQueryAsync($@"
                    INSERT INTO books (title)
                    VALUES ('{title}');");
            });

            MessageBox.Show(" Додано до списку."); 

            LoadingBooks();  
            LoadingAuthors();  
            Updating();  
        }
     private void SelectAuthor(object sender, EventArgs e)
    {
       
        string selectedAuthor = comboBox1.SelectedItem.ToString();

        // Виконання SQL-запиту до бази даних для отримання списку назв книг, пов'язаних з обраним автором
        using (SqlDataReader result = Database.ExecuteQuery($@"
            SELECT books.title FROM books
            INNER JOIN book_authors ON books.id = book_authors.book_id
            INNER JOIN authors ON book_authors.author_id = authors.id
            WHERE authors.name = '{selectedAuthor}';"))
        {
            // Очищення listView1 перед заповненням його новими даними
            listView1.Items.Clear();

            // Проходження через результати запиту та відображення назв книг у listView1
            while (result.Read())
            {
                string title = result.GetString(0);
                listView1.Items.Add(new ListViewItem(title));
            }

            result.Close();  
        }
    }
    
    private void SelectBook(object sender, EventArgs e)
    {
        
        string selectedBook = comboBox3.SelectedItem.ToString();

        using (SqlDataReader result = Database.ExecuteQuery($@"
            SELECT authors.name FROM books
            JOIN book_authors ON books.id = book_authors.book_id
            JOIN authors ON book_authors.author_id = authors.id
            WHERE books.title = '{selectedBook}';"))
        {
         
            listView2.Items.Clear();

            // Проходження через результати запиту та відображення імен авторів у listView2
            while (result.Read())
            {
                string name = result.GetString(0);
               
                listView2.Items.Add(new ListViewItem(name));
            }

            result.Close();  
        }
    }

  private void Relation(object sender, EventArgs e)
    {
       
        string author = comboBox4.SelectedItem.ToString();
        string book = comboBox2.SelectedItem.ToString();

        // Отримання ідентифікатора автора за його іменем
        string authorId = Authors.FirstOrDefault(a => a.Name == author)?.Id;
        string bookId = Books.FirstOrDefault(b => b.Title == book)?.Id;

        try
        {
            if (authorId != null && bookId != null)
            {
                // Виконання SQL-запиту до бази даних для створення зв'язку між книгою та автором в таблиці book_authors
                int rowsAffected = Database.ExecuteInsertQueryAsync($@"
                    INSERT INTO book_authors (book_id, author_id)
                    VALUES ('{bookId}', '{authorId}'); ");

                MessageBox.Show(" Додано зв'язоок");
                Updating();
            }
            else
            {
                MessageBox.Show("Невірний автор або книга");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Помилка при додаванні зв'язку");
        }
    }

        private void Deleter(object sender, KeyEventArgs e)
        {
            ListView listViewControl = sender as ListView;

            // Перевірка, чи натиснута клавіша Delete
            if (e.KeyCode == Keys.Delete)
            {
                // Створення копії обраних елементів у ListView
                ListViewItem[] selectedItems = new ListViewItem[listViewControl.SelectedItems.Count];
                listViewControl.SelectedItems.CopyTo(selectedItems, 0);

                // Видалення обраних елементів з ListView
                foreach (ListViewItem selectedItem in selectedItems)
                {
                    listViewControl.Items.Remove(selectedItem);
                }
            }
        }
    }
}