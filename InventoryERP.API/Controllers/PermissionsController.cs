using System;
using System.Threading.Tasks;
using InventoryERP.Infrastructure.Entities;
using InventoryERP.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    
    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var permission = await _permissionService.GetByIdAsync(id);
            if (permission == null)
                return NotFound(new { message = "权限不存在" });
                
            return Ok(permission);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("modules")]
    public async Task<IActionResult> GetAllModules()
    {
        try
        {
            var modules = await _permissionService.GetAllModulesAsync();
            return Ok(modules);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("modules/{module}")]
    public async Task<IActionResult> GetByModule(string module)
    {
        try
        {
            var permissions = await _permissionService.GetByModuleAsync(module);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Permission permission)
    {
        try
        {
            var newPermission = await _permissionService.CreateAsync(permission);
            return CreatedAtAction(nameof(GetById), new { id = newPermission.Id }, newPermission);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Permission permission)
    {
        try
        {
            if (id != permission.Id)
                return BadRequest(new { message = "权限ID不匹配" });
                
            var existingPermission = await _permissionService.GetByIdAsync(id);
            if (existingPermission == null)
                return NotFound(new { message = "权限不存在" });
                
            var updatedPermission = await _permissionService.UpdateAsync(permission);
            return Ok(updatedPermission);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _permissionService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "权限不存在" });
                
            return Ok(new { message = "权限删除成功" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetPermissionRoles(int id)
    {
        try
        {
            var permission = await _permissionService.GetByIdAsync(id);
            if (permission == null)
                return NotFound(new { message = "权限不存在" });
                
            var roles = await _permissionService.GetRolesWithPermissionAsync(id);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}