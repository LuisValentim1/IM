using System;
using System.Linq;
using System.Windows;
using System.Collections;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;

using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using static OpenQA.Selenium.Remote.RemoteWebElement;

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

        public Dictionary<string, string> cards = new Dictionary<string, string>();
        public void populateDictionary()
        {
            cards.Add("KS", "rei de espadas");
            cards.Add("QS", "dama de espadas");
            cards.Add("JS", "valete de espadas");
            cards.Add("TS", "dez de espadas");
            cards.Add("9S", "nove de espadas");
            cards.Add("8S", "oito de espadas");
            cards.Add("7S", "sete de espadas");
            cards.Add("6S", "seis de espadas");
            cards.Add("5S", "cinco de espadas");
            cards.Add("4S", "quatro de espadas");
            cards.Add("3S", "tres de espadas");
            cards.Add("2S", "dois de espadas");
            cards.Add("AS", "ás de espadas");
            cards.Add("KH", "rei de copas");
            cards.Add("QH", "dama de copas");
            cards.Add("JH", "valete de copas");
            cards.Add("TH", "dez de copas");
            cards.Add("9H", "nove de copas");
            cards.Add("8H", "oito de copas");
            cards.Add("7H", "sete de copas");
            cards.Add("6H", "seis de copas");
            cards.Add("5H", "cinco de copas");
            cards.Add("4H", "quatro de copas");
            cards.Add("3H", "tres de copas");
            cards.Add("2H", "dois de copas");
            cards.Add("AH", "ás de copas");
            cards.Add("KD", "rei de ouros");
            cards.Add("QD", "dama de ouros");
            cards.Add("JD", "valete de ouros");
            cards.Add("TD", "dez de ouros");
            cards.Add("9D", "nove de ouros");
            cards.Add("8D", "oito de ouros");
            cards.Add("7D", "sete de ouros");
            cards.Add("6D", "seis de ouros");
            cards.Add("5D", "cinco de ouros");
            cards.Add("4D", "quatro de ouros");
            cards.Add("3D", "tres de ouros");
            cards.Add("2D", "dois de ouros");
            cards.Add("AD", "ás de ouros");
            cards.Add("KC", "rei de paus");
            cards.Add("QC", "dama de paus");
            cards.Add("JC", "valete de paus");
            cards.Add("TC", "dez de paus");
            cards.Add("9C", "nove de paus");
            cards.Add("8C", "oito de paus");
            cards.Add("7C", "sete de paus");
            cards.Add("6C", "seis de paus");
            cards.Add("5C", "cinco de paus");
            cards.Add("4C", "quatro de paus");
            cards.Add("3C", "tres de paus");
            cards.Add("2C", "dois de paus");
            cards.Add("AC", "ás de paus");
        }
        public void SetUp()
        {
            driver = new ChromeDriver(@"C:\Program Files\chromedriver\");
            js = (IJavaScriptExecutor)driver;
            vars = new Dictionary<string, object>();
            populateDictionary();

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
            if (float.Parse(json.confidence[0].ToString()) < 0.7)
            {
                sendJson("Desculpe podia repetir?");
            }
            else
            {

                switch ((string)json.recognized[0].ToString())
                {
                    case "START":
                        if (driver.FindElement(By.Id("new-player")).Displayed)
                        {
                            driver.FindElement(By.Id("new-player")).Click();
                            sendJson("aight.");
                        }
                        break;
                    case "END":
                        driver.FindElement(By.Id("endgame")).Click();
                        break;
                    case "RESTART":
                        driver.FindElement(By.Id("endgame")).Click();
                        driver.FindElement(By.CssSelector(".button-copy")).Click();
                        driver.FindElement(By.CssSelector(".menu-button")).Click();
                        sendJson("Com quantos fichas e quantos jogadores gostaria de jogar?");

                        /*driver.FindElement(By.Id("initialChips")).SendKeys("80");
                        driver.FindElement(By.Id("playerCount")).Click();
                        driver.FindElement(By.Id("playerCount")).SendKeys("5");
                        driver.FindElement(By.Id("playnewgame")).Click(); */

                        break;
                    case "CALL":
                        if (driver.FindElement(By.Id("Call")).Displayed)
                        {
                            driver.FindElement(By.Id("Call")).Click();
                        }
                        break;
                    case "CHECK":
                        if (driver.FindElement(By.Id("Check")).Displayed)
                        {
                            driver.FindElement(By.Id("Check")).Click();
                        }
                        break;
                    case "FOLD":
                        driver.FindElement(By.Id("Fold")).Click();
                        break;
                    case "HAND":
                        var cardImage1 = driver.FindElement(By.CssSelector("#seat2 > .card1 > img"));
                        String card1 = cardImage1.GetAttribute("src").Substring(36, 2);

                        var cardImage2 = driver.FindElement(By.CssSelector("#seat2 > .card2 > img"));
                        String card2 = cardImage2.GetAttribute("src").Substring(36, 2);

                        String message = cards[card1] + " e " + cards[card2] + "são as suas cartas esta ronda.";
                        sendJson(message);
                        break;

                    /** 
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

            string json2 = "";
            //string json2 = "{ \"synthesize\": ["; // "{ \"synthesize\": [";
            //json2 += (string)json.recognized[0].ToString()+ " ";
            json2 += content;
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
