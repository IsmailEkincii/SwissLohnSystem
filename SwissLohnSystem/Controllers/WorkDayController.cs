using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Controllers;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Models;

[Route("api/[controller]")]
[ApiController]
public class WorkDayController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WorkDayController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/WorkDay/Employee/5
    [HttpGet("Employee/{employeeId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkDay>>>> GetEmployeeWorkDays(int employeeId)
    {
        var workDays = await _context.WorkDays
            .Where(w => w.EmployeeId == employeeId)
            .OrderBy(w => w.Date)
            .ToListAsync();

        return Ok(new ApiResponse<IEnumerable<WorkDay>>
        {
            Success = true,
            Message = "Arbeitszeiten erfolgreich geladen.",
            Data = workDays
        });
    }

    // POST: api/WorkDay
    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkDay>>> PostWorkDay(WorkDay workDay)
    {
        _context.WorkDays.Add(workDay);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<WorkDay>
        {
            Success = true,
            Message = "Arbeitszeit erfolgreich hinzugefügt.",
            Data = workDay
        });
    }

    // PUT: api/WorkDay/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> PutWorkDay(int id, WorkDay workDay)
    {
        if (id != workDay.Id)
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Ungültige Arbeitszeit-ID."
            });

        _context.Entry(workDay).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.WorkDays.Any(w => w.Id == id))
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Arbeitszeit nicht gefunden."
                });
            else
                throw;
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Arbeitszeit erfolgreich aktualisiert."
        });
    }

    // DELETE: api/WorkDay/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWorkDay(int id)
    {
        var workDay = await _context.WorkDays.FindAsync(id);
        if (workDay == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Arbeitszeit nicht gefunden."
            });

        _context.WorkDays.Remove(workDay);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Arbeitszeit erfolgreich gelöscht."
        });
    }
}
