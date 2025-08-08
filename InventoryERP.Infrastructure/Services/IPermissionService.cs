using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryERP.Infrastructure.Entities;

namespace InventoryERP.Infrastructure.Services;

public interface IPermissionService
{
    // 获取权限
    Task<Permission?> GetByIdAsync(int id);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<IEnumerable<Permission>> GetByModuleAsync(string module);
    
    // 创建权限
    Task<Permission> CreateAsync(Permission permission);
    
    // 更新权限
    Task<Permission> UpdateAsync(Permission permission);
    
    // 删除权限
    Task<bool> DeleteAsync(int id);
    
    // 获取所有模块
    Task<IEnumerable<string>> GetAllModulesAsync();
    
    // 获取使用此权限的角色
    Task<IEnumerable<Role>> GetRolesWithPermissionAsync(int permissionId);
}