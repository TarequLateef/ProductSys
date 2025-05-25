using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Core;
using Product.Core.DbStructs;
using Product.Core.DTOs.ProSch;
using Product.Core.Models.ProdSch;

namespace Product.API.Controllers.ProdSch
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeasureController : ControllerBase
    {
        readonly IProductUnIts _proUnit;
        readonly IMapper _proMap;
        public MeasureController(IProductUnIts proUnit, IMapper proMap)
        {
            _proUnit=proUnit;
            _proMap=proMap;
        }


        [HttpGet("MeasureList")]
        public async Task<IActionResult> measureList(string? measure, int pg, int itemsPerPage)
        {
            try
            {
                var measList = await _proUnit.MeasureUnit.Find(new[] { GoodTables.Properties });
                var result = _proUnit.MeasureUnit.ManageListPages(measList, pg, itemsPerPage);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }


        [HttpGet("MeasureInfo")]
        public async Task<IActionResult> measureInfo(string id)
        {
            try
            {
                var resItem = await _proUnit.MeasureUnit.GetByStringID(id);
                if (resItem is null) return NotFound("Measurement Not Found");
                resItem.PropertyTBL = await _proUnit.MeasureUnit.PropData(resItem.PropID);
                return Ok(resItem);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }


        [HttpPost("AddMeasure")]
        public async Task<IActionResult> addMeasure([FromBody] MeasureDTO dTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var measure = _proMap.Map<MeasureDTO, Measurment>(dTO);
                var result = await _proUnit.MeasureUnit.AddItem(measure);
                result.MeasureCode = _proUnit.MeasureUnit.GenerateMeasureCode(result.PropID);
                result.PropFullCode = await _proUnit.MeasureUnit.GetPropCode(result.PropID)
                    +result.MeasureCode;
                await _proUnit.SubmitAsync();
                result.PropertyTBL = await _proUnit.MeasureUnit.PropData(result.PropID);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("UpdateMeasure")]
        public async Task<IActionResult> updateMeasure([FromBody] MeasureDTO dTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var measure = await _proUnit.MeasureUnit.GetByStringID(dTO.MeasureID);
                if (measure == null) return NotFound("Measurement is not found");
                measure = _proMap.Map<MeasureDTO, Measurment>(dTO);
                await _proUnit.SubmitAsync();
                measure.PropertyTBL = await _proUnit.MeasureUnit.PropData(measure.PropID);
                return Ok(measure);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("DeleteMeasure")]
        public async Task<IActionResult> deleteMeasure(string id, int pg, int itemsPerPage)
        {
            try
            {
                var measure = await _proUnit.MeasureUnit.GetByStringID(id);
                var result = await _proUnit.MeasureUnit.Delete(id);
                await _proUnit.SubmitAsync();
                return Ok(_proUnit.MeasureUnit.ManageListPages(result, pg, itemsPerPage));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}
