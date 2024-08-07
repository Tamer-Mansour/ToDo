using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using ToDo.Models;

namespace ToDo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DepartmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Department>> Get()
        {
            string query = @"select DepartmentId, DepartmentName from dbo.Department";
            var departments = new List<Department>();
            string sqlDataSource = _configuration.GetConnectionString("ToDo");

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            departments.Add(new Department
                            {
                                DepartmentId = (int)myReader["DepartmentId"],
                                DepartmentName = myReader["DepartmentName"].ToString()
                            });
                        }
                    }
                }
            }

            return Ok(departments);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Department updatedDepartment)
        {
            if (updatedDepartment == null || id != updatedDepartment.DepartmentId)
            {
                return BadRequest("Department ID mismatch or invalid data.");
            }

            string query = @"UPDATE dbo.Department SET DepartmentName = @DepartmentName WHERE DepartmentId = @DepartmentId";
            string sqlDataSource = _configuration.GetConnectionString("ToDo");

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                try
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@DepartmentId", updatedDepartment.DepartmentId);
                        myCommand.Parameters.AddWithValue("@DepartmentName", updatedDepartment.DepartmentName);

                        int rowsAffected = myCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return NoContent(); // Indicates success but no content to return
                        }
                        else
                        {
                            return NotFound("Department not found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., log the error)
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string query = @"DELETE FROM dbo.Department WHERE DepartmentId = @DepartmentId";
            string sqlDataSource = _configuration.GetConnectionString("ToDo");

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                try
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@DepartmentId", id);

                        int rowsAffected = myCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return NoContent(); // Indicates success but no content to return
                        }
                        else
                        {
                            return NotFound("Department not found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., log the error)
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Department newDepartment)
        {
            if (newDepartment == null)
            {
                return BadRequest("Invalid department data.");
            }

            string query = @"INSERT INTO dbo.Department (DepartmentName) VALUES (@DepartmentName)";
            string sqlDataSource = _configuration.GetConnectionString("ToDo");

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                try
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@DepartmentName", newDepartment.DepartmentName);

                        int rowsAffected = myCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return CreatedAtAction(nameof(GetById), new { id = newDepartment.DepartmentId }, newDepartment);
                        }
                        else
                        {
                            return StatusCode(500, "A problem occurred while handling your request.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., log the error)
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            string query = @"SELECT DepartmentId, DepartmentName FROM dbo.Department WHERE DepartmentId = @DepartmentId";
            string sqlDataSource = _configuration.GetConnectionString("ToDo");
            Department department = null;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@DepartmentId", id);
                    using (var myReader = myCommand.ExecuteReader())
                    {
                        if (myReader.Read())
                        {
                            department = new Department
                            {
                                DepartmentId = (int)myReader["DepartmentId"],
                                DepartmentName = myReader["DepartmentName"].ToString()
                            };
                        }
                    }
                }
            }
            if (department == null)
            {
                return NotFound("Department not found.");
            }

            return Ok(department);
        }
    }
}
