﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ReGenerateReport.Api.Helper
{
    public class HttpStatus
    {
        public static int NotFound => (int)HttpStatusCode.NotFound;
        public static int OK => (int)HttpStatusCode.OK;
        public static int BadRequest => (int)HttpStatusCode.BadRequest;
        public static int InternalServerError => (int)HttpStatusCode.InternalServerError;
        public static int Unauthorized => (int)HttpStatusCode.Unauthorized;
    }
}
