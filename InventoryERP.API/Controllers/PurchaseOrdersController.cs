using InventoryERP.Infrastructure;
using InventoryERP.Infrastructure.Entities;
using InventoryERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace InventoryERP.API.Controllers;

/// <summary>
/// 采购订单管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly AppDbContext _db;
    public PurchaseOrdersController(IUnitOfWork uow, AppDbContext db) { _uow = uow; _db = db; }

    /// <summary>
    /// 获取所有采购订单列表
    /// </summary>
    /// <returns>返回采购订单列表</returns>
    /// <response code="200">成功返回采购订单列表</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PurchaseOrder>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get() => Ok(await _uow.PurchaseOrders.GetAllAsync());

    /// <summary>
    /// 根据ID获取采购订单详情（包含订单项）
    /// </summary>
    /// <param name="id">采购订单ID</param>
    /// <returns>返回采购订单详情</returns>
    /// <response code="200">成功返回采购订单详情</response>
    /// <response code="404">采购订单不存在</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PurchaseOrder), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var po = await _db.PurchaseOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (po == null) return NotFound();
        return Ok(po);
    }

    /// <summary>
    /// 创建新采购订单
    /// </summary>
    /// <param name="po">采购订单信息</param>
    /// <returns>返回创建的采购订单</returns>
    /// <response code="201">采购订单创建成功</response>
    /// <response code="400">请求参数错误</response>
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseOrder), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PurchaseOrder po)
    {
        // basic: create purchase order
        await _uow.PurchaseOrders.AddAsync(po);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = po.Id }, po);
    }

    /// <summary>
    /// 接收采购订单货物（更新库存）
    /// </summary>
    /// <param name="id">采购订单ID</param>
    /// <returns>返回接收结果</returns>
    /// <response code="200">货物接收成功</response>
    /// <response code="404">采购订单不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("{id}/receive")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Receive(int id)
    {
        // naive receive: increase product stock by qty (demo)
        var po = await _db.PurchaseOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (po == null) return NotFound();

        foreach(var it in po.Items)
        {
            var product = await _db.Products.FindAsync(it.ProductId);
            if (product != null)
            {
                product.Stock += it.Quantity;
            }
        }
        
        po.Status = "Received";
        await _db.SaveChangesAsync();
        return Ok(new { message = "采购订单已接收，库存已更新" });
    }
}