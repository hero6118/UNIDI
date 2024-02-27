using Microsoft.AspNetCore.Mvc;
using Core;
namespace UniDi.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CronjobController : ControllerBase
    {
        [HttpGet("[action]")]
        public string Cronjob_1H()   // CALL CRONJOB > ORG
        {
            Task.Run(() => C_BlockChain.ScanWallet());
            Task.Run(()=> C_BlockChain.UpdatePriceToken());
            return "Đã xong";
        }
        [HttpGet("[action]")]
        public bool ScanWallet()
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return false;

            C_BlockChain.ScanWallet(user.Id);
            return true;
        }
    }
}
