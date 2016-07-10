using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;

namespace SeleniumCSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private IWebDriver _driver;

        private void PesquisaClick(object sender, RoutedEventArgs e)
        {
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            using (_driver = new PhantomJSDriver(driverService))
            {
                _driver.Navigate().GoToUrl("http://www.bing.com");
                var query = _driver.FindElement(By.Name("q"));
                query.SendKeys(TextoPesquisa.Text);
                query.Submit();
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                wait.Until(d => ((IJavaScriptExecutor) d)
                    .ExecuteScript("return document.readyState")
                    .Equals("complete"));

                var listaResultados = ObtemResultados();
                var botaoProximo = _driver.FindElement(By.CssSelector(
                    "#b_results li.b_pag nav li a.sb_pagN"));
                botaoProximo?.Click();
                wait.Until(d => ((IJavaScriptExecutor) d)
                    .ExecuteScript("return document.readyState")
                    .Equals("complete"));
                listaResultados.AddRange(ObtemResultados());
                Resultados.ItemsSource = listaResultados;
            }
        }

        private List<Resultado> ObtemResultados()
        {
            var resultados = _driver.FindElements(By.CssSelector("#b_results li.b_algo"));
            return resultados.Select(r => new Resultado
            {
                Endereco = r.FindElement(By.CssSelector("h2 a")).GetAttribute("href"),
                Titulo = r.FindElement(By.CssSelector("h2 a")).Text,
                Descricao = r.FindElement(By.CssSelector("div.b_caption p")).Text
            }).ToList();
        }

        private void NavegaEndereco(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }

    internal class Resultado
    {
        public string Endereco { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
    }
}
