using System.ComponentModel.DataAnnotations;

namespace DemoUser.ASP.Models.Todo
{
    public class TodoEditViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        [MinLength(3)]
        public string Title { get; set; } = string.Empty;

        public bool IsDone { get; set; }
    }
}
