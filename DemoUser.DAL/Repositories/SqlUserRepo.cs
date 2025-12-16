using System;
using System.Collections.Generic;
using System.Text;
using DemoUser.Domain.Entities;
using DemoUser.Domain.Repositories;
using Microsoft.Data.SqlClient;

namespace DemoUser.DAL.Repositories
{
    public class SqlUserRepo : IUserRepo
    {
        private readonly string _connectionString;

        public SqlUserRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public User? GetByUsername(string username)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(
                "SELECT * FROM [User] WHERE Username = @username",
                conn);

            cmd.Parameters.AddWithValue("@username", username);

            conn.Open();
            using SqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new User(
                reader.GetGuid(reader.GetOrdinal("Id")),
                reader.GetString(reader.GetOrdinal("Username")),
                reader.GetString(reader.GetOrdinal("PasswordHash")),
                reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            );
        }

        public void Create(User user)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(
                @"INSERT INTO [User] (Id, Username, PasswordHash, CreatedAt)
              VALUES (@id, @username, @passwordHash, @createdAt)",
                conn);

            cmd.Parameters.AddWithValue("@id", user.Id);
            cmd.Parameters.AddWithValue("@username", user.Username);
            cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@createdAt", user.CreatedAt);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
