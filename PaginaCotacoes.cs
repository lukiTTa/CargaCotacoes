using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CargaCotacoes
{
    public class PaginaCotacoes
    {
        private IWebDriver _driver;

        public PaginaCotacoes()
        {
            try
            {
                ChromeOptions optionsFF = new ChromeOptions();
                optionsFF.AddArgument("--headless");
                optionsFF.AddArgument("--log-level=3");

                _driver = new ChromeDriver(optionsFF);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        public void CarregarPagina(string url)
        {
            try
            {
                _driver.Manage().Timeouts().PageLoad =
                TimeSpan.FromSeconds(60);
                _driver.Navigate().GoToUrl(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        public void ObterCotacoes(CotacoesContext context)
        {
            List<string> stocks = new List<string> { "itub4", "vale3", "petr3" };
            Console.WriteLine("Obtendo atualizações...");

            try
            {
                foreach (string stock in stocks)
                {
                    var cotacao = new CotacaoAcoes();

                    CarregarPagina(string.Format("https://finance.yahoo.com/quote/{0}.SA/history", stock.ToUpper()));

                    var row = _driver
                    .FindElement(By.CssSelector(@"#Col1-1-HistoricalDataTable-Proxy > section > div.Pb\(10px\).Ovx\(a\).W\(100\%\) > table > tbody > tr:nth-child(1)"))
                    .FindElements(By.TagName("td"));

                    cotacao.date = Convert.ToDateTime(row[0].Text).ToString("yyyy-MM-dd");
                    cotacao.open = row[1].Text;
                    cotacao.high = row[2].Text;
                    cotacao.low = row[3].Text;
                    cotacao.close = row[4].Text;
                    cotacao.adj_close = row[5].Text;
                    cotacao.volume = row[6].Text.Replace(",", string.Empty);

                    context.UpdateAcao(stock, cotacao);
                }

                Console.WriteLine("\n");
                Console.WriteLine("Ações atualizadas com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public double FormatValue(string value)
        {
            double selic = 13.25;
            double ipca = 0.47;

            value = value.Replace("+", String.Empty);
            value = value.Replace("%", String.Empty);
            value = value.Replace("R$", String.Empty);
            value = value.Trim();

            if (value.Contains("SELIC"))
            {
                value = value.Replace("SELIC", String.Empty);
                value = value.Trim();
                return Convert.ToDouble(value) + selic;
            }
            if (value.Contains("IPCA"))
            {
                value = value.Replace("IPCA", String.Empty);
                value = value.Trim();
                return Convert.ToDouble(value) + ipca;
            }

            return Convert.ToDouble(value);
        }

        public void ObterRendaFixa(CotacoesContext context)
        {
            List<CotacaoTesouro> cotacaoSelic = new List<CotacaoTesouro>();
            List<CotacaoTesouro> cotacaoIpca = new List<CotacaoTesouro>();
            List<CotacaoTesouro> cotacaoPrefix = new List<CotacaoTesouro>();

            try
            {
                CarregarPagina(@"https://www.tesourodireto.com.br/titulos/precos-e-taxas.htm");

                var titulos = _driver.FindElement(By.TagName("table"))
                                     .FindElements(By.ClassName("td-invest-table__card"));

                foreach (var titulo in titulos)
                {
                    var propriedades = titulo.FindElements(By.TagName("td"));

                    if (propriedades[0].FindElement(By.TagName("span")).GetAttribute("aria-label").Contains("Selic"))
                    {
                        cotacaoSelic.Add(new CotacaoTesouro()
                        {
                            title = propriedades[0].FindElement(By.TagName("span")).GetAttribute("aria-label"),
                            profit_year = FormatValue(propriedades[1].GetAttribute("textContent")),
                            min_invest = FormatValue(propriedades[2].GetAttribute("textContent")),
                            unit_price = FormatValue(propriedades[3].GetAttribute("textContent")),
                            due_date = Convert.ToDateTime(propriedades[4].GetAttribute("textContent")).ToString("yyyy-MM-dd")
                        });
                    }
                    if (propriedades[0].FindElement(By.TagName("span")).GetAttribute("aria-label").Contains("IPCA"))
                    {
                        cotacaoIpca.Add(new CotacaoTesouro()
                        {
                            title = propriedades[0].FindElement(By.TagName("span")).GetAttribute("aria-label"),
                            profit_year = FormatValue(propriedades[1].GetAttribute("textContent")),
                            min_invest = FormatValue(propriedades[2].GetAttribute("textContent")),
                            unit_price = FormatValue(propriedades[3].GetAttribute("textContent")),
                            due_date = Convert.ToDateTime(propriedades[4].GetAttribute("textContent")).ToString("yyyy-MM-dd")
                        });
                    }
                    if (propriedades[0].FindElement(By.TagName("span")).GetAttribute("aria-label").Contains("Prefixado"))
                    {
                        cotacaoPrefix.Add(new CotacaoTesouro()
                        {
                            title = propriedades[0].FindElement(By.TagName("span")).GetAttribute("aria-label"),
                            profit_year = FormatValue(propriedades[1].GetAttribute("textContent")),
                            min_invest = FormatValue(propriedades[2].GetAttribute("textContent")),
                            unit_price = FormatValue(propriedades[3].GetAttribute("textContent")),
                            due_date = Convert.ToDateTime(propriedades[4].GetAttribute("textContent")).ToString("yyyy-MM-dd")
                        });
                    }
                }

                context.UpdateTesouro("selic", cotacaoSelic);
                context.UpdateTesouro("ipca", cotacaoIpca);
                context.UpdateTesouro("prefixado", cotacaoPrefix);

                Console.WriteLine("Renda fixa atualizada com sucesso!");
                Console.WriteLine("\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            

        }

        public void Fechar()
        {
            _driver.Quit();
            _driver = null;
        }
    }
}