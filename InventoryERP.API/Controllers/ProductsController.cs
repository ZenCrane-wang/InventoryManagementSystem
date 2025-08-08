using InventoryERP.Infrastructure.Entities;
using InventoryERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InventoryERP.API.Controllers;

/// <summary>
/// 产品管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public ProductsController(IUnitOfWork uow) => _uow = uow;

    /// <summary>
    /// 获取所有产品列表
    /// </summary>
    /// <returns>返回产品列表</returns>
    /// <response code="200">成功返回产品列表</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get() => Ok(await _uow.Products.GetAllAsync());

    /// <summary>
    /// 根据ID获取产品详情
    /// </summary>
    /// <param name="id">产品ID</param>
    /// <returns>返回产品详情</returns>
    /// <response code="200">成功返回产品详情</response>
    /// <response code="404">产品不存在</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var p = await _uow.Products.GetByIdAsync(id);
        if (p == null) return NotFound();
        return Ok(p);
    }

    /// <summary>
    /// 创建新产品
    /// </summary>
    /// <param name="p">产品信息</param>
    /// <returns>返回创建的产品</returns>
    /// <response code="201">产品创建成功</response>
    /// <response code="400">请求参数错误</response>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Product p)
    {
        await _uow.Products.AddAsync(p);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
    }

    /// <summary>
    /// 更新产品信息
    /// </summary>
    /// <param name="id">产品ID</param>
    /// <param name="p">更新的产品信息</param>
    /// <returns>无内容返回</returns>
    /// <response code="204">产品更新成功</response>
    /// <response code="400">请求参数错误</response>
    /// <response code="404">产品不存在</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Product p)
    {
        var exists = await _uow.Products.GetByIdAsync(id);
        if (exists == null) return NotFound();
        p.Id = id;
        _uow.Products.Update(p);
        await _uow.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// 删除产品
    /// </summary>
    /// <param name="id">产品ID</param>
    /// <returns>无内容返回</returns>
    /// <response code="204">产品删除成功</response>
    /// <response code="404">产品不存在</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var exists = await _uow.Products.GetByIdAsync(id);
        if (exists == null) return NotFound();
        _uow.Products.Remove(exists);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}