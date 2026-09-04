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
public class GroupsController : ControllerBase
{
    private readonly AppDbContext _context;

    public GroupsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetGroups()
    {
        var groups = await _context.Groups
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.TeacherId
            })
            .ToListAsync();

        return Ok(groups);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroup(int id)
    {
        var group = await _context.Groups
            .Where(g => g.Id == id)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.TeacherId
            })
            .FirstOrDefaultAsync();

        if (group == null)
            return NotFound();

        return Ok(group);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateGroup(Group group)
    {
        group.Id = 0;

        group.TeacherId = GetCurrentTeacherId();

        _context.Groups.Add(group);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetGroup),
            new { id = group.Id },
            group
        );
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateGroup(
        int id,
        Group updatedGroup)
    {
        var group = await _context.Groups.FindAsync(id);

        if (group == null)
            return NotFound();

        group.Name = updatedGroup.Name;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var group = await _context.Groups.FindAsync(id);

        if (group == null)
            return NotFound();

        _context.Groups.Remove(group);

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