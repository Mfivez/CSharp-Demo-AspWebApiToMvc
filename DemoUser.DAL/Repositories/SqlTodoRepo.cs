using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DemoUser.Domain.Entities;
using DemoUser.Domain.Repositories;
using Microsoft.Data.SqlClient;

namespace DemoUser.DAL.Repositories
{
    public class SqlTodoRepo : ITodoRepo
    {
        private readonly string _connectionString;

        public SqlTodoRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        private DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public IEnumerable<Todo> GetAll()
        {
            var list = new List<Todo>();

            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                SELECT Id, Title, IsDone, CreatedAt
                FROM [Todo]
                ORDER BY CreatedAt DESC;
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapTodo(reader));
            }

            return list;
        }

        public Todo? GetById(Guid id)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                SELECT Id, Title, IsDone, CreatedAt
                FROM [Todo]
                WHERE Id = @Id;
            ";

            AddParameter(command, "@Id", id);

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return MapTodo(reader);
        }

        public Todo Insert(Todo todo)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                INSERT INTO [Todo] (Id, Title, IsDone, CreatedAt)
                VALUES (@Id, @Title, @IsDone, @CreatedAt);

                SELECT Id, Title, IsDone, CreatedAt
                FROM [Todo]
                WHERE Id = @Id;
            ";

            AddParameter(command, "@Id", todo.Id);
            AddParameter(command, "@Title", todo.Title);
            AddParameter(command, "@IsDone", todo.IsDone);
            AddParameter(command, "@CreatedAt", todo.CreatedAt);

            using var reader = command.ExecuteReader();
            if (!reader.Read()) throw new InvalidOperationException("Failed to insert todo.");

            return MapTodo(reader);
        }

        public void Update(Todo todo)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                UPDATE [Todo]
                SET Title = @Title,
                    IsDone = @IsDone
                WHERE Id = @Id;
            ";

            AddParameter(command, "@Id", todo.Id);
            AddParameter(command, "@Title", todo.Title);
            AddParameter(command, "@IsDone", todo.IsDone);

            command.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                DELETE FROM [Todo]
                WHERE Id = @Id;
            ";

            AddParameter(command, "@Id", id);

            command.ExecuteNonQuery();
        }

        private static Todo MapTodo(IDataRecord record)
        {
            return new Todo(
                id: (Guid)record["Id"],
                title: (string)record["Title"],
                isDone: (bool)record["IsDone"],
                createdAt: (DateTime)record["CreatedAt"]
            );
        }

        private static void AddParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}
