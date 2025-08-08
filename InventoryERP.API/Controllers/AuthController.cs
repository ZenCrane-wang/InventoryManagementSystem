using System;
using System.Threading.Tasks;
using InventoryERP.Infrastructure.Models;
using InventoryERP.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InventoryERP.API.Controllers;

/// <summary>
/// 身份验证控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }
    
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="model">登录请求模型</param>
    /// <returns>返回JWT令牌和用户信息</returns>
    /// <response code="200">登录成功，返回JWT令牌</response>
    /// <response code="400">请求参数错误</response>
    /// <response code="401">用户名或密码错误</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        try
        {
            var response = await _userService.AuthenticateAsync(model.Username, model.Password);
            
            if (response == null)
                return Unauthorized(new { message = "用户名或密码错误" });
                
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="model">注册请求模型</param>
    /// <returns>返回注册结果</returns>
    /// <response code="200">注册成功</response>
    /// <response code="400">请求参数错误或注册失败</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        try
        {
            // 验证密码确认
            if (model.Password != model.ConfirmPassword)
                return BadRequest(new { message = "密码和确认密码不匹配" });
                
            var user = await _userService.RegisterAsync(model);
            return Ok(new { message = "注册成功", userId = user.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="model">修改密码请求模型</param>
    /// <returns>返回修改结果</returns>
    /// <response code="200">密码修改成功</response>
    /// <response code="400">请求参数错误或当前密码错误</response>
    /// <response code="401">未授权访问</response>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
    {
        try
        {
            // 验证密码确认
            if (model.NewPassword != model.ConfirmNewPassword)
                return BadRequest(new { message = "新密码和确认密码不匹配" });
                
            // 获取当前用户ID
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized(new { message = "无效的用户身份" });
                
            var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
            
            if (!result)
                return BadRequest(new { message = "当前密码错误" });
                
            return Ok(new { message = "密码修改成功" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// 重置用户密码（管理员权限）
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>返回新密码</returns>
    /// <response code="200">密码重置成功</response>
    /// <response code="400">请求参数错误</response>
    /// <response code="401">未授权访问</response>
    /// <response code="403">权限不足</response>
    [Authorize(Roles = "Admin")]
    [HttpPost("reset-password/{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetPassword(int id)
    {
        try
        {
            var newPassword = await _userService.ResetPasswordAsync(id);
            return Ok(new { message = "密码重置成功", newPassword });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}