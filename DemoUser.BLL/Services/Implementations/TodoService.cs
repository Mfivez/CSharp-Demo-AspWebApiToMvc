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

        public IEnumerable<Todo> GetAll()
            => _todoRepo.GetAll();

        public Todo? GetById(Guid id)
            => _todoRepo.GetById(id);

        public Todo Create(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            var todo = new Todo(title);
            return _todoRepo.Insert(todo);
        }

        public void MarkAsDone(Guid id)
        {
            var todo = _todoRepo.GetById(id);
            if (todo is null) return;

            todo.MarkAsDone();
            _todoRepo.Update(todo);
        }

        public void Rename(Guid id, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title is required.", nameof(newTitle));

            var todo = _todoRepo.GetById(id);
            if (todo is null) return;

            todo.Rename(newTitle);
            _todoRepo.Update(todo);
        }

        public void Delete(Guid id)
            => _todoRepo.Delete(id);
    }
}
