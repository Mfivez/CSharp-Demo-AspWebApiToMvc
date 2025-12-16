using System;
using System.Collections.Generic;
using DemoUser.Domain.Entities;

namespace DemoUser.Domain.Repositories
{
    public interface ITodoRepo
    {
        IEnumerable<Todo> GetAll();
        Todo? GetById(Guid id);
        Todo Insert(Todo todo);
        void Update(Todo todo);
        void Delete(Guid id);
    }
}
