namespace SearchAiDirectory.Shared.Services;

public interface IUserService
{
    Task<IList<User>> GetAllUsers();
    Task<IList<User>> GetActiveUsers();
    Task<IList<User>> GetUsersWithTool();
    Task<bool> EmailExisits(string email);
    Task<bool> SignUp(User newUser);
    Task<User> Login(string email, string password, bool persist);
    Task<User> GetUserByID(long userID);
    Task<User> GetUserByEmail(string email);
    Task<bool> ValidPassword(string email, string password);
    Task<bool> ConfirmEmail(string email);
    Task<bool> UpdateUserNameAvatar(long userID, string name, string avatar);
    Task<string> CreateEmailCode(string email);
    Task<bool> ConfirmEmailCode(string email, string code);
    Task<bool> ChangePassword(string email, string password);
    Task<bool> DeleteAccount(long userID);
}

public class UserService(ApplicationDataContext db) : IUserService
{
    public async Task<IList<User>> GetAllUsers()
        => await db.Users.ToListAsync();

    public async Task<IList<User>> GetActiveUsers()
        => await db.Users.Where(w => w.IsActive).ToListAsync();

    public async Task<IList<User>> GetUsersWithTool()
        => await db.Users.Where(w => w.ToolID.HasValue).ToListAsync();

    public async Task<bool> EmailExisits(string email)
        => await db.Users.AnyAsync(w => w.Email.ToLower() == email.ToLower());

    public async Task<bool> SignUp(User newUser)
    {
        var saltHash = await HashPassword(newUser.Email, newUser.Password);
        newUser.SaltCode = saltHash.Key;
        newUser.Password = saltHash.Value;
        newUser.IsActive = true;
        newUser.Avatar = "../img/avatars/profiles/avatar-0.jpg";
        newUser.Registration = DateTime.UtcNow;

        await db.Users.AddAsync(newUser);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<User> Login(string email, string password, bool persist)
    {
        var saltHash = await HashPassword(email, password);
        var user = await db.Users
            .Include(i => i.Tool)
            .Where(w => w.Email.ToLower() == email.ToLower() && w.Password == saltHash.Value && w.IsActive)
            .SingleOrDefaultAsync();

        if (user is null) return null;

        await db.Users.Where(w => w.ID == user.ID).ExecuteUpdateAsync(u => u
            .SetProperty(s => s.LastLogin, DateTime.UtcNow));

        return user;
    }

    public async Task<User> GetUserByID(long userID)
        => await db.Users.SingleOrDefaultAsync(w => w.ID == userID);

    public async Task<User> GetUserByEmail(string email)
        => await db.Users.SingleOrDefaultAsync(w => w.Email == email);

    public async Task<bool> ValidPassword(string email, string password)
    {
        var saltHash = await HashPassword(email, password);
        return await db.Users.AnyAsync(w => w.Email == email && w.Password == saltHash.Value);
    }

    public async Task<bool> ConfirmEmail(string email)
    {
        await db.Users.Where(w => w.Email == email)
            .ExecuteUpdateAsync(u => u
            .SetProperty(s => s.EmailConfirmed, true));
        return true;
    }

    public async Task<bool> UpdateUserNameAvatar(long userID, string name, string avatar)
    {
        await db.Users
            .Where(w => w.ID == userID)
            .ExecuteUpdateAsync(u => u
            .SetProperty(s => s.Name, name)
            .SetProperty(s => s.Avatar, avatar));

        return true;
    }

    public async Task<string> CreateEmailCode(string email)
    {
        string code = string.Empty;
        for (int i = 0; i < 5; i++)
            code += new Random().Next(0, 9).ToString();

        await db.UserCodes.AddAsync(new()
        {
            Email = email,
            Code = code,
            DateCreated = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return code;
    }

    public async Task<bool> ConfirmEmailCode(string email, string code)
        => await db.UserCodes.AnyAsync(a => a.Email == email && a.Code == code);

    public async Task<bool> ChangePassword(string email, string password)
    {
        var saltpassword = await HashPassword(email, password);
        await db.Users.Where(w => w.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase))
            .ExecuteUpdateAsync(u => u
            .SetProperty(s => s.SaltCode, saltpassword.Key)
            .SetProperty(s => s.Password, saltpassword.Value));
        return true;
    }

    public async Task<bool> DeleteAccount(long userID)
    {
        await db.Users.Where(w => w.ID == userID)
            .ExecuteUpdateAsync(u => u
            .SetProperty(s => s.IsActive, false));
        return true;
    }

    private async Task<KeyValuePair<string, string>> HashPassword(string email, string password)
    {
        var user = await db.Users.Where(w => w.Email == email).FirstOrDefaultAsync();
        var salt = user is null || string.IsNullOrEmpty(user.SaltCode) ? Guid.NewGuid().ToString("N") : user.SaltCode;
        var passwordHash = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

        return new KeyValuePair<string, string>(salt, passwordHash);
    }
}