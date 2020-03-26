﻿using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Npgsql specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class NpgsqlPropertyBuilderExtensions
    {
        #region HiLo

        /// <summary>
        /// Configures the property to use a sequence-based hi-lo pattern to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The comment of the sequence.</param>
        /// <param name="schema"> The schema of the sequence.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var property = propertyBuilder.Metadata;

            name ??= NpgsqlModelExtensions.DefaultHiLoSequenceName;

            var model = property.DeclaringEntityType.Model;

            if (model.FindSequence(name, schema) == null)
            {
                model.AddSequence(name, schema).IncrementBy = 10;
            }

            property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);
            property.SetHiLoSequenceName(name);
            property.SetHiLoSequenceSchema(schema);
            property.RemoveIdentityOptions();

            return propertyBuilder;
        }

        /// <summary>
        /// Configures the property to use a sequence-based hi-lo pattern to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The comment of the sequence.</param>
        /// <param name="schema"> The schema of the sequence.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => (PropertyBuilder<TProperty>)UseHiLo((PropertyBuilder)propertyBuilder, name, schema);

        /// <summary>
        /// Configures the database sequence used for the hi-lo pattern to generate values for the key property,
        /// when targeting SQL Server.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The name of the sequence.</param>
        /// <param name="schema">The schema of the sequence.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the sequence.</returns>
        public static IConventionSequenceBuilder HasHiLoSequence(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetHiLoSequence(name, schema))
            {
                return null;
            }

            propertyBuilder.Metadata.SetHiLoSequenceName(name, fromDataAnnotation);
            propertyBuilder.Metadata.SetHiLoSequenceSchema(schema, fromDataAnnotation);

            return name == null
                ? null
                : propertyBuilder.Metadata.DeclaringEntityType.Model.Builder.HasSequence(name, schema, fromDataAnnotation);
        }

        /// <summary>
        /// Returns a value indicating whether the given name and schema can be set for the hi-lo sequence.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The name of the sequence.</param>
        /// <param name="schema">The schema of the sequence.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the given name and schema can be set for the hi-lo sequence.</returns>
        public static bool CanSetHiLoSequence(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return propertyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)
                   && propertyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema, schema, fromDataAnnotation);
        }

        #endregion HiLo

        #region Serial

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <para>
        /// This option should be considered deprecated starting with PostgreSQL 10, consider using <see cref="UseIdentityColumn"/> instead.
        /// </para>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseSerialColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn);
            property.SetHiLoSequenceName(null);
            property.SetHiLoSequenceSchema(null);
            property.RemoveHiLoOptions();
            property.RemoveIdentityOptions();

            return propertyBuilder;
        }

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <para>
        /// This option should be considered deprecated starting with PostgreSQL 10, consider using <see cref="UseIdentityColumn"/> instead.
        /// </para>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseSerialColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseSerialColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <para>
        /// This option should be considered deprecated starting with PostgreSQL 10, consider using <see cref="UseIdentityColumn"/> instead.
        /// </para>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static IConventionPropertyBuilder UseSerialColumn(
            [NotNull] this IConventionPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.CanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn))
            {
                var property = propertyBuilder.Metadata;
                property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn);
                property.SetHiLoSequenceName(null);
                property.SetHiLoSequenceSchema(null);
                property.RemoveHiLoOptions();
                property.RemoveIdentityOptions();

                return propertyBuilder;
            }

            return null;
        }

        #endregion Serial

        #region Identity always

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will always be generated as identity, and the application will not be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseIdentityAlwaysColumn([NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
            property.SetHiLoSequenceName(null);
            property.SetHiLoSequenceSchema(null);

            return propertyBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will always be generated as identity, and the application will not be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseIdentityAlwaysColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseIdentityAlwaysColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will always be generated as identity, and the application will not be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static IConventionPropertyBuilder UseIdentityAlwaysColumn([NotNull] this IConventionPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.CanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn))
            {
                var property = propertyBuilder.Metadata;
                property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
                property.SetHiLoSequenceName(null);
                property.SetHiLoSequenceSchema(null);

                return propertyBuilder;
            }

            return null;
        }

        #endregion Identity always

        #region Identity by default

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseIdentityByDefaultColumn([NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            property.SetHiLoSequenceName(null);
            property.SetHiLoSequenceSchema(null);

            return propertyBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseIdentityByDefaultColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseIdentityByDefaultColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static IConventionPropertyBuilder UseIdentityByDefaultColumn([NotNull] this IConventionPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.CanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn))
            {
                var property = propertyBuilder.Metadata;
                property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
                property.SetHiLoSequenceName(null);
                property.SetHiLoSequenceSchema(null);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>
        /// This internally calls <see cref="UseIdentityByDefaultColumn(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder)"/>.
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.UseIdentityByDefaultColumn();

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>
        /// This internally calls <see cref="UseIdentityByDefaultColumn(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder)"/>.
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.UseIdentityByDefaultColumn();

        #endregion Identity by default

        #region General value generation strategy

        /// <summary>
        /// Configures the value generation strategy for the key property, when targeting PostgreSQL.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="valueGenerationStrategy">The value generation strategy.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        /// The same builder instance if the configuration was applied, <c>null</c> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasValueGenerationStrategy(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            NpgsqlValueGenerationStrategy? valueGenerationStrategy,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetAnnotation(
                NpgsqlAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
                if (valueGenerationStrategy != NpgsqlValueGenerationStrategy.SequenceHiLo)
                {
                    propertyBuilder.HasHiLoSequence(null, null, fromDataAnnotation);
                }

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the given value can be set as the value generation strategy.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="valueGenerationStrategy">The value generation strategy.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the given value can be set as the default value generation strategy.</returns>
        public static bool CanSetValueGenerationStrategy(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            NpgsqlValueGenerationStrategy? valueGenerationStrategy,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return (valueGenerationStrategy == null
                    || NpgsqlPropertyExtensions.IsCompatibleWithValueGeneration(propertyBuilder.Metadata))
                   && propertyBuilder.CanSetAnnotation(
                       NpgsqlAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation);
        }

        #endregion General value generation strategy

        #region Identity options

        /// <summary>
        /// Sets the sequence options on an identity column.
        /// The column must be set as identity via <see cref="UseIdentityColumn(PropertyBuilder)"/> or
        /// <see cref="UseIdentityAlwaysColumn(PropertyBuilder)"/>.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="startValue">
        /// The starting value for the sequence.
        /// The default starting value is <see cref="minValue"/> for ascending sequences and <see cref="maxValue"/> for descending ones.
        /// </param>
        /// <param name="incrementBy">The amount to increment between values. Defaults to 1.</param>
        /// <param name="minValue">
        /// The minimum value for the sequence.
        /// The default for an ascending sequence is 1. The default for a descending sequence is the minimum value of the data type.
        /// </param>
        /// <param name="maxValue">
        /// The maximum value for the sequence.
        /// The default for an ascending sequence is the maximum value of the data type. The default for a descending sequence is -1.
        /// </param>
        /// <param name="isCyclic">
        /// Sets whether or not the sequence will start again from the beginning once the maximum value is reached.
        /// Defaults to false.
        /// </param>
        /// <param name="numbersToCache">
        /// Specifies how many sequence numbers are to be preallocated and stored in memory for faster access.
        /// The minimum value is 1 (only one value can be generated at a time, i.e., no cache), and this is also the default.
        /// </param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder HasIdentityOptions(
            [NotNull] this PropertyBuilder propertyBuilder,
            long? startValue = null,
            long? incrementBy = null,
            long? minValue = null,
            long? maxValue = null,
            bool? isCyclic = null,
            long? numbersToCache = null)
        {
            var property = propertyBuilder.Metadata;
            property.SetIdentityStartValue(startValue);
            property.SetIdentityIncrementBy(incrementBy);
            property.SetIdentityMinValue(minValue);
            property.SetIdentityMaxValue(maxValue);
            property.SetIdentityIsCyclic(isCyclic);
            property.SetIdentityNumbersToCache(numbersToCache);
            return propertyBuilder;
        }

        /// <summary>
        /// Sets the sequence options on an identity column.
        /// The column must be set as identity via <see cref="UseIdentityColumn(PropertyBuilder)"/> or
        /// <see cref="UseIdentityAlwaysColumn(PropertyBuilder)"/>.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="startValue">
        /// The starting value for the sequence.
        /// The default starting value is <see cref="minValue"/> for ascending sequences and <see cref="maxValue"/> for descending ones.
        /// </param>
        /// <param name="incrementBy">The amount to increment between values. Defaults to 1.</param>
        /// <param name="minValue">
        /// The minimum value for the sequence.
        /// The default for an ascending sequence is 1. The default for a descending sequence is the minimum value of the data type.
        /// </param>
        /// <param name="maxValue">
        /// The maximum value for the sequence.
        /// The default for an ascending sequence is the maximum value of the data type. The default for a descending sequence is -1.
        /// </param>
        /// <param name="isCyclic">
        /// Sets whether or not the sequence will start again from the beginning once the maximum value is reached.
        /// Defaults to false.
        /// </param>
        /// <param name="numbersToCache">
        /// Specifies how many sequence numbers are to be preallocated and stored in memory for faster access.
        /// The minimum value is 1 (only one value can be generated at a time, i.e., no cache), and this is also the default.
        /// </param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> HasIdentityOptions<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            long? startValue = null,
            long? incrementBy = null,
            long? minValue = null,
            long? maxValue = null,
            bool? isCyclic = null,
            long? numbersToCache = null)
            => (PropertyBuilder<TProperty>)HasIdentityOptions(
                (PropertyBuilder)propertyBuilder, startValue, incrementBy, minValue, maxValue, isCyclic, numbersToCache);

        /// <summary>
        /// Sets the sequence options on an identity column.
        /// The column must be set as identity via <see cref="UseIdentityColumn(PropertyBuilder)"/> or
        /// <see cref="UseIdentityAlwaysColumn(PropertyBuilder)"/>.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="startValue">
        /// The starting value for the sequence.
        /// The default starting value is <see cref="minValue"/> for ascending sequences and <see cref="maxValue"/> for descending ones.
        /// </param>
        /// <param name="incrementBy">The amount to increment between values. Defaults to 1.</param>
        /// <param name="minValue">
        /// The minimum value for the sequence.
        /// The default for an ascending sequence is 1. The default for a descending sequence is the minimum value of the data type.
        /// </param>
        /// <param name="maxValue">
        /// The maximum value for the sequence.
        /// The default for an ascending sequence is the maximum value of the data type. The default for a descending sequence is -1.
        /// </param>
        /// <param name="isCyclic">
        /// Sets whether or not the sequence will start again from the beginning once the maximum value is reached.
        /// Defaults to false.
        /// </param>
        /// <param name="numbersToCache">
        /// Specifies how many sequence numbers are to be preallocated and stored in memory for faster access.
        /// The minimum value is 1 (only one value can be generated at a time, i.e., no cache), and this is also the default.
        /// </param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static IConventionPropertyBuilder HasIdentityOptions(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            long? startValue = null,
            long? incrementBy = null,
            long? minValue = null,
            long? maxValue = null,
            bool? isCyclic = null,
            long? numbersToCache = null)
        {
            if (propertyBuilder.CanSetIdentityOptions(startValue, incrementBy, minValue, maxValue, isCyclic, numbersToCache))
            {
                var property = propertyBuilder.Metadata;
                property.SetIdentityStartValue(startValue);
                property.SetIdentityIncrementBy(incrementBy);
                property.SetIdentityMinValue(minValue);
                property.SetIdentityMaxValue(maxValue);
                property.SetIdentityIsCyclic(isCyclic);
                property.SetIdentityNumbersToCache(numbersToCache);
                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the sequence options can be set on the identity column.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="startValue">
        /// The starting value for the sequence.
        /// The default starting value is <see cref="minValue"/> for ascending sequences and <see cref="maxValue"/> for descending ones.
        /// </param>
        /// <param name="incrementBy">The amount to increment between values. Defaults to 1.</param>
        /// <param name="minValue">
        /// The minimum value for the sequence.
        /// The default for an ascending sequence is 1. The default for a descending sequence is the minimum value of the data type.
        /// </param>
        /// <param name="maxValue">
        /// The maximum value for the sequence.
        /// The default for an ascending sequence is the maximum value of the data type. The default for a descending sequence is -1.
        /// </param>
        /// <param name="isCyclic">
        /// Sets whether or not the sequence will start again from the beginning once the maximum value is reached.
        /// Defaults to false.
        /// </param>
        /// <param name="numbersToCache">
        /// Specifies how many sequence numbers are to be preallocated and stored in memory for faster access.
        /// The minimum value is 1 (only one value can be generated at a time, i.e., no cache), and this is also the default.
        /// </param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static bool CanSetIdentityOptions(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            long? startValue = null,
            long? incrementBy = null,
            long? minValue = null,
            long? maxValue = null,
            bool? isCyclic = null,
            long? numbersToCache = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var value = new IdentitySequenceOptionsData
            {
                StartValue = startValue,
                IncrementBy = incrementBy ?? 1,
                MinValue = minValue,
                MaxValue = maxValue,
                IsCyclic = isCyclic ?? false,
                NumbersToCache = numbersToCache ?? 1
            }.Serialize();

            return propertyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.IdentityOptions, value);
        }

        #endregion Identity options

        #region Generated tsvector column

        // Note: tsvector properties can be configured with a generic API through the entity type builder

        /// <summary>
        /// Configures the property to be a full-text search tsvector column over the given properties.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <param name="includedPropertyNames">An array of property names to be included in the tsvector.</param>
        /// <returns>A builder to further configure the property.</returns>
        public static PropertyBuilder IsGeneratedTsVectorColumn(
            [NotNull] this PropertyBuilder propertyBuilder,
            [NotNull] string config,
            [NotNull] params string[] includedPropertyNames)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(config, nameof(config));
            Check.NotEmpty(includedPropertyNames, nameof(includedPropertyNames));

            propertyBuilder.HasColumnType("tsvector");
            propertyBuilder.Metadata.SetTsVectorConfig(config);
            propertyBuilder.Metadata.SetTsVectorProperties(includedPropertyNames);

            return propertyBuilder;
        }

        /// <summary>
        /// Configures the property to be a full-text search tsvector column over the given properties.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <param name="includedPropertyNames">An array of property names to be included in the tsvector.</param>
        /// <returns>A builder to further configure the property.</returns>
        public static PropertyBuilder<NpgsqlTsVector> IsGeneratedTsVectorColumn(
            [NotNull] this PropertyBuilder<NpgsqlTsVector> propertyBuilder,
            [NotNull] string config,
            [NotNull] params string[] includedPropertyNames)
            => (PropertyBuilder<NpgsqlTsVector>)IsGeneratedTsVectorColumn((PropertyBuilder)propertyBuilder, config, includedPropertyNames);

        /// <summary>
        /// Configures the property to be a full-text search tsvector column over the given properties.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <param name="includedPropertyNames">An array of property names to be included in the tsvector.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        /// The same builder instance if the configuration was applied,
        /// <c>null</c> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder IsGeneratedTsVectorColumn(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string config,
            [CanBeNull] IReadOnlyList<string> includedPropertyNames,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            if (propertyBuilder.CanSetIsGeneratedTsVector(config, includedPropertyNames, fromDataAnnotation))
            {
                propertyBuilder.HasColumnType("tsvector");
                propertyBuilder.Metadata.SetTsVectorConfig(config, fromDataAnnotation);
                propertyBuilder.Metadata.SetTsVectorProperties(includedPropertyNames, fromDataAnnotation);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the property can be configured as a full-text search tsvector column.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="config">
        /// <para>
        /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </para>
        /// <para>
        /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
        /// </para>
        /// </param>
        /// <param name="includedPropertyNames">An array of property names to be included in the tsvector.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the property can be configured as a full-text search tsvector column.</returns>
        public static bool CanSetIsGeneratedTsVector(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string config,
            [CanBeNull] IReadOnlyList<string> includedPropertyNames,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                   .Overrides(propertyBuilder.Metadata.GetTsVectorConfigConfigurationSource()) &&
                   (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                   .Overrides(propertyBuilder.Metadata.GetTsVectorPropertiesConfigurationSource())
                   ||
                   config == propertyBuilder.Metadata.GetTsVectorConfig() &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(
                       includedPropertyNames, propertyBuilder.Metadata.GetTsVectorProperties());
        }

        #endregion Generated tsvector column

        #region Collation

        /// <summary>
        /// <p>
        /// Configures the property to use the given collation. The database column will be be created with the given
        /// collation, and it will be used implicitly in all collation-sensitive operations.
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseCollation([NotNull] this PropertyBuilder propertyBuilder, [CanBeNull] string collation)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(collation, nameof(collation));

            propertyBuilder.Metadata.SetCollation(collation);

            return propertyBuilder;
        }

        /// <summary>
        /// <p>
        /// Configures the property to use the given collation. The database column will be be created with the given
        /// collation, and it will be used implicitly in all collation-sensitive operations.
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseCollation<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder, [CanBeNull] string collation)
            => (PropertyBuilder<TProperty>)UseCollation((PropertyBuilder)propertyBuilder, collation);

        /// <summary>
        /// <p>
        /// Configures the property to use the given collation. The database column will be be created with the given
        /// collation, and it will be used implicitly in all collation-sensitive operations.
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the property.</returns>
        public static IConventionPropertyBuilder UseCollation(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetUseCollation(collation, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetCollation(collation);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the given value can be set as the collation.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the given value can be set as the collation.</returns>
        public static bool CanSetUseCollation(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return propertyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.Collation, collation, fromDataAnnotation);
        }

        #endregion Collation

        #region Case-insensitive collation

        /// <summary>
        /// <p>
        /// Configures the property to use the given collation for explicitly case-insensitive operations.
        /// This does not affect the column itself, but operations such as <see cref="string.Equals(string, StringComparison)" />
        /// which explicitly specify case-insensitivity will apply this collation explicitly within the query.
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseCaseInsensitiveCollation([NotNull] this PropertyBuilder propertyBuilder, [CanBeNull] string collation)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(collation, nameof(collation));

            propertyBuilder.Metadata.SetCaseInsensitiveCollation(collation);

            return propertyBuilder;
        }

        /// <summary>
        /// <p>
        /// Configures the property to use the given collation for explicitly case-insensitive operations.
        /// This does not affect the column itself, but operations such as <see cref="string.Equals(string, StringComparison)" />
        /// which explicitly specify case-insensitivity will apply this collation explicitly within the query.
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseCaseInsensitiveCollation<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder, [CanBeNull] string collation)
            => (PropertyBuilder<TProperty>)UseCaseInsensitiveCollation((PropertyBuilder)propertyBuilder, collation);

        /// <summary>
        /// <p>
        /// Configures the property to use the given collation for explicitly case-insensitive operations.
        /// This does not affect the column itself, but operations such as <see cref="string.Equals(string, StringComparison)" />
        /// which explicitly specify case-insensitivity will apply this collation explicitly within the query.
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the property.</returns>
        public static IConventionPropertyBuilder UseCaseInsensitiveCollation(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetUseCaseInsensitiveCollation(collation, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetCaseInsensitiveCollation(collation);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the given value can be set as the case-insensitive collation.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the given value can be set as the collation.</returns>
        public static bool CanSetUseCaseInsensitiveCollation(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return propertyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.CaseInsensitiveCollation, collation, fromDataAnnotation);
        }

        #endregion Case-insensitive collation

        #region Case-sensitive collation

        /// <summary>
        /// <p>
        /// Configures the property to use the given collation for explicitly case-sensitive operations.
        /// This does not affect the column itself, but operations such as <see cref="string.Equals(string, StringComparison)" />
        /// which explicitly specify case-sensitivity will apply this collation explicitly within the query.
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseCaseSensitiveCollation([NotNull] this PropertyBuilder propertyBuilder, [CanBeNull] string collation)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(collation, nameof(collation));

            propertyBuilder.Metadata.SetCaseSensitiveCollation(collation);

            return propertyBuilder;
        }

        /// <summary>
        /// <p>
        /// Configures the property to use the given collation for explicitly case-sensitive operations.
        /// This does not affect the column itself, but operations such as <see cref="string.Equals(string, StringComparison)" />
        /// which explicitly specify case-sensitivity will apply this collation explicitly within the query.
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseCaseSensitiveCollation<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder, [CanBeNull] string collation)
            => (PropertyBuilder<TProperty>)UseCaseSensitiveCollation((PropertyBuilder)propertyBuilder, collation);

        /// <summary>
        /// <p>
        /// Configures the property to use the given collation for explicitly case-sensitive operations.
        /// This does not affect the column itself, but operations such as <see cref="string.Equals(string, StringComparison)" />
        /// which explicitly specify case-sensitivity will apply this collation explicitly within the query.
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the property.</returns>
        public static IConventionPropertyBuilder UseCaseSensitiveCollation(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetUseCaseSensitiveCollation(collation, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetCaseSensitiveCollation(collation);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the given value can be set as the case-sensitive collation.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/collation.html
        /// </remarks>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="collation">The collation.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the given value can be set as the collation.</returns>
        public static bool CanSetUseCaseSensitiveCollation(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return propertyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.CaseSensitiveCollation, collation, fromDataAnnotation);
        }

        #endregion Case-sensitive collation
    }
}
