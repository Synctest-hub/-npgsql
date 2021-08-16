﻿using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal
{
    /// <summary>
    /// The default code generator for Npgsql.
    /// </summary>
    public class NpgsqlCodeGenerator : ProviderCodeGenerator
    {
        private static readonly MethodInfo _useNpgsqlMethodInfo
            = typeof(NpgsqlDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql),
                typeof(DbContextOptionsBuilder),
                typeof(string),
                typeof(Action<NpgsqlDbContextOptionsBuilder>));

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlCodeGenerator"/> class.
        /// </summary>
        /// <param name="dependencies">The dependencies.</param>
        public NpgsqlCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
            : base(dependencies) {}

        public override MethodCallCodeFragment GenerateUseProvider(
            string connectionString,
            MethodCallCodeFragment? providerOptions)
            => new(
                _useNpgsqlMethodInfo,
                providerOptions == null
                    ? new object[] { connectionString }
                    : new object[] { connectionString, new NestedClosureCodeFragment("x", providerOptions) });
    }
}
