﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Moq;
using NUnit.Framework;

using SetLocale.Client.Web.Entities;
using SetLocale.Client.Web.Models;
using SetLocale.Client.Web.Repositories;
using SetLocale.Client.Web.Test.Builders;

namespace SetLocale.Client.Web.Test.Services
{
    [TestFixture]
    public class ReportServiceTests
    {
        [Test]
        public async void should_create_a_homestats()
        {
            //arrange
            var model = new HomeStatsModel
            {
                ApplicationCount = 1,
                DeveloperCount = 2,
                TranslatorCount = 3,
                KeyCount = 4,
                TranslationCount = 5
            };

            var userRepository = new Mock<IRepository<User>>();
            userRepository.Setup(x => x.Count(y => y.RoleId != SetLocaleRole.Developer.Value))
                          .Returns(2);

            userRepository.Setup(x => x.Count(y => y.RoleId == SetLocaleRole.Translator.Value))
                          .Returns(3);

            var wordRepository = new Mock<IRepository<Word>>();
            wordRepository.Setup(x => x.Count(It.IsAny<Expression<Func<Word, bool>>>()))
                          .Returns(4);

            wordRepository.Setup(x => x.FindAll(It.IsAny<Expression<Func<Word, bool>>>()))
                         .Returns(new List<Word> { new Word { TranslationCount = 1 }, new Word { TranslationCount = 1 }, new Word { TranslationCount = 1 }, new Word { TranslationCount = 2 }, }.AsQueryable());

            var appRepository = new Mock<IRepository<App>>();
            appRepository.Setup(x => x.Count(It.IsAny<Expression<Func<App, bool>>>()))
                         .Returns(1);

            //act
            var sut = new ReportServiceBuilder().WithUserRepository(userRepository.Object)
                                                .WithWordRepository(wordRepository.Object)
                                                .WithAppRepository(appRepository.Object)
                                                .Build();
            var homestats = await sut.GetHomeStats();

            //assert
            Assert.AreEqual(model.ApplicationCount, homestats.ApplicationCount);
            Assert.AreEqual(model.DeveloperCount, homestats.DeveloperCount);
            Assert.AreEqual(model.TranslatorCount, homestats.TranslatorCount);
            Assert.AreEqual(model.KeyCount, homestats.KeyCount);
            Assert.AreEqual(model.TranslationCount, homestats.TranslationCount);

            appRepository.Verify(x => x.Count(It.IsAny<Expression<Func<App, bool>>>()), Times.AtLeastOnce);
            userRepository.Verify(x => x.Count(It.IsAny<Expression<Func<User, bool>>>()), Times.AtLeastOnce);
            wordRepository.Verify(x => x.Count(It.IsAny<Expression<Func<Word, bool>>>()), Times.AtLeastOnce);
            wordRepository.Verify(x => x.FindAll(It.IsAny<Expression<Func<Word, bool>>>()), Times.AtLeastOnce);
        }
    }
}
