using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Core;
using Product.Core.DTOs.ProSch;
using Product.Core.Models.ProdSch;
using SharedLiberary.General.DbStructs;
using SharedLiberary.Interfaces;

namespace Product.API.Controllers.ProdSch
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        readonly IProductUnIts _proUnit;
        readonly IMapper _proMap;
        public ProductController(IProductUnIts proUnit, IMapper proMap)
        { _proUnit = proUnit; _proMap = proMap; }

        /// <summary>
        /// product list
        /// </summary>
        /// <param name="aval">active data or stoped or all data</param>
        /// <param name="pg">page number</param>
        /// <param name="itemPerPage">items of list shown in list</param>
        /// <returns>kind of data (available, stopped, all data)  to return</returns>
        [HttpGet("AllProducts")]
        public async Task<IActionResult> showProducts(bool? aval, int pg = 1, int itemPerPage = 8)
        {
            try
            {
                
                var proList = aval.HasValue ? aval.Value ?
                    await _proUnit.Product.AvailableListAsync()
                    : await _proUnit.Product.BannedListAsync()
                    : await _proUnit.Product.GetAll();
                if (!proList.Any()) return NotFound("No Product Found");
                return Ok(_proUnit.Product.ManageListPages(proList, pg, itemPerPage));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// product list after search
        /// </summary>
        /// <param name="pName">name or part of product name</param>
        /// <param name="aval">active data or stoped or all data</param>
        /// <param name="pg">page number</param>
        /// <param name="itemPerPage">items of list shown in list</param>
        /// <returns>kind of data (available, stopped, all data) </returns>
        [HttpGet("SearchbyName")]
        public async Task<IActionResult> SearchByName(string pName, bool? aval, int pg = 1, int itemPerPage = 8)
        {
            try
            {

                var proList = 
                    await _proUnit.Product.SearchByName(!aval.HasValue?true:aval.Value, pName);
                return Ok(_proUnit.Product.ManageListPages(proList, pg, itemPerPage));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ProductData")]
        public async Task<IActionResult> productData(string id)
        {
            try
            {
                var proData = await _proUnit.Product.GetByStringID(id);
                if (proData is null) return BadRequest("No product found");
                return Ok(proData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> addProduct([FromBody] ProductDTO product)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Invalid Model");

                //the product name enterd before thend return it 
                if (!_proUnit.Product.IsExists(p => p.ProdName == product.ProdName))
                    return Ok("This product already exists");

                Products pro = _proMap.Map<ProductDTO, Products>(product);
                //generate the code 
                pro.BaseCode = await _proUnit.Product.SetProductCode();
                var result = await _proUnit.Product.AddItem(pro);
                await _proUnit.SubmitAsync();
                if (result is null) return BadRequest("Product not added");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> updatePorduct([FromBody] ProductDTO proDto)
        {
            try
            {
                var proData = await _proUnit.Product.GetByStringID(proDto.ProdID);
                if (proData is null) return BadRequest("No product found");
                //check if the product name enterd before 
                if (_proUnit.Product.IsExists(p => p.ProdName == proDto.ProdName && p.ProdID != proDto.ProdID))
                    return BadRequest("This product could'nt be updated because there is another one with this name");
                Products pro = _proMap.Map<ProductDTO, Products>(proDto);
                pro.BaseCode = await _proUnit.Product.SetProductCode();
                await _proUnit.SubmitAsync();
                return Ok(pro);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("StopRetProduct")]
        public async Task<IActionResult> stopPorduct(string id)
        {
            try
            {
                var pro = await _proUnit.Product.GetByStringID(id);
                if (pro is null) return BadRequest("No product found");
                pro = await _proUnit.Product.RestoreStopAsync(pro);
                await _proUnit.SubmitAsync();
                return Ok(pro);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> deleteProduct(string id, int pg = 1, int itemPerPage = 8)
        {
            try
            {
                var pro = await _proUnit.Product.GetByStringID(id);
                if (pro is null) return BadRequest("No product found");
                if (_proUnit.PropValue.IsExists(pfd => pfd.ProdID == id))
                    return BadRequest("This product could'nt be deleted ");
                var result = await _proUnit.Product.Delete(id);
                await _proUnit.SubmitAsync();
                return Ok(_proUnit.Product.ManageListPages(result, pg, itemPerPage));

            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

    }
}
