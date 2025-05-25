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
    public class PropertyController : ControllerBase
    {
        readonly IProductUnIts _proUnit;
        readonly IMapper _proMap;
        public PropertyController(IProductUnIts proUnit, IMapper proMap)
        {
            _proUnit = proUnit;
            _proMap = proMap;
        }


        [HttpGet("AllProp")]
        public async Task<IActionResult> allProp(string? prop, int pg = 1, int itemsPerPage = 8)
        {
            try
            {
                var propList = await _proUnit.Properity.GetAll();
                if (!string.IsNullOrEmpty(prop))
                    propList = propList.Where(p => p.PropName.Contains(prop)).ToList();
                if (!propList.Any()) return NotFound("No Property Found");
                return Ok(_proUnit.Properity.ManageListPages(propList, pg, itemsPerPage));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("SpecificProp")]
        public async Task<IActionResult> specificProp(string propID)
        {
            try
            {
                var prop = await _proUnit.Properity.GetByStringID(propID);
                if (prop is null) return NotFound("No Property Found");
                return Ok(prop);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("AddProp")]
        public async Task<IActionResult> addProp([FromBody] ProperyDTO propDTO)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid Model");
            try
            {
                var propData = _proMap.Map<ProperyDTO, Property>(propDTO);
                propData.PropCode = _proUnit.Properity.GeneratePropCode();
                var result = await _proUnit.Properity.AddItem(propData);
                await _proUnit.SubmitAsync();
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("UpdateProp")]
        public async Task<IActionResult> updateProp([FromBody] ProperyDTO properyDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var prop = await _proUnit.Properity.GetByStringID(properyDTO.PropID);
                if (prop is null) return NotFound("No Property Found");
                prop= _proMap.Map<ProperyDTO, Property>(properyDTO);
                var result = await _proUnit.Properity.Update(prop.PropID, prop);
                await _proUnit.SubmitAsync();
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("DeleteProp")]
        public async Task<IActionResult> deleteProp(string propID, int pg = 1, int itemsPerPage = 8)
        {
            try
            {
                var prop = await _proUnit.Properity.GetByStringID(propID);
                if (prop is null) return NotFound("No Property Found");
                var result = await _proUnit.Properity.Delete(propID);
                await _proUnit.SubmitAsync();
                return Ok(_proUnit.Properity.ManageListPages(result, pg, itemsPerPage));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}
