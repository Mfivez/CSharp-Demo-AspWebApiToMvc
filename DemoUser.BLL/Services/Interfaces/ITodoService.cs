using System;
using System.Collections.Generic;
using DemoUser.Domain.Entities;

namespace DemoUser.BLL.Services.Interfaces
{
    public interface ITodoService
    {
        IEnumerable<Todo> GetAll();
        Todo? GetById(Guid id);
        Todo Create(string title);
        void MarkAsDone(Guid id);
        void Rename(Guid id, string newTitle);
        void Delete(Guid id);
    }
}
