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
public class ProblemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProblemsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProblems()
    {
        var problems = await _context.Problems
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Description,
                p.Difficulty,
                p.TeacherId
            })
            .ToListAsync();

        return Ok(problems);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProblem(int id)
    {
        var problem = await _context.Problems
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Description,
                p.Difficulty,
                p.TeacherId
            })
            .FirstOrDefaultAsync();

        if (problem == null)
            return NotFound();

        return Ok(problem);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateProblem(Problem problem)
    {
        problem.Id = 0;

        problem.TeacherId = GetCurrentTeacherId();

        _context.Problems.Add(problem);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProblem),
            new { id = problem.Id },
            problem
        );
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateProblem(
        int id,
        Problem updatedProblem)
    {
        var problem = await _context.Problems.FindAsync(id);

        if (problem == null)
            return NotFound();

        problem.Title = updatedProblem.Title;
        problem.Description = updatedProblem.Description;
        problem.Difficulty = updatedProblem.Difficulty;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteProblem(int id)
    {
        var problem = await _context.Problems.FindAsync(id);

        if (problem == null)
            return NotFound();

        _context.Problems.Remove(problem);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int GetCurrentTeacherId()
    {
        return int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );
    }
}