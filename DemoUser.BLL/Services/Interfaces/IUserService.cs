using System;
using System.Collections.Generic;
using System.Text;
using DemoUser.Domain.Entities;

namespace DemoUser.BLL.Services.Interfaces
{
    public interface IUserService
    {
        User? Authenticate(string username, string password);
        void Register(string username, string password);
    }
}
