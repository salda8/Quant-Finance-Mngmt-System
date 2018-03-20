using Common.EntityModels;
using Moq;
using NUnit.Framework;
using Server.Repositories;
using WebApi.Controllers;

namespace UnitTests.Controllers
{
    [TestFixture]
    public class CommissionMessageControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IGenericRepository<CommissionMessage>> mockGenericRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockGenericRepository = this.mockRepository.Create<IGenericRepository<CommissionMessage>>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void TestMethod1()
        {
            // Arrange


            // Act
            CommissionMessageController commissionMessageController = this.CreateCommissionMessageController();


            // Assert

        }

        private CommissionMessageController CreateCommissionMessageController()
        {
            return new CommissionMessageController(
                this.mockGenericRepository.Object);
        }
    }
}
