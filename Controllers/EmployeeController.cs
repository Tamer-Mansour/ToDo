using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ToDo.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace ToDo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> Get()
        {
            var employees = new List<Employee>();
            string query = @"
        SELECT EmployeeId, EmployeeName, Department,
               DateOfJoining,
               PhotoFileName
        FROM dbo.Employee";

            string sqlDataSource = _configuration.GetConnectionString("ToDo");

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                try
                {
                    await myCon.OpenAsync();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        using (SqlDataReader myReader = await myCommand.ExecuteReaderAsync())
                        {
                            while (await myReader.ReadAsync())
                            {
                                var employee = new Employee
                                {
                                    EmployeeId = Convert.ToInt32(myReader["EmployeeId"]),
                                    EmployeeName = myReader["EmployeeName"].ToString(),
                                    Department = myReader["Department"].ToString(),
                                    // Handle DateOfJoining as DateTime?
                                    DateOfJoining = myReader["DateOfJoining"] as DateTime?,
                                    PhotoFileName = myReader["PhotoFileName"].ToString()
                                };
                                employees.Add(employee);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exceptions as needed
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Ok(employees);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee emp)
        {
            if (emp == null)
            {
                return BadRequest("Employee data is null.");
            }

            string query = @"
                           INSERT INTO dbo.Employee
                           (EmployeeName, Department, DateOfJoining, PhotoFileName)
                           VALUES (@EmployeeName, @Department, @DateOfJoining, @PhotoFileName)
                           ";

            string sqlDataSource = _configuration.GetConnectionString("ToDo");

            try
            {
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    await myCon.OpenAsync();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@EmployeeName", emp.EmployeeName ?? (object)DBNull.Value);
                        myCommand.Parameters.AddWithValue("@Department", emp.Department ?? (object)DBNull.Value);
                        myCommand.Parameters.AddWithValue("@DateOfJoining", emp.DateOfJoining ?? (object)DBNull.Value);
                        myCommand.Parameters.AddWithValue("@PhotoFileName", emp.PhotoFileName ?? (object)DBNull.Value);

                        int rowsAffected = await myCommand.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return CreatedAtAction(nameof(GetById), new { id = emp.EmployeeId }, emp);
                        }
                        else
                        {
                            return StatusCode(500, "A problem occurred while handling your request.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions as needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            string query = @"SELECT EmployeeId, EmployeeName, Department, DateOfJoining, PhotoFileName FROM dbo.Employee WHERE EmployeeId = @EmployeeId";
            string sqlDataSource = _configuration.GetConnectionString("ToDo");
            Employee employee = null;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                await myCon.OpenAsync();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@EmployeeId", id);
                    using (var myReader = await myCommand.ExecuteReaderAsync())
                    {
                        if (await myReader.ReadAsync())
                        {
                            employee = new Employee
                            {
                                EmployeeId = Convert.ToInt32(myReader["EmployeeId"]),
                                EmployeeName = myReader["EmployeeName"].ToString(),
                                Department = myReader["Department"].ToString(),
                                DateOfJoining = myReader["DateOfJoining"] as DateTime?,
                                PhotoFileName = myReader["PhotoFileName"].ToString()
                            };
                        }
                    }
                }
            }

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            return Ok(employee);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Employee emp)
        {
            if (id <= 0 || emp == null)
            {
                return BadRequest("Invalid input.");
            }

            if (id != emp.EmployeeId)
            {
                return BadRequest("Employee ID mismatch.");
            }

            string query = @"
                       UPDATE dbo.Employee
                       SET EmployeeName = @EmployeeName,
                           Department = @Department,
                           DateOfJoining = @DateOfJoining,
                           PhotoFileName = @PhotoFileName
                       WHERE EmployeeId = @EmployeeId
                       ";

            string sqlDataSource = _configuration.GetConnectionString("ToDo");

            try
            {
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    await myCon.OpenAsync();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@EmployeeId", emp.EmployeeId);
                        myCommand.Parameters.AddWithValue("@EmployeeName", emp.EmployeeName ?? (object)DBNull.Value);
                        myCommand.Parameters.AddWithValue("@Department", emp.Department ?? (object)DBNull.Value);
                        myCommand.Parameters.AddWithValue("@DateOfJoining", emp.DateOfJoining ?? (object)DBNull.Value);
                        myCommand.Parameters.AddWithValue("@PhotoFileName", emp.PhotoFileName ?? (object)DBNull.Value);

                        int rowsAffected = await myCommand.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return NoContent(); // Success with no content to return
                        }
                        else
                        {
                            return NotFound("Employee not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions as needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Employee ID.");
            }

            string query = @"
                           DELETE FROM dbo.Employee
                           WHERE EmployeeId = @EmployeeId
                           ";

            string sqlDataSource = _configuration.GetConnectionString("ToDo");

            try
            {
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    await myCon.OpenAsync();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@EmployeeId", id);

                        int rowsAffected = await myCommand.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return NoContent(); // Success with no content to return
                        }
                        else
                        {
                            return NotFound("Employee not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions as needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Route("SaveFile")]
        [HttpPost]
        public async Task<IActionResult> SaveFile()
        {
            try
            {
                if (Request.Form.Files.Count == 0)
                {
                    return BadRequest("No files were uploaded.");
                }

                var postedFile = Request.Form.Files[0];
                string filename = Path.GetFileName(postedFile.FileName);
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Photos");

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, filename);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await postedFile.CopyToAsync(stream);
                }

                return Ok(new { filename = filename });
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Photo/{filename}")]
        public IActionResult GetPhoto(string filename)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filename))
                {
                    return BadRequest("Invalid file name.");
                }

                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Photos");
                if (string.IsNullOrWhiteSpace(uploadsFolder))
                {
                    throw new ArgumentNullException(nameof(uploadsFolder), "Uploads folder path cannot be null.");
                }

                var filePath = Path.Combine(uploadsFolder, filename);
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
                }

                // Log paths for debugging
                Console.WriteLine($"Uploads Folder: {uploadsFolder}");
                Console.WriteLine($"File Path: {filePath}");

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File not found.");
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var fileExtension = Path.GetExtension(filename).ToLowerInvariant();
                string mimeType = fileExtension switch
                {
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream",
                };

                return File(fileBytes, mimeType);
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
