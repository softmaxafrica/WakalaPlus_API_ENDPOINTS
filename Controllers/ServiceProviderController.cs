using Microsoft.AspNetCore.Mvc;
 using WakalaPlus.Models;
using WakalaPlus.Shared;
 using Microsoft.EntityFrameworkCore;
using System.Net;



[ApiController]
[Route("api/[controller]")]
public class ServiceProviderController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServiceProviderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("GetAllServiceProviders")]
    public IActionResult GetAllServiceProviders()
    {
        var executionResult = new ExecutionResult();

        try
        {
            var serviceProviders = _context.ServiceProviders.ToList();
            executionResult.SetDataList(serviceProviders);
            executionResult.SetTotalCount(serviceProviders.Count);
            executionResult.SetGeneralInfo(nameof(ServiceProviderController), nameof(GetAllServiceProviders), "Data retrieved successfully");

            return Ok(executionResult.GetServerResponse());
        }
        catch (Exception ex)
        {
            executionResult.SetInternalServerError(nameof(ServiceProviderController), nameof(GetAllServiceProviders), ex);
            return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
        }
    }

    [HttpPost("AddServiceProvider")]
    public IActionResult AddServiceProvider([FromBody] ServiceProviders serviceProvider)
    {
        var executionResult = new ExecutionResult();

        try
        {
            _context.ServiceProviders.Add(serviceProvider);
            _context.SaveChanges();

            executionResult.SetData(serviceProvider);
            executionResult.SetGeneralInfo(nameof(ServiceProviderController), nameof(AddServiceProvider), "Service provider added successfully");

            return Ok(executionResult.GetServerResponse());
        }
        catch (Exception ex)
        {
            executionResult.SetInternalServerError(nameof(ServiceProviderController), nameof(AddServiceProvider), ex);
            return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
        }
    }
}
