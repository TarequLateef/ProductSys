using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Core;
using Product.Core.DbStructs;
using Product.Core.DTOs.ProSch;
using Product.Core.Models.ProdSch;
using SharedLiberary.General.DbStructs;
using SharedLiberary.Interfaces;

namespace Product.API.Controllers.ProdSch
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdValueController : ControllerBase
    {
        readonly IProductUnIts _proUnit;
        readonly IMapper _proMap;
        public ProdValueController(IProductUnIts proUnit, IMapper proMap)
        { _proUnit = proUnit; _proMap = proMap; }

        [HttpGet("PropValueList")]
        public async Task<IActionResult> prodValList(string? val, string? prop, bool? aval, int pg = 1, int itemsPerPage = 8)
        {
            try
            {
                var proValList = aval.HasValue ? aval.Value ?
                    await _proUnit.PropValue.AvailableListAsync()
                    : await _proUnit.PropValue.BannedListAsync()
                    : await _proUnit.PropValue.Find(pv => new[] { GoodTables.Properties, GoodTables.BaseProduct });

                if (!proValList.Any()) return NotFound("No Product Value Found");
                // filter by property value
                if (!string.IsNullOrEmpty(val))
                    proValList = _proUnit.PropValue.SearchValue(proValList, val);
                if (!proValList.Any()) return NotFound("No Product Value Found");
                //filter by property name
                if(!string.IsNullOrEmpty(prop))
                    proValList = _proUnit.PropValue.SearchPorprety(proValList, prop);
                if (!proValList.Any()) return NotFound("No Product Value Found");
                return Ok(_proUnit.PropValue.ManageListPages(proValList, pg, itemsPerPage));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("PropValueDetails")]
        public async Task<IActionResult> prodValDetails(string id)
        {
            try
            {
                var propVal = await _proUnit.PropValue.GetByStringID(id);
                if (propVal == null) return NotFound("No Product Value Found");
                propVal = await _proUnit.PropValue.PropValDetailsAsync(propVal);
                return Ok(propVal);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("AddPropValue")]
        public async Task<IActionResult> addPropValue([FromBody] PropValueDTO pvDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var propVal = _proMap.Map<PropValueDTO,PropValue>(pvDTO);
                propVal.ValCode=_proUnit.PropValue.ValueCode(propVal.PropID, propVal.ProdID);
                var result = await _proUnit.PropValue.AddItem(propVal);
                await _proUnit.SubmitAsync();
                return Ok(await _proUnit.PropValue.PropValDetailsAsync(result));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }

        }

        [HttpPut("UpdatePropValue")]
        public async Task<IActionResult> updatePropValue([FromBody] PropValueDTO pvDTO)
        {
            if(!ModelState.IsValid) return BadRequest("Model is not valid");
            try
            {
                var propVal = await _proUnit.PropValue.GetByStringID(pvDTO.PropValID);
                propVal = _proMap.Map(pvDTO, propVal);
                await _proUnit.SubmitAsync();
                return Ok(propVal);
            }
            catch(Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("RestoreStop")]
        public async Task<IActionResult> restoreStopPropVal(string id)
        {
            try
            {
                var propVal =await _proUnit.PropValue.GetByStringID(id);
                if (propVal == null) return NotFound("No Product Value Found");
                propVal = _proUnit.PropValue.RestorStop(propVal);
                await _proUnit.SubmitAsync();
                propVal = await _proUnit.PropValue.PropValDetailsAsync(propVal);
                return Ok(propVal);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }


        [HttpDelete("DeletePropValue")]
        public async Task<IActionResult> deletePropVal(string id, int pg, int itemPerPage)
        {
            try
            {
                var propVal = await _proUnit.PropValue.GetByStringID(id);
                if (propVal == null) return NotFound("No Product Value Found");
                var result = await _proUnit.PropValue.Delete(id);
                await _proUnit.SubmitAsync();
                return Ok(_proUnit.PropValue.ManageListPages(result,pg,itemPerPage)); // return list of items without the deleted item
            }
            catch(Exception ex) { return BadRequest(ex.Message); }
        }


    }
}
