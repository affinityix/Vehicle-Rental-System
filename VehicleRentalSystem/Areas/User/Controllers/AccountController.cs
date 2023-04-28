﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
using VehicleRentalSystem.Application.Interfaces.Services;
using VehicleRentalSystem.Domain.Constants;
using VehicleRentalSystem.Domain.Entities;
using VehicleRentalSystem.Presentation.Areas.User.ViewModels;

namespace VehicleRentalSystem.Presentation.Areas.User.Controllers;

[Area("User")]
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ICustomerService _customerService;
    private readonly IStaffService _staffService;
    private readonly IFileTransferService _fileService;

    public AccountController(UserManager<IdentityUser> userManager, 
        RoleManager<IdentityRole> roleManager, 
        SignInManager<IdentityUser> signInManager, 
        IEmailSender emailSender, 
        ICustomerService customerService,
        IStaffService staffService,
        IFileTransferService fileService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _customerService = customerService;
        _staffService = staffService;
        _fileService = fileService;
    }

    #region Razor Pages
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var register = new RegisterViewModel();

        return View(register);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterConfirmation(string email, string role, string returnUrl = null)
    {
        if (email == null)
        {
            return RedirectToPage("/Index");
        }

        returnUrl = returnUrl ?? Url.Content("~/");

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return NotFound($"Unable to load user with email '{email}'.");
        }

        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> EmailConfirmation(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return View("Error");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return View("Error");
        }

        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        var result = await _userManager.ConfirmEmailAsync(user, token);

        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnurl = null)
    {
        ViewData["ReturnUrl"] = returnurl;

        var login = new LoginViewModel();

        return View(login);
    }
    #endregion

    #region API Calls
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        returnUrl = returnUrl ?? Url.Content("~/");
        
        var image = Request.Form.Files.FirstOrDefault();

        var user = new AppUser()
        {
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            Address = model.Address,
            State = model.State,
            UserName = model.Email,
            Image = _fileService.ImageByte(image),
            ImageURL = _fileService.FilePath(image, Constants.User, model.FullName, Constants.Customer)
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Constants.Customer);

            var customer = new Customer()
            {
                UserId = user.Id
            };

            _customerService.AddCustomer(customer);
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Action(
            "EmailConfirmation", 
            "Account", 
            new { Area = "User", userId = user.Id, code = code }, 
            protocol: HttpContext.Request.Scheme);

        await _emailSender.SendEmailAsync(model.Email, "Email Confirmation",
                    $"Dear {user.FullName},<br><br>You have been registered to our system as a customer. <br>To confirm your email address, please continue by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br><br>Regards,<br>Hajur ko Car Rental");

        return RedirectToAction("RegisterConfirmation", new { email = model.Email, returnUrl = returnUrl });

    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        returnUrl ??= Url.Content("~/");

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
    }
    #endregion
}
