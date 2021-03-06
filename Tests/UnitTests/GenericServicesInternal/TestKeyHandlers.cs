﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using GenericServices;
using GenericServices.Configuration;
using Xunit;
using GenericServices.Internal.Decoders;
using GenericServices.Internal.MappingCode;
using GenericServices.PublicButHidden;
using GenericServices.Setup;
using Microsoft.AspNetCore.Mvc;
using Tests.Dtos;
using Tests.EfClasses;
using Tests.EfCode;
using Tests.UnitTests.GenericServicesPublic;
using TestSupport.EfHelpers;
using Xunit.Extensions.AssertExtensions;

namespace Tests.UnitTests.GenericServicesInternal
{
    public class TestKeyHandlers
    {

        [Fact]
        public void TestNormalKeyExtract()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<TestDbContext>();
            using (var context = new TestDbContext(options))
            {
                var decodedEntity = new DecodedEntityClass(typeof(NormalEntity), context);
                var decodeDto = new DecodedDto(typeof(NormalEntityDto), decodedEntity, new GenericServicesConfig(), null);

                //ATTEMPT
                var dto = new NormalEntityDto {Id = 123};
                var keys = context.GetKeysFromDtoInCorrectOrder(dto, decodeDto);

                //VERIFY
                ((int)keys[0]).ShouldEqual(123);
            }
        }

        [Fact]
        public void TestNormalKeyCopyBack()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<TestDbContext>();
            using (var context = new TestDbContext(options))
            {
                var decodedEntity = new DecodedEntityClass(typeof(NormalEntity), context);

                //ATTEMPT
                var entity = new NormalEntity{ Id = 123 };
                var dto = new NormalEntityDto();
                entity.CopyBackKeysFromEntityToDtoIfPresent(dto, decodedEntity);

                //VERIFY
                dto.Id.ShouldEqual(123);
            }
        }

        [Fact]
        public void TestPrivateSetterKeyNotCopiedBack()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<TestDbContext>();
            using (var context = new TestDbContext(options))
            {
                var decodedEntity = new DecodedEntityClass(typeof(NormalEntity), context);

                //ATTEMPT
                var entity = new NormalEntity { Id = 123 };
                var dto = new NormalEntityKeyPrivateSetDto();
                entity.CopyBackKeysFromEntityToDtoIfPresent(dto, decodedEntity);

                //VERIFY
                dto.Id.ShouldEqual(0);
            }
        }


        [Fact]
        public void TestAbstractSetterKeyNotCopiedBack()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<TestDbContext>();
            using (var context = new TestDbContext(options))
            {
                var decodedEntity = new DecodedEntityClass(typeof(NormalEntity), context);

                //ATTEMPT
                var entity = new NormalEntity { Id = 123 };
                var dto = new NormalEntityKeyAbstractDto();
                entity.CopyBackKeysFromEntityToDtoIfPresent(dto, decodedEntity);

                //VERIFY
                dto.Id.ShouldEqual(0);
            }
        }

        [Fact]
        public void TestCompositeKeyCopyBack()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<TestDbContext>();
            using (var context = new TestDbContext(options))
            {
                var decodedEntity = new DecodedEntityClass(typeof(DddCompositeIntString), context);

                //ATTEMPT
                var entity = new DddCompositeIntString("Hello",999);
                var dto = new DddCompositeIntStringCreateDto();
                entity.CopyBackKeysFromEntityToDtoIfPresent(dto, decodedEntity);

                //VERIFY
                dto.MyString.ShouldEqual("Hello");
                dto.MyInt.ShouldEqual(999);
            }
        }

        
    }
}