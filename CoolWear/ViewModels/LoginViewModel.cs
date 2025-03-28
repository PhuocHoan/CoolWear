using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.ViewModels;

public class LoginViewModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; } = false;

    public bool CanLogin()
    {
        return true;
    }

    public bool Login()
    {
        if (((Username == "admin")
            || Username == "tester"
            )
            && (Password == "1234")
        )
        {
            return true;
        }
        return false;
    }
}
