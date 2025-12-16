namespace DemoUser.ASP.Models.Todo
{
    public class TodoListItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
