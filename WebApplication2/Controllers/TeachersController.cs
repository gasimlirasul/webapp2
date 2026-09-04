using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeachersController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeachersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTeachers()
    {
        var teachers = await _context.Teachers
            .Select(t => new
            {
                t.Id,
                t.FirstName,
                t.LastName,
                t.Email
            })
            .ToListAsync();

        return Ok(teachers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeacher(int id)
    {
        var teacher = await _context.Teachers
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.FirstName,
                t.LastName,
                t.Email
            })
            .FirstOrDefaultAsync();

        if (teacher == null)
            return NotFound();

        return Ok(teacher);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateTeacher(Teacher teacher)
    {
        if (await _context.Teachers
            .AnyAsync(t => t.Email == teacher.Email))
        {
            return BadRequest("Email already exists.");
        }

        teacher.Id = 0;

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTeacher),
            new { id = teacher.Id },
            new
            {
                teacher.Id,
                teacher.FirstName,
                teacher.LastName,
                teacher.Email
            }
        );
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateTeacher(
        int id,
        Teacher updatedTeacher)
    {
        var currentTeacherId = GetCurrentUserId();

        if (currentTeacherId == null)
            return Unauthorized();

        if (currentTeacherId.Value != id)
            return Forbid();

        var teacher = await _context.Teachers
            .FindAsync(id);

        if (teacher == null)
            return NotFound();

        teacher.FirstName = updatedTeacher.FirstName;
        teacher.LastName = updatedTeacher.LastName;
        teacher.Email = updatedTeacher.Email;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteTeacher(int id)
    {
        var currentTeacherId = GetCurrentUserId();

        if (currentTeacherId == null)
            return Unauthorized();

        if (currentTeacherId.Value != id)
            return Forbid();

        var teacher = await _context.Teachers
            .FindAsync(id);

        if (teacher == null)
            return NotFound();

        _context.Teachers.Remove(teacher);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        var id = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (int.TryParse(id, out int userId))
            return userId;

        return null;
    }
}