using System;

namespace DemoUser.Domain.Entities
{
    public class Todo
    {
        public Guid Id { get; }
        public string Title { get; private set; }
        public bool IsDone { get; private set; }
        public DateTime CreatedAt { get; }

        // Ctor pour hydratation depuis la DB
        public Todo(Guid id, string title, bool isDone, DateTime createdAt)
        {
            Id = id;
            Title = title;
            IsDone = isDone;
            CreatedAt = createdAt;
        }

        // Ctor pour créer un nouveau Todo côté BLL
        public Todo(string title)
        {
            Id = Guid.NewGuid();
            Title = title;
            IsDone = false;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsDone()
        {
            IsDone = true;
        }

        public void Rename(string newTitle)
        {
            Title = newTitle;
        }
    }
}
