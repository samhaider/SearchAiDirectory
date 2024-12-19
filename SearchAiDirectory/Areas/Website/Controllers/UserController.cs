namespace SearchAiDirectory.Areas.Website.Controllers;

[AllowAnonymous]
[Area("Website")]
[Route("[action]")]
public class UserController(IHttpContextAccessor httpContextAccessor, IUserService userService, IOutputCacheStore outputCache, IUserAuthenticator auth) : Controller
{
    private readonly HttpContext httpContext = httpContextAccessor.HttpContext;

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        if (httpContext.User.Claims.Any()) await httpContext.SignOutAsync();

        TempData.Clear();
        httpContext.Session.Clear();
        return Redirect("/");
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (!httpContext.User.Identity.IsAuthenticated) return View();

        return Redirect("/");
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, bool persist)
    {
        if (!await userService.EmailExisits(email))
        {
            ViewBag.LoginMessage = "Invalid Email Address.";
            return View();
        }

        var user = await userService.Login(email, password, persist);
        if (user is null)
        {
            ViewBag.LoginMessage = "Invalid Password.";
            return View();
        }

        await outputCache.EvictByTagAsync("global-cache", default);
        await auth.Authenticate(user, persist);

        TempData.Clear();
        TempData["UserID"] = user.ID.ToString();
        TempData["Name"] = $"{user.Name}";
        TempData["TimeZone"] = user.TimeZone;

        return View();
    }

    [HttpGet]
    public JsonResult GetUserAvatar()
    {
        var userID = Convert.ToInt64(httpContext.User.Claims.Where(c => c.Type == "nameid").First().Value);
        var user = userService.GetUserByID(userID).Result;
        return Json(user.Avatar);
    }

    [HttpGet]
    public IActionResult Signup()
    {
        var user = new User();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Signup(User newUser)
    {
        var signupCompleted = await userService.SignUp(newUser);
        //await emailService.SendWelcomeEmail(newUser.Email, $"{newUser.Name}");
        //await emailService.ListSignUp(newUser, await roleService.GetAllRoles());
        return signupCompleted ? Redirect("login") : View(newUser);
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (!await userService.EmailExisits(email))
        {
            ViewBag.LoginMessage = "Invalid Email Address.";
            return View();
        }

        //string code = await userService.CreateResetPasswordCode(email);
        //await emailService.SendResetCodeEmail(email, code);
        return RedirectToAction("EnterCode", routeValues: new { email });
    }

    [HttpGet]
    public async Task<IActionResult> EnterCode(string email, string code = null)
    {
        if (code is null)
        {
            ViewBag.Email = email;
            return View();
        }
        else
        {
            return await userService.ConfirmEmailCode(email, code)
                ? RedirectToAction("ResetPassword", routeValues: new { email })
                : View();
        }
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmCode(string email, string code)
        => await userService.ConfirmEmailCode(email, code)
        ? RedirectToAction("ResetPassword", routeValues: new { email })
        : View();

    [HttpGet]
    public IActionResult ResetPassword(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(string email, string password)
        => await userService.ChangePassword(email, password)
        ? RedirectToAction("Login")
        : RedirectToAction("ResetPassword", new { email });

    [HttpPost]
    public IActionResult CheckEmailExisit(string email)
        => Json(userService.EmailExisits(email).Result);

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string email)
        => await userService.ConfirmEmail(email)
        ? View("EmailConfirmed")
        : Redirect("login");
}