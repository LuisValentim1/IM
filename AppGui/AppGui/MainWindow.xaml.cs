using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;

        //  new 16 april 2020
        private MmiCommunication mmiSender;
        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        private IWebDriver driver;
        public IDictionary<string, object> vars { get; private set; }
        private IJavaScriptExecutor js;

        public void SetUp()
        {
            driver = new ChromeDriver(@"C:\Program Files\chromedriver\");
            js = (IJavaScriptExecutor)driver;
            vars = new Dictionary<string, object>();

            driver.Navigate().GoToUrl("http://lyncz.co.uk/poker-game/");
        }

        protected void TearDown()
        {
            driver.Quit();
        }
        public MainWindow()
        {
            //InitializeComponent();

            SetUp();

            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            // NEW 16 april 2020
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");
            

        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            Shape _s = null;
            Console.WriteLine((string)json.recognized[0].ToString());
            switch ((string)json.recognized[0].ToString())
            {
                case "START":
                    driver.FindElement(By.Id("new-player")).Click();
                    break;
                case "END":
                    driver.FindElement(By.Id("endgame")).Click();
                    break;
                case "RESTART":
                    driver.FindElement(By.Id("endgame")).Click();
                    driver.FindElement(By.CssSelector(".button-copy")).Click();
                    driver.FindElement(By.CssSelector(".menu-button")).Click();
                    driver.FindElement(By.Id("initialChips")).SendKeys("80");
                    driver.FindElement(By.Id("playerCount")).Click();
                    driver.FindElement(By.Id("playerCount")).SendKeys("5");
                    driver.FindElement(By.Id("playnewgame")).Click();
                    break;
                case "CALL":
                    driver.FindElement(By.Id("Call")).Click();
                    break;
                case "CHECK":
                    driver.FindElement(By.Id("Check")).Click();
                    break;
                case "FOLD":
                    driver.FindElement(By.Id("Fold")).Click();
                    break;
               /** case "HAND":
                    IWebElement img = driver.FindElement(By.ClassName("card1"));
                    IWebElement img2 = driver.FindElement(By.ClassName("card2"));
                    String src = img.GetAttribute("src").Substring(36, 38);
                    String src2 = img2.GetAttribute("src").Substring(36, 38);

                    break;
                case "QAULITY":
                    IWebElement meter = driver.FindElement(By.Id("background"));
                    String stylePercentage = meter.GetAttribute("style").Substring(22,26);
                    if(stylePercentage[2] != '0')
                    {
                        stylePercentage = stylePercentage.Substring(0, 2);
                    }
                    break;**/
                case "RAISE":
                    driver.FindElement(By.Id("RaiseAmount")).Clear();
                    driver.FindElement(By.Id("RaiseAmount")).SendKeys((string)json.recognized[1].ToString());
                    driver.FindElement(By.Id("Raise")).Click();
                    break;
            }


            /*
            //  new 16 april 2020
            mmic.Send(lce.NewContextRequest());

            string json2 = "{ \"synthesize\": ["; // "{ \"synthesize\": [";
            //json2 += (string)json.recognized[0].ToString()+ " ";
            json2 += (string)json.recognized[1].ToString() + " DONE." ;
            //json2 += "] }";
            /*
             foreach (var resultSemantic in e.Result.Semantics)
            {
                json += "\"" + resultSemantic.Value.Value + "\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);
            */
        }

        public void sendJson(String content)
        {
            mmic.Send(lce.NewContextRequest());

            string json2 = "{ \"synthesize\": ["; // "{ \"synthesize\": [";
            //json2 += (string)json.recognized[0].ToString()+ " ";
            json2 += content + " DONE.";
            //json2 += "] }";
            /*
             foreach (var resultSemantic in e.Result.Semantics)
            {
                json += "\"" + resultSemantic.Value.Value + "\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            */
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);
        }
    }
}