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
public class TagsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TagsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTags()
    {
        var tags = await _context.Tags
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.TeacherId
            })
            .ToListAsync();

        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTag(int id)
    {
        var tag = await _context.Tags
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.TeacherId
            })
            .FirstOrDefaultAsync();

        if (tag == null)
            return NotFound();

        return Ok(tag);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateTag(Tag tag)
    {
        tag.Id = 0;

        tag.TeacherId = GetCurrentTeacherId();

        _context.Tags.Add(tag);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTag),
            new { id = tag.Id },
            tag
        );
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateTag(
        int id,
        Tag updatedTag)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
            return NotFound();

        tag.Name = updatedTag.Name;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
            return NotFound();

        _context.Tags.Remove(tag);

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