using System;
using System.Collections.Generic;
using DemoUser.Domain.Entities;

namespace DemoUser.Domain.Repositories
{
    public interface IUserRepo
    {
        User? GetByUsername(string username);
        void Create(User user);
    }
}
