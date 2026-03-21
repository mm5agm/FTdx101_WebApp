using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FTdx101_WebApp.Services;
using FTdx101_WebApp.Models.Calibration;

[ApiController]
[Route("api/calibration")]
public class CalibrationController : ControllerBase
{
    private readonly ICalibrationService _service;

    public CalibrationController(ICalibrationService service)
    {
        _service = service;
    }

    [HttpGet("all")]
    public IActionResult GetAll()
    {
        var all = _service.GetAllCalibrationTables();
        return Ok(all);
    }
}
