using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace PiecebyPiece.Filters
{
    public class AdminAuthorizeFilter : IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminAuthorizeFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var session = _httpContextAccessor.HttpContext.Session;

            // ดึงค่า email admin จาก Session
            var email = session.GetString("adminEmail");

            // ถ้าไม่ได้ login ด้วย admin → ส่งกลับไปหน้า Login
            if (string.IsNullOrEmpty(email))
            {
                context.Result = new RedirectToActionResult("Login", "cUser", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
