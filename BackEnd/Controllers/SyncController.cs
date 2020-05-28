using System;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using Dapper;

namespace BackEnd.Controllers
{
    [Route("api/trees/sync")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SyncController : ControllerBase
    {
        private static readonly string procedure = "OneMoreTreeContext.dbo.sync_trees";

        private readonly string _databaseConnectionString;

        public SyncController(IConfiguration configuration)
        {
            _databaseConnectionString = configuration.GetConnectionString("DATABASE_CONNECTION_STRING");
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetDelta(int fromVersion)
        {
            JsonDocument result = null;

            using (SqlConnection connection = new SqlConnection(_databaseConnectionString))
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("json", JsonSerializer.Serialize(new { fromVersion }));

                var query = await connection.ExecuteScalarAsync<string>(
                    sql: procedure,
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (query != null)
                    result = JsonDocument.Parse(query);
            }

            if (result == null)
                result = JsonDocument.Parse("[]");

            return Ok(result.RootElement);
        }
    }
}