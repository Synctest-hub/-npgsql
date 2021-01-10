﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlComplianceTest : RelationalComplianceTestBase
    {
        protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
        {
            typeof(FromSqlSprocQueryTestBase<>),
            typeof(UdfDbFunctionTestBase<>),
            typeof(UpdateSqlGeneratorTestBase)
        };

        protected override Assembly TargetAssembly { get; } = typeof(NpgsqlComplianceTest).Assembly;
    }
}
