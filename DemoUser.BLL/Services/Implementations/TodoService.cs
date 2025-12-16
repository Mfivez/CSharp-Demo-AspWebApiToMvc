using System;
using System.Collections.Generic;
using DemoUser.BLL.Services.Interfaces;
using DemoUser.Domain.Entities;
using DemoUser.Domain.Repositories;

namespace DemoUser.BLL.Services.Implementations
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepo _todoRepo;

        public TodoService(ITodoRepo todoRepo)
        {
            _todoRepo = todoRepo;
        }

        public IEnumerable<Todo> GetAll(Guid userId)
            => _todoRepo.GetAll(userId);

        public Todo? GetById(Guid userId, Guid todoId)
            => _todoRepo.GetById(userId, todoId);

        public Todo Create(Guid userId, string title)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            var todo = new Todo(
                id: Guid.NewGuid(),
                userId: userId,
                title: title.Trim(),
                isDone: false,
                createdAt: DateTime.UtcNow
            );

            _todoRepo.Insert(todo);

            return todo;
        }

        public bool Rename(Guid userId, Guid todoId, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title is required.", nameof(newTitle));

            return _todoRepo.Rename(userId, todoId, newTitle.Trim());
        }

        public bool MarkAsDone(Guid userId, Guid todoId)
            => _todoRepo.MarkAsDone(userId, todoId);

        public bool Delete(Guid userId, Guid todoId)
            => _todoRepo.Delete(userId, todoId);
    }
}
