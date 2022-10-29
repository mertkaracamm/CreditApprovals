using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CreditApproval_Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: CreditApproval
        [HttpGet]
        public ActionResult Index()
        {
            
            return View();
        }

     

        [HttpPost]
        public ActionResult Index(Models.CreditApplication application)
        {
            var model=new Models.CreditApplication();
          

            //Open HttpClient
            using (var client = new HttpClient())
            {
                //Populate data structure that will be posted to Azure ML Service
                ScoreData scoreData = new ScoreData()
                {
                    FeatureVector = new Dictionary<string, string>() 
                    {
                        { "Col1", application.StatusOfExistingCheckingAccount },
                        { "Col2", application.DurationInMonths },
                        { "Col3", application.CreditHistory },
                        { "Col4", application.Purpose },
                        { "Col5", application.CreditAmount },
                        { "Col6", application.SavingsAccountBonds },
                        { "Col7", application.PresentEmploymentSince },
                        { "Col8", application.InstallmentRate },
                        { "Col9", application.PersonalStatusAndSex },
                        { "Col10", application.OtherDebtorsGuarantors },
                        { "Col11", application.PresentResidenceSince },
                        { "Col12", application.Property },
                        { "Col13", application.AgeInYears },
                        { "Col14", application.OtherInstallmentPlans },
                        { "Col15", application.Housing },
                        { "Col16", application.NumberOfExistingCredits },
                        { "Col17", application.Job },
                        { "Col18", application.NumberOfPeopleBeingLiableFor },
                        { "Col19", application.Telephone },
                        { "Col20", application.ForeignWorker },
                         { "Col21", application.CreditRisk },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                //Encapsulate request and make it ready for posting
                ScoreRequest scoreRequest = new ScoreRequest()
                {
                    Id = "score00001",
                    Instance = scoreData
                };

                // Replace this with the API key for the web service
                const string apiKey = "Pgym78W96iDiE1LibqgcJ994bavErX83iDQiaq6EVkfPcQfTUz3dxAhc0eUrocmo1tNKczcYQpNljti/3MEvPQ==";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                //Set the Web Service address in Azure ML
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/221bddc4a92b4daf85d62426cb9270de/services/8dafe7a9b5a2427aa6d22927bc3d3f37/score");

                //Send the request as JSON to web service and get the response

               

                HttpResponseMessage response =client.PostAsJsonAsync("", scoreRequest).Result;

               

                //If response is success
                if (response.IsSuccessStatusCode)
                {
                    //Get unformatted result set from Azure ML
                    string result = response.Content.ReadAsStringAsync().Result;



                    string[] resultArray = result.Split(',');

                    if (resultArray[20] == "\"1\"")
                    {

                        application.Result = "Kredi Vermeye Uygun";
                        ViewData["CreditResult"] = application.Result;

                        ViewData["ResultText"] = "Kredi İsteği Başarılı Bir Şekilde İşleme Konuldu. İşlem Sonucu: ";

                    }
                    else
                    {
                        application.Result = "Kredi İçin Riskli";
                        ViewData["CreditResult"] = application.Result;

                        ViewData["ResultText"] = "Kredi İsteği Olumsuz. İşlem Sonucu: ";
                    }
                    //Get the result data from ML and set to model, 1/true =>  Low Credit Risk / 2/false => Hight Credit Risk
                    //application.Result = resultArray[0].Replace('\"', '1').Trim() == "1" ? "Kredi Vermeye Uygun" : "Kredi İçin Riskli";

                    //ViewData["CreditResult"] = application.Result;

                    //ViewData["ResultText"] = "Kredi İsteğiniz Başarılı Bir Şekilde İşleme Konuldu. İşlem Sonucu: ";
                }
                else
                {
                    ViewData["ResultText"] = "İşlem Başarısız, hata kodu: " + response.StatusCode;
                }
            }

            return View(application);
        }
 }

    public class ScoreData
    {
        public Dictionary<string, string> FeatureVector { get; set; }
        public Dictionary<string, string> GlobalParameters { get; set; }
    }

    public class ScoreRequest
    {
        public string Id { get; set; }
        public ScoreData Instance { get; set; }
    }
}