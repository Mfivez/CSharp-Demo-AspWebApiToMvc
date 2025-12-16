using System;
using System.Collections.Generic;
using DemoUser.Domain.Entities;

namespace DemoUser.Domain.Repositories
{
    public interface ITodoRepo
    {
        IEnumerable<Todo> GetAll(Guid userId);
        Todo? GetById(Guid userId, Guid todoId);
        void Insert(Todo todo);
        bool Rename(Guid userId, Guid todoId, string newTitle);
        bool MarkAsDone(Guid userId, Guid todoId);
        bool Delete(Guid userId, Guid todoId);
    }
}
