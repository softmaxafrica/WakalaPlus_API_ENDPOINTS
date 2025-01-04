using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WakalaPlus.Models;
using WakalaPlus.Shared;

namespace YourApiNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceIdController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeviceIdController(AppDbContext context)
        {
            _context = context;
        }

        #region Get All Devices

        [HttpGet("GetAllDeviceDetails")]
        public IActionResult GetAllDeviceDetails()
        {
            var executionResult = new ExecutionResult();
            try
            {
                var deviceDetails = _context.DeviceDetails.ToList();
                executionResult.SetDataList(deviceDetails);
                executionResult.SetTotalCount(deviceDetails.Count);
                executionResult.SetGeneralInfo(nameof(DeviceIdController), nameof(GetAllDeviceDetails), "Data retrieved successfully");

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(DeviceIdController), nameof(GetAllDeviceDetails), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
            }
        }

        #endregion

        #region Get Device by DeviceId

        [HttpGet("GetDeviceById/{deviceId}")]
        public IActionResult GetDeviceById(string deviceId)
        {
            var executionResult = new ExecutionResult();
            try
            {
                var device = _context.DeviceDetails.FirstOrDefault(d => d.deviceId == deviceId);
                if (device == null)
                {
                    executionResult.SetGeneralError(nameof(DeviceIdController), nameof(GetDeviceById), "Device not found");
                    return NotFound(executionResult.GetServerResponse());
                }

                executionResult.SetData(device);
                executionResult.SetGeneralInfo(nameof(DeviceIdController), nameof(GetDeviceById), "Device retrieved successfully");

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(DeviceIdController), nameof(GetDeviceById), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
            }
        }

        #endregion

        #region Add Device

        [HttpPost("AddDevice")]
        public async Task<IActionResult> AddDevice([FromBody] DeviceDetails newDevice)
        {
            var executionResult = new ExecutionResult();
            try
            {
                if (newDevice == null || string.IsNullOrEmpty(newDevice.deviceId))
                {
                    executionResult.SetGeneralError(nameof(DeviceIdController), nameof(AddDevice), "Invalid device data");
                    return BadRequest(executionResult.GetServerResponse());
                }

                _context.DeviceDetails.Add(newDevice);
                await _context.SaveChangesAsync();

                executionResult.SetData(newDevice);
                executionResult.SetGeneralInfo(nameof(DeviceIdController), nameof(AddDevice), "Device added successfully");

                return CreatedAtAction(nameof(GetDeviceById), new { deviceId = newDevice.deviceId }, executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(DeviceIdController), nameof(AddDevice), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
            }
        }

        #endregion

        #region Update Device

        [HttpPut("UpdateDevice/{deviceId}")]
        public async Task<IActionResult> UpdateDevice(string deviceId, [FromBody] DeviceDetails updatedDevice)
        {
            var executionResult = new ExecutionResult();
            try
            {
                if (updatedDevice == null || deviceId != updatedDevice.deviceId)
                {
                    executionResult.SetGeneralError(nameof(DeviceIdController), nameof(UpdateDevice), "Invalid device data or DeviceId mismatch");
                    return BadRequest(executionResult.GetServerResponse());
                }

                var existingDevice = await _context.DeviceDetails.FirstOrDefaultAsync(d => d.deviceId == deviceId);
                if (existingDevice == null)
                {
                    executionResult.SetGeneralError(nameof(DeviceIdController), nameof(UpdateDevice), "Device not found");
                    return NotFound(executionResult.GetServerResponse());
                }

                // Update fields
                existingDevice.Identity = updatedDevice.Identity;
                existingDevice.LastConnectionDate = updatedDevice.LastConnectionDate;
                existingDevice.createdDate = updatedDevice.createdDate;
                existingDevice.connectionId = updatedDevice.connectionId;
                existingDevice.LastAction = updatedDevice.LastAction;

                _context.DeviceDetails.Update(existingDevice);
                await _context.SaveChangesAsync();

                executionResult.SetData(existingDevice);
                executionResult.SetGeneralInfo(nameof(DeviceIdController), nameof(UpdateDevice), "Device updated successfully");

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(DeviceIdController), nameof(UpdateDevice), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
            }
        }

        #endregion
        #region Delete All Device Connections

        [HttpDelete("DeleteAllDeviceConnections")]
        public async Task<IActionResult> DeleteAllDeviceConnections()
        {
            var executionResult = new ExecutionResult();
            try
            {
                // Get all the device connections from the database
                var allDevices = _context.DeviceDetails.ToList();

                if (allDevices.Any())
                {
                    // Remove all the devices
                    _context.DeviceDetails.RemoveRange(allDevices);
                    await _context.SaveChangesAsync();

                    executionResult.SetGeneralInfo(nameof(DeviceIdController), nameof(DeleteAllDeviceConnections), "All device connections deleted successfully");
                }
                else
                {
                    executionResult.SetGeneralInfo(nameof(DeviceIdController), nameof(DeleteAllDeviceConnections), "No device connections found to delete");
                }

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(DeviceIdController), nameof(DeleteAllDeviceConnections), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
            }
        }

        #endregion


        #region Delete Device By Id

        [HttpDelete("DeleteDevice/{deviceId}")]
        public async Task<IActionResult> DeleteDevice(string deviceId)
        {
            var executionResult = new ExecutionResult();
            try
            {
                var existingDevice = await _context.DeviceDetails.FirstOrDefaultAsync(d => d.deviceId == deviceId);
                if (existingDevice == null)
                {
                    executionResult.SetGeneralError(nameof(DeviceIdController), nameof(DeleteDevice), "Device not found");
                    return NotFound(executionResult.GetServerResponse());
                }

                _context.DeviceDetails.Remove(existingDevice);
                await _context.SaveChangesAsync();

                executionResult.SetGeneralInfo(nameof(DeviceIdController), nameof(DeleteDevice), "Device deleted successfully");

                return Ok(executionResult.GetServerResponse());
            }
            catch (Exception ex)
            {
                executionResult.SetInternalServerError(nameof(DeviceIdController), nameof(DeleteDevice), ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, executionResult.GetServerResponse());
            }
        }

        #endregion
    }
}
