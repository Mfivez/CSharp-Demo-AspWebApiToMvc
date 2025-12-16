using System;
using System.Collections.Generic;
using DemoUser.Domain.Entities;

namespace DemoUser.BLL.Services.Interfaces
{
    public interface ITodoService
    {
        IEnumerable<Todo> GetAll(Guid userId);
        Todo? GetById(Guid userId, Guid todoId);
        Todo Create(Guid userId, string title);
        bool Rename(Guid userId, Guid todoId, string newTitle);
        bool MarkAsDone(Guid userId, Guid todoId);
        bool Delete(Guid userId, Guid todoId);
    }
}
