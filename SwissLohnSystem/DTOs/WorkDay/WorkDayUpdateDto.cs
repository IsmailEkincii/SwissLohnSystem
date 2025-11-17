using System;
using System.ComponentModel.DataAnnotations;

namespace SwissLohnSystem.API.DTOs.WorkDay
{
    public class WorkDayUpdateDto : WorkDayCreateDto
    {
        [Required]
        public int Id { get; set; }
    }
}
