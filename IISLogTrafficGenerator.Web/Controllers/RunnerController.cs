using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IISLogTrafficGenerator.Logic;
using IISLogTrafficGenerator.Logic.Logging;
using IISLogTrafficGenerator.Logic.Runners;

namespace IISLogTrafficGenerator.Web.Controllers
{
    public class RunnerController : Controller
    {
        AppPoolRunner runner = new AppPoolRunner();

        // GET: Runner
        public ActionResult Index()
        {
            var logfile = string.Concat(Session["filePath"]);
            if (!string.IsNullOrWhiteSpace(logfile))
            {
                LogReader reader = new LogReader(logfile);
                var fileContent = reader.ParseLogFile();
                ViewBag.Lines = fileContent.Count();
                return View();
            }
            
            return RedirectToRoute("/home");
        }

        public ActionResult Process()
        {
            if (Session["initialized"] == null)
            {
                var serverUrl = Request.Form["target"];

                if (string.IsNullOrWhiteSpace(serverUrl))
                {
                    serverUrl = "http://localhost/";
                }

                var pathToLog = string.Concat(Session["filePath"]);
                
                runner.StartProgram(pathToLog, serverUrl, WaitMode.LogIntervalButStartNow, true, new StringBuilderLogger());

                Session["initialized"] = true;
                Session["runner"] = runner;
            }
            else
            {
                runner = (AppPoolRunner) Session["runner"];
            }

            ViewBag.Progressing = runner.Progressing;

            ViewBag.Messages = runner.OutputPipeline.ToString();

            return View();
        }
    }
}