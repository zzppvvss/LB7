using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace LB7
{
    internal class Database
    {
        private static string connecting = "Data Source=(localdb)\\MyDB;Initial Catalog=MyDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        // Метод для виконання SQL-запиту SELECT і повернення результатів у вигляді SqlDataReader
        public static SqlDataReader ExecuteQuery(string query)
        {
            // Створення об'єкта підключення SqlConnection з використанням рядка підключення
            SqlConnection connection = new SqlConnection(connecting);
            // Створення об'єкта команди SqlCommand з SQL-запитом і об'єктом підключення
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader result = command.ExecuteReader();
            return result;
        }

        // Метод для виконання SQL-запиту INSERT і повернення кількості змінених рядків
        public static int ExecuteInsertQueryAsync(string query)
        {
            using (SqlConnection connection = new SqlConnection(connecting))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            int rownum = command.ExecuteNonQuery();
                            transaction.Commit();
                            return rownum;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Помилка", ex.Message);
                        throw;
                    }
                }
            }
        }

        // Метод для відключення від бази даних і закриття з'єднання
        public static void Disconnect()
        {
            using (SqlConnection connection = new SqlConnection(connecting))
            {
                connection.Close();
            }
        }

    }
}