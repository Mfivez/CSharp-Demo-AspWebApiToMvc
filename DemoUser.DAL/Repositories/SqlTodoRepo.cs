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

        private DbConnection CreateConnection() => new SqlConnection(_connectionString);

        public IEnumerable<Todo> GetAll(Guid userId)
        {
            var list = new List<Todo>();

            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"SELECT * FROM [Todo] WHERE UserId = @userId;";

            AddParameter(command, "@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapTodo(reader));
            }

            return list;
        }

        public Todo? GetById(Guid userId, Guid todoId)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                SELECT * FROM [Todo]
                WHERE Id = @todoId AND UserId = @userId;
            ";

            AddParameter(command, "@todoId", todoId);
            AddParameter(command, "@userId", userId);

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return MapTodo(reader);
        }

        public void Insert(Todo todo)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                INSERT INTO [Todo] (Id, UserId, Title, IsDone, CreatedAt)
                VALUES (@Id, @UserId, @Title, @IsDone, @CreatedAt);
            ";

            AddParameter(command, "@Id", todo.Id);
            AddParameter(command, "@UserId", todo.UserId);
            AddParameter(command, "@Title", todo.Title);
            AddParameter(command, "@IsDone", todo.IsDone);
            AddParameter(command, "@CreatedAt", todo.CreatedAt);

            command.ExecuteNonQuery();
        }

        public bool Rename(Guid userId, Guid todoId, string newTitle)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                UPDATE [Todo]
                SET Title = @newTitle
                WHERE Id = @todoId AND UserId = @userId;
            ";

            AddParameter(command, "@newTitle", newTitle);
            AddParameter(command, "@todoId", todoId);
            AddParameter(command, "@userId", userId);

            return command.ExecuteNonQuery() > 0;
        }

        public bool MarkAsDone(Guid userId, Guid todoId)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                UPDATE [Todo]
                SET IsDone = 1
                WHERE Id = @todoId AND UserId = @userId;
            ";

            AddParameter(command, "@todoId", todoId);
            AddParameter(command, "@userId", userId);

            return command.ExecuteNonQuery() > 0;
        }

        public bool Delete(Guid userId, Guid todoId)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                DELETE FROM [Todo]
                WHERE Id = @todoId AND UserId = @userId;
            ";

            AddParameter(command, "@todoId", todoId);
            AddParameter(command, "@userId", userId);

            return command.ExecuteNonQuery() > 0;
        }

        private static Todo MapTodo(IDataRecord record)
        {
            return new Todo(
                id: (Guid)record["Id"],
                userId: (Guid)record["UserId"],
                title: (string)record["Title"],
                isDone: (bool)record["IsDone"],
                createdAt: (DateTime)record["CreatedAt"]
            );
        }

        private static void AddParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}
