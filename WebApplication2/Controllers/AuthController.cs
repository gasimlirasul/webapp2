using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.DTOs;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordHasher<object> _passwordHasher;

    public AuthController(
        AppDbContext context,
        JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = new PasswordHasher<object>();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (dto.Role != "Teacher" && dto.Role != "Student")
        {
            return BadRequest("Role must be Teacher or Student.");
        }

        if (dto.Role == "Teacher")
        {
            if (await _context.Teachers.AnyAsync(t => t.Email == dto.Email))
            {
                return BadRequest("Email already exists.");
            }

            var teacher = new Teacher
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            };

            teacher.PasswordHash =
                _passwordHasher.HashPassword(
                    teacher,
                    dto.Password
                );

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Teacher registered successfully.",
                teacherId = teacher.Id
            });
        }

        if (await _context.Students.AnyAsync(s => s.Email == dto.Email))
        {
            return BadRequest("Email already exists.");
        }

        var student = new Student
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email
        };

        student.PasswordHash =
            _passwordHasher.HashPassword(
                student,
                dto.Password
            );

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Student registered successfully.",
            studentId = student.Id
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Email == dto.Email);

        if (teacher != null)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                teacher,
                teacher.PasswordHash,
                dto.Password
            );

            if (result == PasswordVerificationResult.Success)
            {
                var token = _jwtService.CreateToken(
                    teacher.Id,
                    teacher.Email,
                    "Teacher"
                );

                return Ok(new LoginResponseDto
                {
                    Token = token,
                    UserId = teacher.Id,
                    Role = "Teacher"
                });
            }
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Email == dto.Email);

        if (student != null)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                student,
                student.PasswordHash,
                dto.Password
            );

            if (result == PasswordVerificationResult.Success)
            {
                var token = _jwtService.CreateToken(
                    student.Id,
                    student.Email,
                    "Student"
                );

                return Ok(new LoginResponseDto
                {
                    Token = token,
                    UserId = student.Id,
                    Role = "Student"
                });
            }
        }

        return Unauthorized("Invalid email or password.");
    }
}