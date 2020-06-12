using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SAMLTEST.SAMLObjects;
using System.Web;

namespace SAMLTEST.Pages
{
    /// <summary>
    /// This is the Index Page Model
    /// </summary>
    public class IndexModel : PageModel
    {
        public string Tenant { get; private set; }
        public string TenantId { get; private set; }
        public string Issuer { get; private set; }
        public string DCInfo { get; private set; }
        private readonly IConfiguration _configuration;

        /// <summary>
        /// This Constructor is used to retrieve the Appsettings data
        /// </summary>
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Issuer = _configuration["Issuer"];
            Tenant = _configuration["Tenant"];
            DCInfo = _configuration["DCInfo"];

            // Normalization
            TenantId = Tenant.ToLower()?.Replace(".onmicrosoft.com", string.Empty);
            DCInfo = string.IsNullOrWhiteSpace(DCInfo) ? string.Empty : "&" + DCInfo;
            Issuer = string.IsNullOrWhiteSpace(Issuer) ? SAMLHelper.GetThisURL(this) : Issuer;
        }

        /// <summary>
        /// Support both GET and POST on index for sample usage
        /// </summary>
        /// <param name="policy"></param>
        public void OnGet(string policy)
        {
            if (!string.IsNullOrEmpty(policy))
            {
                var redirect = (RedirectResult)RunPolicy(policy);
                HttpContext.Response.Redirect(redirect.Url);
            }
        }

        /// <summary>
        /// Support both GET and POST on index for sample usage
        /// </summary>
        /// <param name="policy"></param>
        public IActionResult OnPost(string policy)
        {
            return RunPolicy(policy);
        }

        private IActionResult RunPolicy(string policy)
        {
            if (!string.IsNullOrEmpty(_configuration[policy]))
            {
                policy = _configuration[policy];
            }
            else
            {
                policy = _configuration["Policy"];
            }
            return RunB2CLogin(policy, this);
        }

        private IActionResult RunB2CLogin(string policy, PageModel model)
        {
            var b2cloginurl = TenantId + ".b2clogin.com";
            Tenant = (Tenant.ToLower().Contains("onmicrosoft.com") || Tenant.ToLower().Contains(".net")) ? Tenant : Tenant + ".onmicrosoft.com";
            DCInfo = string.IsNullOrWhiteSpace(DCInfo) ? string.Empty : "&" + DCInfo;
            Issuer = string.IsNullOrWhiteSpace(Issuer) ? SAMLHelper.GetThisURL(model) : Issuer;

            var RelayState = $"{SAMLHelper.toB64(Tenant)}.{SAMLHelper.toB64(policy)}.{SAMLHelper.toB64(Issuer)}";

            if (!string.IsNullOrEmpty(DCInfo))
            {
                RelayState += "." + SAMLHelper.toB64(DCInfo);
            }

            var URL = $"https://{b2cloginurl}/{Tenant}/{policy}/samlp/sso/login?{DCInfo}";
            var AuthnReq = new AuthnRequest(URL, SAMLHelper.GetThisURL(model), Issuer);
            var cdoc = SAMLHelper.Compress(AuthnReq.ToString());
            URL += "&SAMLRequest=" + HttpUtility.UrlEncode(cdoc) + "&RelayState=" + HttpUtility.UrlEncode(RelayState);
            return Redirect(URL);
        }
    }
}
