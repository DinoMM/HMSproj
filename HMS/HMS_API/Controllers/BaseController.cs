using DBLayer.Models;
using HMSModels.Models;
using HMSModels.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;

namespace HMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[MyAuthorize]
    public abstract class BaseController<T> : ControllerBase where T : class
    {
        protected readonly DBLayer.DBContext _db;
        public BaseController(DBLayer.DBContext context)
        {
            _db = context;
        }

        #region GetAll
        [HttpGet]
        public virtual async Task<IActionResult> GetAllAsync()
        {
            var items = await _db.Set<T>().ToListAsync();
            return Ok(items);
        }

        // GET data with all navigation properties included
        [HttpGet("includeall")]
        public virtual async Task<IActionResult> GetAllIncludeAsync()
        {
            var items = await _db.Set<T>()
                .IncludeAll(_db)
                .ToListAsync();
            return Ok(items);
        }
        #endregion

        #region GetByIdOrDefault
        [HttpPost("GetByIdOrDefault")]
        public virtual async Task<IActionResult> GetByIdOrDefaultAsync([FromBody] JsonElement id)
        {
            T? item = null;
            if (id.ValueKind == JsonValueKind.String)
            {
                var stringId = id.GetString();
                item = await _db.Set<T>().FindAsync(stringId);
            }
            else if (id.ValueKind == JsonValueKind.Number && id.TryGetInt64(out long longId))
            {
                item = await _db.Set<T>().FindAsync(longId);
            }
            else
            {
                return BadRequest("Invalid ID type");
            }
            return Ok(item);
        }

        [HttpPost("includeal/GetByIdOrDefault")]
        public virtual async Task<IActionResult> GetByIdOrDefaultIncludeAsync([FromBody] JsonElement id)
        {
            T? item = null;
            if (id.ValueKind == JsonValueKind.String)
            {
                var stringId = id.GetString();
                item = await _db.Set<T>().FindAsync(stringId);
            }
            else if (id.ValueKind == JsonValueKind.Number && id.TryGetInt64(out long longId))
            {
                item = await _db.Set<T>().FindAsync(longId);
            }
            else
            {
                return BadRequest("Invalid ID type");
            }
            if (item != null)
            {
                item.IncludeForThis(_db);
            }
            return Ok(item);
        }

        [HttpPost("filter/GetByIdOrDefault")]
        public virtual async Task<IActionResult> GetByIdOrDefaultAsync([FromBody] List<FilterCondition> filters)
        {
            try
            {
                var expresion = FilterHtml<T>.GetExpression(filters);
                IQueryable<T> query = _db.Set<T>().Where(expresion);

                var result = await query.FirstOrDefaultAsync();
                return result != null ? Ok(result) : NotFound(null);
            }
            catch (Exception e)
            {
                return NotFound(null);
            }
        }

        [HttpPost("includeall/filter/GetByIdOrDefault")]
        public virtual async Task<IActionResult> GetByIdOrDefaultIncludeAsync([FromBody] List<FilterCondition> filters)
        {
            T? item = null;
            try
            {
                var expresion = FilterHtml<T>.GetExpression(filters);
                IQueryable<T> query = _db.Set<T>().IncludeAll(_db).Where(expresion);

                item = await query.FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                return NotFound(null);
            }
            if (item != null)
            {
                item.IncludeForThis(_db);
            }
            return item != null ? Ok(item) : NotFound(null);
        }

        #endregion

        #region Create
        [HttpPost("Create")]
        public virtual async Task<IActionResult> CreateAsync([FromBody] T entity)
        {
            try
            {
                _db.Set<T>().Add(entity);
                if (await _db.SaveChangesAsync() > 0)
                {
                    return Ok(entity);
                }
                return Problem(detail: $"Not created/added in DB: {typeof(T)}",  statusCode: 488);
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode: 489);
            }
        }
        #endregion

        #region Update
        [HttpPost("Update")]
        public virtual async Task<IActionResult> UpdateAsync([FromBody] HMSModels.UpdateRequest<T> update)
        {
            var val = FilterCondition.GetValueFromJsonElement((JsonElement?)update.Id, null);
            var existingEntity = await _db.Set<T>().FindAsync(val);
            if (existingEntity == null)
            {
                return NotFound();
            }
            try
            {
                _db.Entry(existingEntity).CurrentValues.SetValues(update.Entity);
                if (await _db.SaveChangesAsync() > 0)
                {
                    return Ok();
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message);
            }
        }
        #endregion

        #region Delete
        [HttpDelete]
        public virtual async Task<IActionResult> DeleteAsync([FromBody] object id)
        {
            var val = FilterCondition.GetValueFromJsonElement((JsonElement?)id, null);
            var entity = await _db.Set<T>().FindAsync(val);
            if (entity == null)
            {
                return NotFound();
            }

            try
            {
                _db.Set<T>().Remove(entity);
                await _db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message);
            }
        }
        #endregion

        #region Misc
        [HttpGet("GetNextID")]
        public virtual IActionResult GetNextID()
        {
            return NoContent();
        }
        #endregion
    }
}
