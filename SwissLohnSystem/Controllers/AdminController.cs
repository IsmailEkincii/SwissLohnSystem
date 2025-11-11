using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Admin;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Responses;



namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context) => _context = context;

        // GET: api/Admin
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AdminDto>>>> GetAdmins()
        {
            var list = await _context.Admins
                .AsNoTracking()
                .OrderBy(a => a.Username)
                .Select(a => a.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<AdminDto>>.Ok(list, "Admins erfolgreich geladen.");
        }

        // GET: api/Admin/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AdminDto>>> GetAdmin(int id)
        {
            var admin = await _context.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (admin is null)
                return NotFound(ApiResponse<AdminDto>.Fail("Admin wurde nicht gefunden."));

            return ApiResponse<AdminDto>.Ok(admin.ToDto(), "Admin erfolgreich gefunden.");
        }

        // POST: api/Admin
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AdminDto>>> PostAdmin([FromBody] AdminCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AdminDto>.Fail("Ungültige Eingabedaten."));

            // (Opsiyonel) aynı kullanıcı adı kontrolü
            var exists = await _context.Admins.AnyAsync(a => a.Username == dto.Username);
            if (exists)
                return BadRequest(ApiResponse<AdminDto>.Fail("Benutzername ist bereits vergeben."));

            var entity = dto.ToEntity();
            _context.Admins.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdmin), new { id = entity.Id },
                ApiResponse<AdminDto>.Ok(entity.ToDto(), "Admin wurde erfolgreich erstellt."));
        }

        // PUT: api/Admin/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> PutAdmin(int id, [FromBody] AdminUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<string>.Fail("ID stimmt nicht überein."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Eingabe."));

            var entity = await _context.Admins.FindAsync(id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Admin wurde nicht gefunden."));

            // (Opsiyonel) username uniqueness
            var exists = await _context.Admins
                .AnyAsync(a => a.Id != id && a.Username == dto.Username);
            if (exists)
                return BadRequest(ApiResponse<string>.Fail("Benutzername ist bereits vergeben."));

            entity.Apply(dto);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Admin wurde erfolgreich aktualisiert.");
        }

        // DELETE: api/Admin/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteAdmin(int id)
        {
            var entity = await _context.Admins.FindAsync(id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Admin wurde nicht gefunden."));

            _context.Admins.Remove(entity);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Admin wurde erfolgreich gelöscht.");
        }
    }
}
