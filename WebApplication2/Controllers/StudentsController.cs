using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _context.Students
            .Select(s => new
            {
                s.Id,
                s.FirstName,
                s.LastName,
                s.Email
            })
            .ToListAsync();

        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudent(int id)
    {
        var student = await _context.Students
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.FirstName,
                s.LastName,
                s.Email
            })
            .FirstOrDefaultAsync();

        if (student == null)
            return NotFound();

        return Ok(student);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateStudent(Student student)
    {
        if (await _context.Students
            .AnyAsync(s => s.Email == student.Email))
        {
            return BadRequest("Email already exists.");
        }

        student.Id = 0;

        _context.Students.Add(student);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetStudent),
            new { id = student.Id },
            new
            {
                student.Id,
                student.FirstName,
                student.LastName,
                student.Email
            }
        );
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateStudent(
        int id,
        Student updatedStudent)
    {
        var student = await _context.Students.FindAsync(id);

        if (student == null)
            return NotFound();

        student.FirstName = updatedStudent.FirstName;
        student.LastName = updatedStudent.LastName;
        student.Email = updatedStudent.Email;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student == null)
            return NotFound();

        _context.Students.Remove(student);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}