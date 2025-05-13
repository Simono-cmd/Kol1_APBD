using Kol1_APBD.Models.DTOs;
using Kol1_APBD.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kol1_APBD.Controllers;

[Route("api/")]
[ApiController]

public class MyController : Controller
{
    private readonly IDBservice _service;

    public MyController(IDBservice service)
    {
        _service = service;
    }


    [HttpGet("visits/{id}")]
    public async Task<IActionResult> GetAppointmentById(int id)
    {
        var visit = await _service.GetVisitById(id);
        if (visit == null)
        {
            return NotFound(new { message = $"Visit with ID {id} not found." });
        }

        return Ok(visit);
    }

    [HttpPost("visits")]
    public async Task<IActionResult> AddAppointment([FromBody] VisitInsertDTO visit)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");
        
        var (success, message) = await _service.AddVisit(visit);

        if (!success)
        {
            return BadRequest(message);
        }
        
        return CreatedAtAction(
            nameof(GetAppointmentById),
            new { id = visit.visitId },
            new
            {
                visitId = visit.visitId,
                status = message
            }
        );
    }

    
    
}