using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Upd8API.Models;
using Upd8API.Models.DTO;

namespace Upd8API.Controllers
{
    public class HomeController : Controller
    {
        private Upd8DBEntities db = new Upd8DBEntities();

        public ActionResult Index()
        {

            List<Estado> estados = new List<Estado>();
            estados = db.Estado.ToList();

            Result<List<vwClienteComLocalizacaoCompleta>> retorno = new Result<List<vwClienteComLocalizacaoCompleta>>();
            retorno.Content = db.vwClienteComLocalizacaoCompleta.OrderBy(x => x.Id).ToList();

            dynamic result = new ExpandoObject();
            result.Estados = estados;
            result.Cadastro = retorno;

            return View(result);
        }


        public ActionResult Cadastro()
        {
            List<Estado> estados = new List<Estado>();
            estados = db.Estado.ToList();

            dynamic result = new ExpandoObject();
            result.Estados = estados;

            return View(result);
        }

        public ActionResult Cadastrar(CadCliente cadastro)
        {
            Result result = new Result();

            try
            {
                if (ModelState.IsValid)
                {
                    db.CadCliente.Add(cadastro);
                    db.SaveChanges();

                    result.Mensagem = "Inserido com sucesso.";
                    result.Status = true;
                }
                else
                {
                    result.Mensagem = "Erro ao inserir o registro.";
                    result.Status = false;
                }
            }
            catch (DbException e)
            {
                result.Mensagem = e.Message;
                result.Status = false;

            }

            return Json(result);
        }

        public ActionResult Editar(int id)
        {
            List<Estado> estados = new List<Estado>();
            estados = db.Estado.ToList();

            Result<CadCliente> retorno = new Result<CadCliente>();
            retorno.Content = db.CadCliente.Where(x => x.Id == id).FirstOrDefault();

            dynamic result = new ExpandoObject();
            result.Estados = estados;
            result.Cadastro = retorno;

            return View(result);

        }


        public JsonResult EditarCadastro(CadCliente cadastro)
        {

            Result result = new Result();

            if (!ModelState.IsValid)
            {
                result.Mensagem = "Modelo invalido";
                result.Status = false;
            }

            var cadastroEncontrado = db.CadCliente.AsNoTracking().Where(x => x.Id == cadastro.Id).FirstOrDefault();
            if (cadastroEncontrado == null)
            {
                result.Mensagem = "Cliente não encontrado";
                result.Status = false;
                return Json(result);
            }

            try
            {
                cadastroEncontrado.Nome = cadastro.Nome;
                cadastroEncontrado.CPF = cadastro.CPF;
                cadastroEncontrado.DtNascimento = cadastro.DtNascimento;
                cadastroEncontrado.Sexo = cadastro.Sexo;
                cadastroEncontrado.Endereco = cadastro.Endereco;
                cadastroEncontrado.EstadoId = cadastro.EstadoId;
                cadastroEncontrado.MunicipioId = cadastro.MunicipioId;

                db.Entry(cadastroEncontrado).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                result.Mensagem = ex.Message;
                result.Status = false;
                return Json(result);
            }

            result.Mensagem = "Cadastro alterado com sucesso.";
            result.Status = true;
            return Json(result);
        }


        public JsonResult Excluir(int id)
        {
            Result result = new Result();

            CadCliente cliente = db.CadCliente.Where(x => x.Id == id).FirstOrDefault();

            if (cliente == null)
            {
                result.Mensagem = "Cliente não encontrado";
                result.Status = false;
                return Json(result);
            }

            try
            {
                db.CadCliente.Remove(cliente);
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                result.Mensagem = ex.Message;
                result.Status = false;
                return Json(result);
            }

            result.Mensagem = "Cadastro excluido com sucesso.";
            result.Status = true;

            return Json(result);
        }


        [HttpPost]
        public JsonResult Pesquisar(CadCliente cliente)
        {

            /*  PESQUISA DINAMICA: 
             *  Usa o nome e tipo da propriedade do objeto para montar o filtro da consulta.
             *  Levando em consideração usar um objeto do Entity para reaproveitar o nome dos campos da tabela.
             */

            
            string filtro = " ";

            foreach (var item in cliente.GetType().GetProperties())
            {
                //Selecionar somente as propriedades que possui valor "não default".
                if (item.GetValue(cliente) != null && item.GetValue(cliente).ToString() != "0")
                {
                    //Filtra os tipos que contem Nullable junto a propriedade
                    var propriedade = item.PropertyType;

                    if (propriedade.IsGenericType && propriedade.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propriedade = propriedade.GetGenericArguments()[0];
                    }

                    //Constroi a consulta comforme o tipo encontrado
                    switch (propriedade.Name)
                    {
                        case "String":
                            filtro = filtro + (item.Name + " Like '%" + item.GetValue(cliente).ToString() + "%' and ");
                            break;

                        case "DateTime":
                            DateTime dt = Convert.ToDateTime(item.GetValue(cliente));
                            filtro = filtro + ("convert(date," + item.Name + ",103) = '" + dt.ToString("yyyy-MM-dd") + "' and ");
                            break;
                        
                        default:
                            filtro = filtro + (item.Name + " = " + item.GetValue(cliente).ToString() + " and ");
                            break;
                    }
                }
            }

            //Retira o ultimo "and"
            filtro = filtro.Substring(0, (filtro.Length - 5));
            
            string cmd = string.Format("SELECT * FROM vwClienteComLocalizacaoCompleta Where {0}", filtro);

            Result<List<vwClienteComLocalizacaoCompleta>> result = new Result<List<vwClienteComLocalizacaoCompleta>>();

            result.Content = db.Database.SqlQuery<vwClienteComLocalizacaoCompleta>(cmd).ToList();
            result.Status = true;

            return Json(result);
        }


        [HttpPost]
        public JsonResult ObterCidadesPorEstado(string UF)
        {
            Result<List<Municipio>> result = new Result<List<Municipio>>();
            result.Content = db.Municipio.Where(x => x.Uf.Contains(UF)).ToList();
            result.Status = true;

            return Json(result);
        }



    }
}
