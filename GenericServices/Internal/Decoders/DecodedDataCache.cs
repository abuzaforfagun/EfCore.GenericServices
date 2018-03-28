﻿// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using GenericServices.Configuration;
using GenericServices.Configuration.Internal;
using Microsoft.EntityFrameworkCore;

namespace GenericServices.Internal.Decoders
{
    internal static class DecodedDataCache
    {
        public static DecodedEntityClass GetUnderlyingEntityInfo(this DbContext context, Type entityOrDto)
        {
            if (EntityInfoCache.ContainsKey(entityOrDto)) return EntityInfoCache[entityOrDto];

            //If the entity type is found in the LinkToEntity interface it returns that, otherwise the called type because it must be the entity
            var entityType = entityOrDto.GetLinkedEntityFromDto() ?? entityOrDto;
            return context.GetEntityClassInfo(entityType);
        }

        public static DecodedEntityClass RegisterDecodedEntityClass(this DbContext context, Type entityType)
        {
            return context.GetEntityClassInfo(entityType);
        }

        public static DecodedEntityClass GetRegisteredEntityInfo(this Type entityType)
        {
            return EntityInfoCache.ContainsKey(entityType) ? EntityInfoCache[entityType] : null;
        }

        public static DecodedDto GetRegisteredDtoInfo(this Type dtoType)
        {
            return DecodedDtoCache.ContainsKey(dtoType) ? DecodedDtoCache[dtoType] : null;
        }

        public static DecodedDto GetDtoInfoThrowExceptionIfNotThere(this Type dtoType)
        {
            if (!DecodedDtoCache.TryGetValue(dtoType, out var result))
                   throw new NullReferenceException(
                       $"The DTO/ViewModel class {dtoType} is not registered as a valid GenericService DTO." +
                       $" Have you left off the {DecodedDtoExtensions.HumanReadableILinkToEntity} interface?");
            return result;
        }

        public static IStatusGeneric<DecodedDto> GetOrCreateDtoInfo(this Type classType, DecodedEntityClass entityInfo,
            IExpandedGlobalConfig overallConfig, PerDtoConfig perDtoConfig)
        {
            var status = new StatusGenericHandler<DecodedDto>();
            if (classType.IsPublic || classType.IsNestedPublic)
                return status.SetResult(DecodedDtoCache.GetOrAdd(classType, type => new DecodedDto(classType, entityInfo, overallConfig, perDtoConfig)));

            status.AddError($"Sorry, but the DTO/ViewModel class '{classType.Name}' must be public for GenericServices to work.");
            return status;
        }

        /// <summary>
        /// This is called by the UnitTestSetup that developers can use to set up a test environment for their unit tests
        /// </summary>
        public static void ClearDictionaryCaches()
        {
            EntityInfoCache.Clear();
            DecodedDtoCache.Clear();
        }

        //-----------------------------------------------------
        //private methods/dicts

        private static readonly ConcurrentDictionary<Type, DecodedDto> DecodedDtoCache = new ConcurrentDictionary<Type, DecodedDto>();

        private static readonly ConcurrentDictionary<Type, DecodedEntityClass> EntityInfoCache = new ConcurrentDictionary<Type, DecodedEntityClass>();

        private static DecodedEntityClass GetEntityClassInfo(this DbContext context, Type classType) 
        {
            return EntityInfoCache.GetOrAdd(classType, type => new DecodedEntityClass(classType, context));
        }

    }
}