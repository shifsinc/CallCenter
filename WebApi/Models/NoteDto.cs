using System;

namespace WebApi.Models
{
    public class NoteDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public UserDto User { get; set; }
    }
}