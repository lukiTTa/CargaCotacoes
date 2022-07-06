using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace CargaCotacoes
{
    public class CotacoesContext
    {
        private MySqlConnection _conn;

        public CotacoesContext(IConfiguration config)
        {
            try
            {
                _conn = new MySqlConnection();
                _conn.ConnectionString = config.GetConnectionString("BaseCotacoes");
                _conn.Open();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Fechar()
        {
            _conn.Close();
        }

        public void UpdateAcao(string stock, CotacaoAcoes acao)
        {
            try
            {
                int volume = 0;

                if(acao.volume != "-")
                {
                    volume = int.Parse(acao.volume);
                }

                string query = $"INSERT into {stock} values" +
                               string.Format("('{0}',{1},{2},{3},{4},{5},{6})", 
                                             acao.date,acao.open,acao.high,acao.low,acao.close,acao.adj_close,volume);
                MySqlCommand cmd = new MySqlCommand(query, _conn);
                cmd.ExecuteNonQuery();
            }
            catch(MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                _conn.Close();
            }
        }

        public string FormatQuery(double value)
        {
            return value.ToString().Replace(',', '.');
        }

        public void UpdateTesouro(string stock, List<CotacaoTesouro> tesouros)
        {
            foreach (var tesouro in tesouros)
            {
                try
                {
                    string query = $"INSERT into tesouro_{stock} (title, profit_year, min_invest, unit_price, due_date) values" +
                                   $"('{tesouro.title}',{FormatQuery(tesouro.profit_year)},{FormatQuery(tesouro.min_invest)},{FormatQuery(tesouro.unit_price)},'{tesouro.due_date}')";
                    MySqlCommand cmd = new MySqlCommand(query, _conn);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    _conn.Close();
                }
            }
        }
    }
}