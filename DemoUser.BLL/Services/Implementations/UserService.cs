using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DemoUser.BLL.Services.Interfaces;
using DemoUser.Domain.Entities;
using DemoUser.Domain.Repositories;

namespace DemoUser.BLL.Services.Implementations
{
    /// <summary>
    /// Note ici (vrai projet on passe plutôt par Bcrypt que SHA256 pour crypter).
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepo _repo;

        public UserService(IUserRepo repo)
        {
            _repo = repo;
        }

        public User? Authenticate(string username, string password)
        {
            var user = _repo.GetByUsername(username);
            if (user is null)
                return null;

            var hash = HashPassword(password);
            return hash == user.PasswordHash ? user : null;
        }

        public void Register(string username, string password)
        {
            var hash = HashPassword(password);

            var user = new User(
                Guid.NewGuid(),
                username,
                hash,
                DateTime.UtcNow
            );

            if (_repo.GetByUsername(username) != null)
                throw new InvalidOperationException("Username already exists.");


            _repo.Create(user);
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
