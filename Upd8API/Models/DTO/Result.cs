using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Upd8API.Models.DTO
{
    public class Result
    {
        //
        // Summary:
        //     Mensagem de retorno
        public string Mensagem { get; set; }

        //
        // Summary:
        //     Status da requisição
        public bool Status { get; set; }
    }

    public class Result<T> : Result
    {
        public T Content { get; set; }
    }

}