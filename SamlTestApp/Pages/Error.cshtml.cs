using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace SAMLTEST.Pages
{
    /// <summary>
    /// This is the Error Page Model
    /// </summary>
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }
        public string ErrorMessage { get; set; }
        public string Title { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// This method is used to display custom errors
        /// StatusCode    -   will be passed from UseStatusCodePagesWithRedirects  in the
        ///                   StartUp class. Error
        /// ErrorMessage  -   Will be populated from SAML Responses
        /// All other unhandled messages will also be displayed.
        /// </summary>
        public void OnGet(string ErrorMessage,string StatusCode="200")
        {
            // This is sample code to catch password reset / forgot
            // Please use response error description AADB2C90118 to handle this is a more proper way
            // See https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-flow-overview for more information on flows

            if (!string.IsNullOrEmpty(ErrorMessage) &&
               ErrorMessage.ToLower().Contains("forgot") &&
               ErrorMessage.ToLower().Contains("password"))
            {
                HttpContext.Response.Redirect("/?policy=PasswordReset");
            }


            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
            Title = "Unhandled Error.";
            switch (StatusCode)
            {
                case "404":
                    this.Title = "Page does not exist.";
                    break;
                case "200":
                    if(!string.IsNullOrEmpty(ErrorMessage))
                    {
                        Title = "SAML Error";
                        this.ErrorMessage = ErrorMessage;                        
                    }
                    else
                    {
                        if (null != exception)
                        {
                            this.ErrorMessage = $"ex.Message: [{exception.Error.Message}]";
                        }
                    }
                    break;
                default:
                    if (null != exception)
                    {
                        this.ErrorMessage = $"ex.Message: [{exception.Error.Message}]";
                    }
                    break;
            }   
        }
    }
}
