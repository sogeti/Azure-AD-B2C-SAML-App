using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SAMLTEST.SAMLObjects;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SAMLTEST.Pages.SP
{
    /// <summary>
    /// This is the Index Page Model for the Service Provider
    /// </summary>
    public class IndexModel : PageModel
    {
        [DisplayName("Tenant Name"), Required]
        public string Tenant { get; private set; }

        [DisplayName("B2C Policy"),Required]
        public string Policy { get; private set; }

        [DisplayName("Issuer")]
        public string Issuer { get; private set; }

        [DisplayName("DCInfo")]
        public string DCInfo { get; private set; } 

        /// <summary>
        /// This Constructor is used to retrieve the Appsettings data
        /// </summary>
        public IndexModel(IConfiguration configuration)
        {
            Issuer = configuration["Issuer"];
            Tenant = configuration["Tenant"];
            Policy = configuration["Policy"];
            DCInfo = configuration["DCInfo"];
        }

        /// <summary>
        /// This Post Action is used to Generate the AuthN Request and redirect to the B2C Login endpoint
        /// </summary>
        public IActionResult OnPost(string Tenant,string Policy, string Issuer, string DCInfo, bool IsAzureAD)
        {
            if (string.IsNullOrEmpty(Policy) || IsAzureAD)
            {
                return SendAzureAdRequest();
            }

            var TenantId = Tenant.ToLower()?.Replace(".onmicrosoft.com", string.Empty);
            var b2cloginurl = TenantId + ".b2clogin.com";
            Policy = Policy.StartsWith("B2C_1A_") ? Policy : "B2C_1A_" + Policy;
            Tenant = (Tenant.ToLower().Contains("onmicrosoft.com") || Tenant.ToLower().Contains(".net")) ? Tenant : Tenant + ".onmicrosoft.com";
            DCInfo = string.IsNullOrWhiteSpace(DCInfo) ? string.Empty : "&" + DCInfo;
            Issuer = string.IsNullOrWhiteSpace(Issuer) ? SAMLHelper.GetThisURL(this) : Issuer;

            var RelayState = $"{SAMLHelper.toB64(Tenant)}.{SAMLHelper.toB64(Policy)}.{SAMLHelper.toB64(Issuer)}";

            if (!string.IsNullOrEmpty(DCInfo))
            {
                RelayState += $".{SAMLHelper.toB64(DCInfo)}";
            }

            var URL = $"https://{b2cloginurl}/{Tenant}/{Policy}/samlp/sso/login?{DCInfo}";
            var AuthnReq = new AuthnRequest(URL, SAMLHelper.GetThisURL(this), Issuer);
            var cdoc = SAMLHelper.Compress(AuthnReq.ToString());
            URL += "&SAMLRequest=" + System.Web.HttpUtility.UrlEncode(cdoc) + "&RelayState=" + System.Web.HttpUtility.UrlEncode(RelayState);
            return Redirect(URL);
        }

        public IActionResult SendAzureAdRequest()
        {
            var AuthnReq = new AuthnRequest("https://login.microsoftonline.com/00000000-0000-0000-0000-000000000000/saml2", SAMLHelper.GetThisURL(this), string.Empty);
            var cdoc = SAMLHelper.Compress(AuthnReq.ToString());
            var URL = $"https://login.microsoftonline.com/00000000-0000-0000-0000-000000000000/saml2?SAMLRequest=" + System.Web.HttpUtility.UrlEncode(cdoc);
            return Redirect(URL);
        }

    }
}
