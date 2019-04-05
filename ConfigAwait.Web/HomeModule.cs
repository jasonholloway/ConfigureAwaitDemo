using Nancy;
using System.Threading.Tasks;

namespace caf_proof.web
{
     public class Home : NancyModule
    {
       public Home()
       {
            Get["/", true] = async (x, _) =>
            {
                await Task.Delay(10);
                return "HELLO JASON";
            };
       }
    }
}