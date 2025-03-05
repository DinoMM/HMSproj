using DBLayer.Models;
using HMSModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjednavkaController : BaseController<Objednavka>
    {
        public ObjednavkaController(DBLayer.DBContext context) : base(context)
        {
        }

        //[HttpPost("Create")]
        public override async Task<IActionResult> CreateAsync([FromBody] Objednavka entity)
        {
            //if (!Objednavka.JeObjednavkaOK(objednavka))
            //    {
            //        return BadRequest("Invalid Objednavka data.");
            //    }
            try
            {
                if (!string.IsNullOrEmpty(entity.ID))   //ak uz tu je ID tak len skontrolujeme existenciu
                {
                    if (_db.Objednavky.Any(x => x.ID == entity.ID))
                    {
                        return BadRequest("Objednavka s daným ID už existuje.");
                    }
                }
                else
                {
                    entity.ID = Objednavka.DajNoveID(_db);
                }

                _db.Entry(entity.DodavatelX).State = EntityState.Unchanged;
                _db.Entry(entity.OdberatelX).State = EntityState.Unchanged;
                _db.Entry(entity.TvorcaX).State = EntityState.Unchanged;

                return await base.CreateAsync(entity);
            }
            catch (Exception e)
            {
                return Problem(e?.Message, statusCode: 499);
            }
        }

        public override IActionResult GetNextID()
        {
            try
            {
                var response = new ValueResponse();
                response.Value = Objednavka.DajNoveID(_db);
                response.TypeOfValue = nameof(System.String);
                return Ok(response);
            }
            catch (Exception e)
            {
                return Problem(e?.Message);
            }
        }

    }
}
