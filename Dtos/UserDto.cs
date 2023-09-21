using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
namespace signalR.Dtos
{
    public class UserDto
    {
        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Name must be at least (2), and max (1) characters long" )]
        public string Name { get; set; }
    }
}
