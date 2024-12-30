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
    [ValidateAntiForgeryToken]
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

        if (!user.EmailConfirmed)
        {
            ViewBag.LoginMessage = "Could you please confirm your email.";
            return View();
        }

        await outputCache.EvictByTagAsync("global-cache", default);
        await auth.Authenticate(user, persist);

        return View();
    }

    [HttpGet]
    public IActionResult Signup()
    {
        var user = new User();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(User newUser)
    {
        var signupCompleted = await userService.SignUp(newUser);
        await SendGridService.SendWelcomeEmail(newUser.Email, newUser.Name);
        await SendGridService.AddContactToList(newUser.Email, newUser.Name);
        return signupCompleted ? Redirect("login") : View(newUser);
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (!await userService.EmailExisits(email))
        {
            ViewBag.LoginMessage = "Invalid Email Address.";
            return View();
        }

        string code = await userService.CreateEmailCode(email);
        var user = await userService.GetUserByEmail(email);
        await SendGridService.SendPasswordResetEmail(user.Email, user.Name, code);
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCode(string email, string code)
    {
        if (!await userService.EmailExisits(email))
        {
            ViewBag.Message = "Invalid Email Address.";
            return RedirectToAction("EnterCode", new { email });
        }

        return await userService.ConfirmEmailCode(email, code)
            ? RedirectToAction("ResetPassword", routeValues: new { email })
            : View();
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string email)
    {
        if (string.IsNullOrEmpty(email) || !await userService.EmailExisits(email))
            return RedirectToAction("Login");

        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string email, string password)
    {
        if (await userService.ChangePassword(email, password))
        {
            ViewBag.LoginMessage = "Password Changed.";
            var user = await userService.GetUserByEmail(email);
            await SendGridService.SendPasswordChangedEmail(user.Email, user.Name);
            return RedirectToAction("Login");
        }
        else
        {
            ViewBag.Message = "Unable to change password, please contact support at support@searchaidirectory.com";
            return RedirectToAction("ResetPassword", new { email });
        }
    }

    [HttpPost]
    public IActionResult CheckEmailExisit(string email)
        => Json(userService.EmailExisits(email).Result);

    [HttpGet("/confirmemail/{email}")]
    public async Task<IActionResult> ConfirmEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

        return await userService.ConfirmEmail(email)
            ? View("EmailConfirmed")
            : Redirect("login");
    }

    [HttpGet]
    public JsonResult GetUserAvatar()
    {
        var userID = Convert.ToInt64(httpContext.User.Claims.Where(c => c.Type == "nameid").First().Value);
        var user = userService.GetUserByID(userID).Result;
        return Json(user.Avatar);
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var userID = Convert.ToInt64(httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
        var user = await userService.GetUserByID(userID);
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Settings(User model, IFormFile avatar, string confirmEmail)
    {
        var userID = Convert.ToInt64(httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
        var user = await userService.GetUserByID(userID);

        // Check if email confirmation matches
        if (model.Email != confirmEmail)
        {
            ModelState.AddModelError("Email", "Email confirmation does not match.");
            return View("Index", model);
        }

        // Update user details
        user.Name = model.Name;
        user.Email = model.Email;

        // Handle avatar upload
        if (avatar is not null && avatar.Length > 0)
        {
            // Upload to Azure Storage (implement your Azure upload logic here)
            var avatarUrl = await UploadToAzureStorage(avatar);
            user.Avatar = avatarUrl;
        }

        await userService.UpdateUserNameAvatar(userID, user.Name, user.Avatar);
        return RedirectToAction("Index");
    }

    private async Task<string> UploadToAzureStorage(IFormFile file)
    {
        var userID = Convert.ToInt64(httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);

        string fileName = "user-" + userID + Path.GetExtension(file.FileName);
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        byte[] fileBytes = memoryStream.ToArray();
        return await AzureStorageService.UploadBlobFiles(BlobContainers.user, fileName, fileBytes);
    }
}