using System.Collections.Generic;

namespace InventoryERP.Infrastructure.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Module { get; set; } = null!; // 模块名称，如：产品、供应商、采购订单等
    public string Action { get; set; } = null!; // 操作类型，如：查看、创建、编辑、删除等
    
    // 导航属性
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}