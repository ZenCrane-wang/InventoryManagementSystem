using InventoryERP.Infrastructure.Entities;
using InventoryERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace InventoryERP.API.Controllers;

/// <summary>
/// 供应商管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SuppliersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public SuppliersController(IUnitOfWork uow) => _uow = uow;

    /// <summary>
    /// 获取所有供应商列表
    /// </summary>
    /// <returns>返回供应商列表</returns>
    /// <response code="200">成功返回供应商列表</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Supplier>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get() => Ok(await _uow.Suppliers.GetAllAsync());

    /// <summary>
    /// 创建新供应商
    /// </summary>
    /// <param name="s">供应商信息</param>
    /// <returns>返回创建的供应商</returns>
    /// <response code="201">供应商创建成功</response>
    /// <response code="400">请求参数错误</response>
    [HttpPost]
    [ProducesResponseType(typeof(Supplier), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Supplier s)
    {
        await _uow.Suppliers.AddAsync(s);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = s.Id }, s);
    }
}