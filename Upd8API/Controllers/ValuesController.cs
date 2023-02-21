using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using Upd8API.Models;
using Upd8API.Models.DTO;
using System.Data.Common;

namespace Upd8API.Controllers
{
    public class ValuesController : ApiController
    {
        private Upd8DBEntities db = new Upd8DBEntities();

        // GET api/values
        //Retorna todos os dados cadastrados
        public JsonResult<Result<List<CadCliente>>> Get()
        {
            Result<List<CadCliente>> result = new Result<List<CadCliente>>();

            result.Content = db.CadCliente.OrderBy(x => x.Id).ToList();

            return Json(result);

        }

        // GET api/values/5
       //Retorna cadastro do id selecionado
        public JsonResult<Result<CadCliente>> Get(int id)
        {
            Result<CadCliente> result = new Result<CadCliente>();

            result.Content = db.CadCliente.Where(x => x.Id == id).FirstOrDefault();

            if (result.Content == null)
            {
                result.Mensagem = "Cliente não encontrado";
                result.Status = false;
            }

            return Json(result);
        }

        // GET api/values
        //Retorna os cadastros com pesquisa dinamica
        public JsonResult<Result<List<CadCliente>>> Get(CadCliente cliente)
        {

            string filtro = " ";

            foreach (var item in cliente.GetType().GetProperties())
            {
                if (item.GetValue(cliente) != null && item.GetValue(cliente).ToString() != "0")
                {
                    var propriedade = item.PropertyType;

                    if (propriedade.IsGenericType && propriedade.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propriedade = propriedade.GetGenericArguments()[0];
                    }

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

            filtro = filtro.Substring(0, (filtro.Length - 5));

            string cmd = string.Format("SELECT * FROM CadCliente Where {0}", filtro);

            Result<List<CadCliente>> result = new Result<List<CadCliente>>();

            result.Content = db.Database.SqlQuery<CadCliente>(cmd).ToList();
            result.Status = true;

            return Json(result);
        }



        // POST api/values
        //Insere um novo cadastro
        public JsonResult<Result> Post(CadCliente cliente)
        {
            Result result = new Result();

            try
            {
                if (ModelState.IsValid)
                {
                    db.CadCliente.Add(cliente);
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

        // PUT api/values/5
        //Altera o cadastro
        public JsonResult<Result> Put(CadCliente cliente)
        {
            Result result = new Result();

            if (!ModelState.IsValid)
            {
                result.Mensagem = "Modelo invalido";
                result.Status = false;
            }

            var cadastroEncontrado = db.CadCliente.AsNoTracking().Where(x => x.Id == cliente.Id).FirstOrDefault();
            if (cadastroEncontrado == null)
            {
                result.Mensagem = "Cliente não encontrado";
                result.Status = false;
                return Json(result);
            }

            try
            {
                cadastroEncontrado.Nome = cliente.Nome;
                cadastroEncontrado.CPF = cliente.CPF;
                cadastroEncontrado.DtNascimento = cliente.DtNascimento;
                cadastroEncontrado.Sexo = cliente.Sexo;
                cadastroEncontrado.Endereco = cliente.Endereco;
                cadastroEncontrado.EstadoId = cliente.EstadoId;
                cadastroEncontrado.MunicipioId = cliente.MunicipioId;

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

        // DELETE api/values/5
        //Exclui o cadastro
        public JsonResult<Result> Delete(int id)
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
    }
}
