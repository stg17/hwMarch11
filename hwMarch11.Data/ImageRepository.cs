using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace hwMarch11.Data
{
    public class ImageRepository
    {
        private readonly string _connectionString;
        public ImageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddImage(Image image)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO Images
                                    VALUES (@path, @pswd, 0)
                                    SELECT SCOPE_IDENTITY()";
            command.Parameters.AddWithValue("@path", image.ImagePath);
            command.Parameters.AddWithValue("@pswd", image.Password);
            connection.Open();
            image.Id = (int)(decimal)command.ExecuteScalar();
        }

        public Image GetImageById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Images WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = command.ExecuteReader();
            if(!reader.Read())
            {
                return null;
            }
            return new Image()
            {
                Id = (int)reader["id"],
                ImagePath = (string)reader["imagePath"],
                Password = (string)reader["password"],
                Views = (int)reader["Views"]
            };
        }

        public void AddView(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Images SET Views += 1 WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
