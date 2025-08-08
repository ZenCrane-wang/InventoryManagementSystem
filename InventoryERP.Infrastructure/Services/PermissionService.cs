using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryERP.Infrastructure.Entities;
using InventoryERP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryERP.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    
    public PermissionService(AppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Permission?> GetByIdAsync(int id)
    {
        return await _context.Permissions.FindAsync(id);
    }
    
    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions.ToListAsync();
    }
    
    public async Task<IEnumerable<Permission>> GetByModuleAsync(string module)
    {
        return await _context.Permissions
            .Where(p => p.Module == module)
            .ToListAsync();
    }
    
    public async Task<Permission> CreateAsync(Permission permission)
    {
        // 检查是否已存在相同模块和操作的权限
        if (await _context.Permissions.AnyAsync(p => 
            p.Module == permission.Module && p.Action == permission.Action))
        {
            throw new System.Exception($"已存在相同模块和操作的权限: {permission.Module}.{permission.Action}");
        }
        
        _context.Permissions.Add(permission);
        await _unitOfWork.SaveChangesAsync();
        return permission;
    }
    
    public async Task<Permission> UpdateAsync(Permission permission)
    {
        // 检查是否已存在相同模块和操作的其他权限
        if (await _context.Permissions.AnyAsync(p => 
            p.Module == permission.Module && 
            p.Action == permission.Action && 
            p.Id != permission.Id))
        {
            throw new System.Exception($"已存在相同模块和操作的其他权限: {permission.Module}.{permission.Action}");
        }
        
        _context.Permissions.Update(permission);
        await _unitOfWork.SaveChangesAsync();
        return permission;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
            return false;
            
        // 检查是否有角色使用此权限
        var rolesWithPermission = await _context.RolePermissions.AnyAsync(rp => rp.PermissionId == id);
        if (rolesWithPermission)
        {
            throw new System.Exception("无法删除权限，因为有角色正在使用此权限");
        }
        
        _context.Permissions.Remove(permission);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<string>> GetAllModulesAsync()
    {
        return await _context.Permissions
            .Select(p => p.Module)
            .Distinct()
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Role>> GetRolesWithPermissionAsync(int permissionId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.PermissionId == permissionId)
            .Select(rp => rp.Role)
            .ToListAsync();
    }
}