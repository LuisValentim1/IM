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

        public Dictionary<string, string> cards = null;
        Random r = null;
        string[] repeat = {"Desculpe, não percebi, pode repetir?", "Não o consegui ouvir, pode repetir por favor?", "Poderia repetir se faz favor? Não percebi bem" };
        string[] turn = { "Ainda não é o seu turno, só um momento.", "Espere um pouco se faz favor, ainda não é o seu turno.", "O seu turno está quase a chegar, só um segundo." };
        string[] afirmative = {"Já está.", "Ok.", "Feito."};
        public void SetUp()
        {
            driver = new ChromeDriver(@"C:\Program Files\chromedriver\");
            js = (IJavaScriptExecutor)driver;
            vars = new Dictionary<string, object>();
            r = new Random(DateTime.Now.Day);
            cards = new Dictionary<string, string>();
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
                sendJson(repeat[r.Next(0, 2)]);
            }
            else
            {

                switch ((string)json.recognized[0].ToString())
                {
                    case "START":
                        if (driver.FindElement(By.Id("new-player")).Displayed)
                        {
                            driver.FindElement(By.Id("new-player")).Click();
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
                        if (driver.FindElements(By.CssSelector(".control-button.inactive")).Count == 0)
                        {
                            driver.FindElement(By.Id("Call")).Click();
                            sendJson(afirmative[r.Next(0, 2)]);
                        }
                        else
                        {
                            sendJson(turn[r.Next(0,2)]);
                        }
                        break;
                    case "CHECK":
                        if (driver.FindElement(By.Id("Check")).Displayed)
                        {
                            driver.FindElement(By.Id("Check")).Click();
                            sendJson(afirmative[r.Next(0, 2)]);
                        }
                        else
                        {
                            sendJson("Não pode passar, tem que pagar, subir ou sair.");
                        }
                        break;
                    case "FOLD":
                        if (driver.FindElements(By.CssSelector(".control-button.inactive")).Count == 0)
                        {
                            driver.FindElement(By.Id("Fold")).Click();
                            sendJson(afirmative[r.Next(0, 2)]);
                        }
                        else
                        {
                            sendJson(turn[r.Next(0,2)]);
                        }
                        break;
                    case "HAND":
                        if(driver.FindElement(By.Id("seat2")).GetAttribute("class") != "player folded") {
                            List<string> hand = new List<string>();
                            var cardImage1 = driver.FindElement(By.CssSelector("#seat2 > .card1 > img"));
                            String card1 = cardImage1.GetAttribute("src").Substring(36, 2);

                            var cardImage2 = driver.FindElement(By.CssSelector("#seat2 > .card2 > img"));
                            String card2 = cardImage2.GetAttribute("src").Substring(36, 2);

                            hand.Add(card1);
                            hand.Add(card2);

                            String message = "Esta ronda tem na mão, ";
                            message = cardMessageFormating(hand, message);
                            sendJson(message);
                        }
                        else
                        {
                            sendJson("Está fora desta jogadas.");
                        }
                        break;

                    case "QUALITY":

                        int quality = 0;
                        string stylePercentage = driver.FindElement(By.Id("background")).GetAttribute("style").Substring(21,3);
                        if(stylePercentage == "100")
                        {
                            sendJson("A sua mão é muito má.");
                        }
                        else
                        {
                            quality = 100 - Int16.Parse(stylePercentage.Substring(0, 2));
                            qualityMessage(quality);
                        }
                        break;

                    case "RAISE":
                        if (driver.FindElements(By.CssSelector(".control-button.inactive")).Count == 0)
                        {
                            driver.FindElement(By.Id("RaiseAmount")).Clear();
                            driver.FindElement(By.Id("RaiseAmount")).SendKeys((string)json.recognized[1].ToString());
                            driver.FindElement(By.Id("Raise")).Click();
                            sendJson(afirmative[r.Next(0, 2)]);
                        }
                        else
                        {
                            sendJson(turn[r.Next(0,2)]);
                        }
                        break;

                    case "QUANTITY":
                        if (driver.FindElements(By.CssSelector(".menu-button.open")).Count == 1)
                        {
                            driver.FindElement(By.Id("InitialChips")).Clear();
                            driver.FindElement(By.Id("InitialChips")).SendKeys((string)json.recognized[2].ToString());

                            driver.FindElement(By.Id("playerCount")).Clear();
                            driver.FindElement(By.Id("playerCount")).SendKeys((string)json.recognized[1].ToString());

                            driver.FindElement(By.Id("playnewgame")).Click();
                        }
                        break;

                    case "CARDS":
                        string cartasMensagem = "Ainda não existem cartas na mesa.";
                        List<String> cartasMesa = new List<String>();

                        var flop1 = driver.FindElement(By.CssSelector("#flop1 > img"));
                        String flopActive = flop1.GetAttribute("style").Substring(12, 6);
                        if (flopActive == "hidden")
                        {
                            sendJson(cartasMensagem);
                        }
                        else
                        {
                            cartasMensagem = "Atualmente estão na mesa, ";
                            var flop2 = driver.FindElement(By.CssSelector("#flop2 > img"));
                            var flop3 = driver.FindElement(By.CssSelector("#flop3 > img"));
                            String flopCard1 = flop1.GetAttribute("src").Substring(36, 2);
                            String flopCard2 = flop2.GetAttribute("src").Substring(36, 2);
                            String flopCard3 = flop3.GetAttribute("src").Substring(36, 2);
                            cartasMesa.Add(flopCard1);
                            cartasMesa.Add(flopCard2);
                            cartasMesa.Add(flopCard3);

                            var turn = driver.FindElement(By.CssSelector("#turn > img"));
                            String turnActive = turn.GetAttribute("style").Substring(12, 6);

                            if (turnActive != "hidden")
                            {
                                String turnCard = turn.GetAttribute("src").Substring(36, 2);
                                cartasMesa.Add(turnCard);

                                var river = driver.FindElement(By.CssSelector("#river > img"));
                                String riverActive = river.GetAttribute("style").Substring(12, 6);

                                if (riverActive != "hidden")
                                {
                                    String riverCard = river.GetAttribute("src").Substring(36, 2);
                                    cartasMesa.Add(riverCard);
                                    cartasMensagem = cardMessageFormating(cartasMesa, cartasMensagem);
                                    sendJson(cartasMensagem);
                                }
                                else
                                {
                                    cartasMensagem = cardMessageFormating(cartasMesa, cartasMensagem);
                                    sendJson(cartasMensagem);
                                }
                            }
                            else
                            {
                                cartasMensagem = cardMessageFormating(cartasMesa, cartasMensagem);
                                sendJson(cartasMensagem);
                            }
                        }
                        break;
                }
            }
        }

        public string cardMessageFormating(List<string> cardsToAdd, string mensagem)
        {
            foreach(string card in cardsToAdd)
            {
                if (card != cardsToAdd[cardsToAdd.Count - 1])
                {
                    if (card.Substring(0, 1) == "Q")
                    {
                        mensagem += "uma ";
                    }
                    else
                    {
                        mensagem += "um ";
                    }
                    mensagem += cards[card] + " ,";
                }
                else
                {
                    if (card.Substring(0, 1) == "Q")
                    {
                        mensagem += "e uma ";
                    }
                    else
                    {
                        mensagem += "e um ";
                    }
                    mensagem += cards[card] + " .";
                }
            }
            return mensagem;
        }

        public void qualityMessage(int quality)
        {
            if(quality > 25)
            {
                if(quality > 50)
                {
                    if(quality > 75)
                    {
                        sendJson("A sua mão é muito boa!");
                    }
                    else
                    {
                        sendJson("Tem uma boa mão.");
                    }
                }
                else
                {
                    sendJson("A sua mão não é muito boa mas pode testar a sua sorte.");
                }
            }
            else
            {
                sendJson("A sua mão não é lá muito boa, cuidado.");
            }
        }

        public void sendJson(String content)
        {
            mmic.Send(lce.NewContextRequest());

            string json2 = "";
            json2 += content;
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);
        }

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
            cards.Add("3S", "três de espadas");
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
            cards.Add("3H", "três de copas");
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
            cards.Add("3D", "três de ouros");
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
            cards.Add("3C", "três de paus");
            cards.Add("2C", "dois de paus");
            cards.Add("AC", "ás de paus");
        }
    }
}
