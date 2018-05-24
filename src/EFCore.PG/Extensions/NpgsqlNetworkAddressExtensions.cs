﻿#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System.Net;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods supporting PostgreSQL network address operator translation.
    /// </summary>
    [PublicAPI]
    public static class NpgsqlNetworkAddressExtensions
    {
        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> contains another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="ipAddress">The IP address to search.</param>
        /// <param name="other">The IP address to locate.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> contains the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Contains([CanBeNull] this DbFunctions _, IPAddress ipAddress, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) contains another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The cidr to search.</param>
        /// <param name="other">The cidr to locate.</param>
        /// <returns>
        /// True if the (IPAddress Address, int Subnet) contains the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Contains([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> contains or is equal to another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="ipAddress">The IP address to search.</param>
        /// <param name="other">The IP address to locate.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> contains or is equal to the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainsOrEqual([CanBeNull] this DbFunctions _, IPAddress ipAddress, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) contains or is equal to another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The cidr to search.</param>
        /// <param name="other">The cidr to locate.</param>
        /// <returns>
        /// True if the (IPAddress Address, int Subnet) contains or is equal to the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainsOrEqual([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> is less than another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The left-hand inet.</param>
        /// <param name="other">The right-hand inet.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> is less than the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool LessThan([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) is less than another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The left-hand cidr.</param>
        /// <param name="other">The right-hand cidr.</param>
        /// <returns>
        /// True if the (IPAddress Address, int Subnet) is less than the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool LessThan([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> is less than or equal to another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The left-hand inet.</param>
        /// <param name="other">The right-hand inet.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> is less than or equal to the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool LessThanOrEqual([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) is less than or equal to another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The left-hand cidr.</param>
        /// <param name="other">The right-hand cidr.</param>
        /// <returns>
        /// True if the (IPAddress Address, int Subnet) is less than or equal to the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool LessThanOrEqual([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> is equal to another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The left-hand inet.</param>
        /// <param name="other">The right-hand inet.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> is equal to the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Equal([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) is equal to another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The left-hand cidr.</param>
        /// <param name="other">The right-hand cidr.</param>
        /// <returns>
        /// True if the (IPAddress Address, int Subnet) is equal to the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Equal([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> is greater than or equal to another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The left-hand inet.</param>
        /// <param name="other">The right-hand inet.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> is greater than or equal to the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool GreaterThanOrEqual([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) is greater than or equal to another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The left-hand cidr.</param>
        /// <param name="other">The right-hand cidr.</param>
        /// <returns>
        /// True if the (IPAddress Address, int Subnet) is greater than or equal to the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool GreaterThanOrEqual([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> is greater than another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The left-hand inet.</param>
        /// <param name="other">The right-hand inet.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> is greater than the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool GreaterThan([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) is greater than another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The left-hand cidr.</param>
        /// <param name="other">The right-hand cidr.</param>
        /// <returns>
        /// True if the (IPAddress Address, int Subnet) is greater than the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool GreaterThan([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> is not equal to another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The left-hand inet.</param>
        /// <param name="other">The right-hand inet.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> is not equal to the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool NotEqual([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) is not equal to another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The left-hand cidr.</param>
        /// <param name="other">The right-hand cidr.</param>
        /// <returns>
        /// True if the IPAddress Address, int Subnet) is not equal to the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool NotEqual([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> is contained within another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The inet to locate.</param>
        /// <param name="other">The inet to search.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> is contained within the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainedBy([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) is contained within another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The cidr to locate.</param>
        /// <param name="other">The cidr to search.</param>
        /// <returns>
        /// True if the (IPAddress Address, int Subnet) is contained within the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainedBy([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> is contained within or equal to another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The inet to locate.</param>
        /// <param name="other">The inet to search.</param>
        /// <returns>
        /// True if the <see cref="IPAddress"/> is contained within or equal to the other <see cref="IPAddress"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainedByOrEqual([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Determines whether an (IPAddress Address, int Subnet) is contained within or equal to another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The cidr to locate.</param>
        /// <param name="other">The cidr to search.</param>
        /// <returns>
        /// True if the (IPAddress Address, int Subnet) is contained within or equal to the other (IPAddress Address, int Subnet); otherwise, false.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainedByOrEqual([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Computes the bitwise NOT operation on an <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The inet to negate.</param>
        /// <returns>
        /// The result of the bitwise NOT operation.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static IPAddress Not([CanBeNull] this DbFunctions _, IPAddress inet) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Computes the bitwise NOT operation on an (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The cidr to negate.</param>
        /// <returns>
        /// The result of the bitwise NOT operation.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static (IPAddress Address, int Subnet) Not([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Computes the bitwise AND of two <see cref="IPAddress"/> instances.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The left-hand inet.</param>
        /// <param name="other">The right-hand inet.</param>
        /// <returns>
        /// The result of the bitwise AND operation.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static IPAddress And([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Computes the bitwise AND of two (IPAddress Address, int Subnet) instances.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The left-hand cidr.</param>
        /// <param name="other">The right-hand cidr.</param>
        /// <returns>
        /// The result of the bitwise AND operation.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static (IPAddress Address, int Subnet) And([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Computes the bitwise OR of two <see cref="IPAddress"/> instances.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The left-hand inet.</param>
        /// <param name="other">The right-hand inet.</param>
        /// <returns>
        /// The result of the bitwise OR operation.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static IPAddress Or([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Computes the bitwise OR of two (IPAddress Address, int Subnet) instances.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The left-hand cidr.</param>
        /// <param name="other">The right-hand cidr.</param>
        /// <returns>
        /// The result of the bitwise OR operation.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static (IPAddress Address, int Subnet) Or([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Adds the <paramref name="value"/> to the <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The inet.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>
        /// The <see cref="IPAddress"/> augmented by the <paramref name="value"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static IPAddress Add([CanBeNull] this DbFunctions _, IPAddress inet, int value) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Adds the <paramref name="value"/> to the (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The cidr.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>
        /// The (IPAddress Address, int Subnet) augmented by the <paramref name="value"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static (IPAddress Address, int Subnet) Add([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, int value) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Subtracts the <paramref name="value"/> from the <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The inet.</param>
        /// <param name="value">The value to subtract.</param>
        /// <returns>
        /// The <see cref="IPAddress"/> augmented by the <paramref name="value"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static IPAddress Subtract([CanBeNull] this DbFunctions _, IPAddress inet, int value) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Subtracts the <paramref name="value"/> from the (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The inet.</param>
        /// <param name="value">The value to subtract.</param>
        /// <returns>
        /// The (IPAddress Address, int Subnet) augmented by the <paramref name="value"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static (IPAddress Address, int Subnet) Subtract([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, int value) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Subtracts one <see cref="IPAddress"/> from another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="inet">The inet from which to subtract.</param>
        /// <param name="other">The inet to subtract.</param>
        /// <returns>
        /// The <see cref="IPAddress"/> augmented by the <paramref name="other"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static IPAddress Subtract([CanBeNull] this DbFunctions _, IPAddress inet, IPAddress other) => throw new ClientEvaluationNotSupportedException();

        /// <summary>
        /// Subtracts one (IPAddress Address, int Subnet) from another (IPAddress Address, int Subnet).
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="cidr">The cidr from which to subtract.</param>
        /// <param name="other">The cidr to subtract.</param>
        /// <returns>
        /// The (IPAddress Address, int Subnet) augmented by the <paramref name="other"/>.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static (IPAddress Address, int Subnet) Subtract([CanBeNull] this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other) => throw new ClientEvaluationNotSupportedException();
    }
}
