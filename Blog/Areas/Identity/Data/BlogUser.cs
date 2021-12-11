using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Models;
using Microsoft.AspNetCore.Identity;

namespace Blog.Areas.Identity.Data;

// Add profile data for application users by adding properties to the BlogUser class
public class BlogUser : IdentityUser
{
    [PersonalData] public string FullName { get; set; } = null!;
    [PersonalData] public string DisplayName { get; set; } = null!;
    [PersonalData] public Models.Blog? Blog { get; set; }
}

