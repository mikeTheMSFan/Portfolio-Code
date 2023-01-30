// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

#region Imports

using System.ComponentModel.DataAnnotations;
using System.Text;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Portfolio.Extensions;
using Portfolio.Models;
using Portfolio.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

#endregion

namespace Portfolio.Areas.Identity.Pages.Account;

[ValidateReCaptcha]
public class RegisterModel : PageModel
{
    private readonly IUserEmailStore<BlogUser> _emailStore;
    private readonly IMWSImageService _imageService;
    private readonly ILogger<RegisterModel> _logger;
    private readonly SignInManager<BlogUser> _signInManager;
    private readonly UserManager<BlogUser> _userManager;
    private readonly IUserStore<BlogUser> _userStore;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public RegisterModel(
        UserManager<BlogUser> userManager,
        IUserStore<BlogUser> userStore,
        SignInManager<BlogUser> signInManager,
        ILogger<RegisterModel> logger,
        IMWSImageService imageService,
        IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _imageService = imageService;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var validationError = false;
            if (Input.UserAcceptedTerms == false)
            {
                validationError = true;
                ModelState.AddModelError("Input.UserAcceptedTerms",
                    "You must our privacy policy terms to register.");
            }

            if (validationError)
            {
                ReturnUrl = returnUrl;
                return Page();
            }

            var user = CreateUser();
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.UserAcceptedTerms = Input.UserAcceptedTerms;

            if (Input.ImageFile == null)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var imagePath = "imgs\\defaultProfilePicture.png";
                var defaultProfilePicturePath = Path.Combine(webRootPath, imagePath);
                user.Image = await GetDefaultProfilePictureByteArrayAsync(defaultProfilePicturePath);
                user.ImageType = "image/png";
            }

            else if (Input.ImageFile != null && Input.ImageFile.IsImage())
            {
                var userAvatar = await _imageService.EncodeImageAsync(Input.ImageFile);
                user.ImageType = Input.ImageFile.ContentType;
                user.Image = userAvatar;
            }

            else
            {
                ModelState.AddModelError("Input.Image", "Please check your avatar.");
                return Page();
            }

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    null,
                    new { area = "Identity", userId, code, returnUrl },
                    Request.Scheme);

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });

                await _signInManager.SignInAsync(user, false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        }

        if (ModelState.ContainsKey("Recaptcha") &&
            ModelState["Recaptcha"]!.ValidationState == ModelValidationState.Invalid)
            ModelState.AddModelError(string.Empty, "reCaptcha Error.");

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private BlogUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<BlogUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(BlogUser)}'. " +
                                                $"Ensure that '{nameof(BlogUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                "override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private async Task<byte[]> GetDefaultProfilePictureByteArrayAsync(string profilePicturePath)
    {
        var ms = new MemoryStream();
        await (await Image.LoadAsync(profilePicturePath)).SaveAsync(ms, new PngEncoder());
        var imageFileOffset = ms.Length;
        var imageFile = new FormFile(ms, 0, imageFileOffset, null, "defaultProfilePicture.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        return await _imageService.EncodeImageAsync(imageFile);
    }

    private IUserEmailStore<BlogUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
            throw new NotSupportedException("The default UI requires a user store with email support.");

        return (IUserEmailStore<BlogUser>)_userStore;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //BlogUser Extra fields
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and no more than {1} characters long.",
            MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and no more than {1} characters long.",
            MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Avatar")] public IFormFile ImageFile { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public bool UserAcceptedTerms { get; set; }
    }
}