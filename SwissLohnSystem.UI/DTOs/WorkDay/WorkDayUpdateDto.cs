using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.UI.DTOs.WorkDay
{
    public class WorkDayUpdateDto : WorkDayCreateDto
    {
        [Required]
        public int Id { get; set; }
    }
}
